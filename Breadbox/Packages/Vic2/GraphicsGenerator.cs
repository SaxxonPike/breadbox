using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Breadbox.Packages.Vic2
{
    public class GraphicsGenerator
    {
        private readonly State _state;
        private readonly Config _config;

        public GraphicsGenerator(State state, Config config)
        {
            _state = state;
            _config = config;
        }

        public Expression Clock
        {
            get
            {
                var isEvenRasterX = Expression.Equal(Expression.And(_state.RASTERXC, Expression.Constant(0x1)), Expression.Constant(0));
                return Expression.LeftShiftAssign(_state.GD, Expression.Condition(CharacterIsMultiColor, Expression.Condition(isEvenRasterX, Expression.Constant(2), Expression.Constant(0)), Expression.Constant(1)));
            }
        }

        private Expression CharacterIsMultiColor
        {
            get
            {
                var matrixData = _state.CBUFFER;
                return Util.All(_state.MCM, Expression.NotEqual(Expression.And(matrixData, Expression.Constant(0x800)), Expression.Constant(0)));
            }
        }

        private Expression CharacterColor
        {
            get
            {
                var matrixData = _state.CBUFFER;
                var highMatrixData = Expression.RightShift(matrixData, Expression.Constant(8));
                var singleColor =
                    Expression.Condition(
                        Expression.NotEqual(Expression.And(_state.GD, Expression.Constant(0x80)), Expression.Constant(0)),
                        highMatrixData, _state.B0C);
                var multiColor = Expression.Switch(Expression.And(_state.GD, Expression.Constant(0xC0)), _state.B0C,
                    Expression.SwitchCase(_state.B1C, Expression.Constant(0x40)),
                    Expression.SwitchCase(_state.B2C, Expression.Constant(0x80)),
                    Expression.SwitchCase(Expression.And(highMatrixData, Expression.Constant(0x7)), Expression.Constant(0xC0))
                    );
                return Expression.Condition(CharacterIsMultiColor, multiColor, singleColor);
            }
        }

        private Expression BitmapColor
        {
            get
            {
                var matrixData = _state.CBUFFER;
                var lowMatrixData = Expression.And(matrixData, Expression.Constant(0xF));
                var midMatrixData = Expression.And(Expression.RightShift(matrixData, Expression.Constant(4)), Expression.Constant(0xF));
                var highMatrixData = Expression.RightShift(matrixData, Expression.Constant(8));
                var singleColor =
                    Expression.Condition(
                        Expression.NotEqual(Expression.And(_state.GD, Expression.Constant(0x80)), Expression.Constant(0)),
                        midMatrixData, lowMatrixData);
                var multiColor = Expression.Switch(Expression.And(_state.GD, Expression.Constant(0xC0)), _state.B0C,
                    Expression.SwitchCase(midMatrixData, Expression.Constant(0x40)),
                    Expression.SwitchCase(lowMatrixData, Expression.Constant(0x80)),
                    Expression.SwitchCase(highMatrixData, Expression.Constant(0xC0))
                    );
                return Expression.Condition(_state.MCM, multiColor, singleColor);
            }
        }

        private Expression ExtraColor
        {
            get
            {
                var matrixData = _state.CBUFFER;
                var backgroundColorIndex = Expression.And(matrixData, Expression.Constant(0xC0));
                var backgroundColor = Expression.Switch(backgroundColorIndex, _state.B0C,
                    Expression.SwitchCase(_state.B1C, Expression.Constant(0x40)),
                    Expression.SwitchCase(_state.B2C, Expression.Constant(0x80)),
                    Expression.SwitchCase(_state.B3C, Expression.Constant(0xC0)));
                var foregroundColor = Expression.RightShift(matrixData, Expression.Constant(8));
                return Expression.Condition(
                    Expression.NotEqual(Expression.And(_state.GD, Expression.Constant(0x80)), Expression.Constant(0)),
                    foregroundColor, backgroundColor);
            }
        }

        public Expression OutputData
        {
            get
            {
                return Expression.And(_state.GD, Expression.Condition(_state.MCM, Expression.Constant(0xC0), Expression.Constant(0x80)));
            }
        }

        public Expression OutputColor
        {
            get
            {
                var isInvalid = Expression.AndAlso(_state.ECM, Expression.OrElse(_state.MCM, _state.BMM));
                return Expression.Condition(isInvalid, Expression.Constant(0),
                    Expression.Condition(_state.BMM, BitmapColor,
                        Expression.Condition(_state.ECM, ExtraColor, CharacterColor)));
            }
        }
    }
}
