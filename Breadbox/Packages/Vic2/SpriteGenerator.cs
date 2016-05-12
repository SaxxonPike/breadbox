using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Breadbox.Packages.Vic2
{
    public class SpriteGenerator
    {
        private readonly State _state;
        private readonly Config _config;

        private readonly Expression[] _outputColor;
        private readonly Expression[] _outputData;
        private readonly Expression[] _clock;

        public SpriteGenerator(State state, Config config)
        {
            _state = state;
            _config = config;

            _outputColor = Enumerable.Range(0, 8).Select(GetOutputColor).ToArray();
            _outputData = Enumerable.Range(0, 8).Select(GetOutputData).ToArray();
            _clock = Enumerable.Range(0, 8).Select(GetClock).ToArray();
        }

        public Expression Clock(int spriteNumber)
        {
            return _clock[spriteNumber];
        }

        public Expression OutputColor(int spriteNumber)
        {
            return _outputColor[spriteNumber];
        }

        public Expression OutputData(int spriteNumber)
        {
            return _outputData[spriteNumber];
        }

        private Expression GetClock(int spriteNumber)
        {
            var multicolor = _state.MnMC[spriteNumber];
            var mxmc = _state.MXMCn[spriteNumber];
            var newMxmc = Expression.Assign(mxmc,Expression.Not(mxmc));
            var shiftAssign = Expression.LeftShiftAssign(_state.MDn[spriteNumber],
                Expression.Condition(multicolor,
                    Expression.Condition(newMxmc, Expression.Constant(2), Expression.Constant(0)),
                    Expression.Constant(1)));
            return Util.Invoke(Expression.IfThen(_state.MSREn[spriteNumber], shiftAssign));
        }

        private Expression GetOutputColor(int spriteNumber)
        {
            var mobData = _state.MDn[spriteNumber];
            var multiColorSprite = _state.MnMC[spriteNumber];
            var foregroundColor = _state.MnC[spriteNumber];
            var singleColor =
                Expression.Condition(
                    Expression.NotEqual(Expression.Constant(0), Expression.And(mobData, Expression.Constant(0x800000))),
                    foregroundColor, Expression.Constant(0));
            var multiColor = Expression.Switch(Expression.And(mobData, Expression.Constant(0xC00000)),
                Expression.Constant(0),
                Expression.SwitchCase(_state.MM0, Expression.Constant(0x400000)),
                Expression.SwitchCase(foregroundColor, Expression.Constant(0x800000)),
                Expression.SwitchCase(_state.MM1, Expression.Constant(0xC00000)));
            return Util.Invoke(Expression.Condition(multiColorSprite, multiColor, singleColor));
        }

        private Expression GetOutputData(int spriteNumber)
        {
            var mobData = _state.MDn[spriteNumber];
            var multiColorSprite = _state.MnMC[spriteNumber];
            return Util.Invoke(Expression.And(mobData, Expression.Condition(multiColorSprite, Expression.Constant(0xC00000), Expression.Constant(0x800000))));
        }
    }
}
