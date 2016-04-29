using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Extensions;

namespace Breadbox.Chips.Vic6567
{
    /// <summary>
    /// Emulates graphics data sequencer (non-sprite).
    /// </summary>
    public class GraphicsDataSequencer
    {
        private int _buffer;
        private int _multiColorFlipFlop;

        private Expression Buffer
        {
            get { return Util.Member(() => _buffer); }
        }

        private Expression MultiColorFlipFlop
        {
            get { return Util.Member(() => _multiColorFlipFlop); }
        }

        public Expression LoadBuffer(Expression value)
        {
            return Expression.Block(
                Expression.Assign(Buffer, value),
                Expression.Assign(MultiColorFlipFlop, Expression.Constant(false)));
        }

        public Expression Shift(Expression mcm, Expression bmm, Expression color)
        {
            return Expression.IfThenElse(IsMulticolorOutput(mcm, bmm, color),
                Expression.Block(
                    Expression.IfThen(MultiColorFlipFlop, Expression.LeftShiftAssign(Buffer, Expression.Constant(2))),
                    Expression.Assign(MultiColorFlipFlop, Expression.Not(MultiColorFlipFlop))),
                Expression.LeftShiftAssign(Buffer, Expression.Constant(1)));
        }

        private Expression HasMulticolorBitSet(Expression color)
        {
            return Expression.NotEqual(Expression.And(color, Expression.Constant(0x80)), Expression.Constant(0));
        }

        private Expression StandardTextModeOutputColor(Expression b0c, Expression color)
        {
            return Expression.Switch(DataStandard, Expression.Constant(0),
                Expression.SwitchCase(b0c, Expression.Constant(0x0)),
                Expression.SwitchCase(Expression.And(Expression.RightShift(color, Expression.Constant(8)), Expression.Constant(0xF)), Expression.Constant(0x3)));
        }

        private Expression MulticolorTextModeOutputColor(Expression b0c, Expression b1c, Expression b2c,
            Expression color)
        {
            var upperBitsOfColorData = Expression.And(Expression.RightShift(color, Expression.Constant(8)), Expression.Constant(0x7));
            return Expression.Condition(HasMulticolorBitSet(color),
                Expression.Switch(DataMulticolor, Expression.Constant(0),
                    Expression.SwitchCase(b0c, Expression.Constant(0x0)),
                    Expression.SwitchCase(b1c, Expression.Constant(0x1)),
                    Expression.SwitchCase(b2c, Expression.Constant(0x2)),
                    Expression.SwitchCase(upperBitsOfColorData, Expression.Constant(0x3))),
                Expression.Switch(DataStandard, Expression.Constant(0),
                    Expression.SwitchCase(b0c, Expression.Constant(0x0)),
                    Expression.SwitchCase(upperBitsOfColorData, Expression.Constant(0x2))));
        }

        private Expression StandardBitmapModeOutputColor(Expression color)
        {
            return Expression.Switch(DataStandard, Expression.Constant(0),
                Expression.SwitchCase(Expression.And(color, Expression.Constant(0xF)), Expression.Constant(0x0)),
                Expression.SwitchCase(Expression.And(Expression.RightShift(color, Expression.Constant(4)), Expression.Constant(0xF)), Expression.Constant(0x2)));
        }

        private Expression MulticolorBitmapModeOutputColor(Expression b0c, Expression color)
        {
            return Expression.Switch(DataMulticolor, Expression.Constant(0),
                Expression.SwitchCase(b0c, Expression.Constant(0x0)),
                Expression.SwitchCase(Expression.And(Expression.RightShift(color, Expression.Constant(4)), Expression.Constant(0xF)), Expression.Constant(0x1)),
                Expression.SwitchCase(Expression.And(color, Expression.Constant(0xF)), Expression.Constant(0x2)),
                Expression.SwitchCase(Expression.And(Expression.RightShift(color, Expression.Constant(8)), Expression.Constant(0xF)), Expression.Constant(0x3)));
        }

        private Expression ExtraColorModeOutputColor(Expression b0c, Expression b1c, Expression b2c, Expression b3c,
            Expression color)
        {
            return Expression.Switch(DataStandard, Expression.Constant(0),
                Expression.SwitchCase(ExtraColorModeOutputColorSwitch(b0c, b1c, b2c, b3c, color), Expression.Constant(0x0)),
                Expression.SwitchCase(Expression.And(Expression.RightShift(color, Expression.Constant(8)), Expression.Constant(0xF)), Expression.Constant(0x2)));
        }

        private Expression ExtraColorModeOutputColorSwitch(Expression b0c, Expression b1c, Expression b2c,
            Expression b3c, Expression color)
        {
            return Expression.Switch(Expression.And(color, Expression.Constant(0xC0)), Expression.Constant(0),
                Expression.SwitchCase(b0c, Expression.Constant(0x00)),
                Expression.SwitchCase(b1c, Expression.Constant(0x40)),
                Expression.SwitchCase(b2c, Expression.Constant(0x80)),
                Expression.SwitchCase(b3c, Expression.Constant(0xC0)));
        }

        private Expression IsMulticolorOutput(Expression mcm, Expression bmm, Expression color)
        {
            return Expression.AndAlso(mcm, Expression.OrElse(bmm, HasMulticolorBitSet(color)));
        }

        private Expression DataStandard
        {
            get
            {
                return Expression.RightShift(Expression.And(Buffer, Expression.Constant(0x80)), Expression.Constant(6));
            }
        }

        private Expression DataMulticolor
        {
            get
            {
                return Expression.RightShift(Expression.And(Buffer, Expression.Constant(0xC0)), Expression.Constant(6));
            }
        }

        public Expression OutputData(Expression bmm, Expression mcm, Expression color)
        {
            return Expression.Condition(IsMulticolorOutput(mcm, bmm, color), DataMulticolor, DataStandard);
        }

        private Expression OutputColorEcmEnabled(Expression b0c, Expression b1c, Expression b2c, Expression b3c, Expression color, Expression bmm, Expression mcm)
        {
            return Expression.Condition(Expression.OrElse(bmm, mcm),
                Expression.Constant(0), ExtraColorModeOutputColor(b0c, b1c, b2c, b3c, color));
        }

        private Expression OutputColorEcmDisabled(Expression b0c, Expression b1c, Expression b2c, Expression color, Expression bmm, Expression mcm)
        {
            return Expression.Condition(mcm,
                Expression.Condition(bmm,
                    MulticolorBitmapModeOutputColor(b0c, color),
                    MulticolorTextModeOutputColor(b0c, b1c, b2c, color)),
                Expression.Condition(bmm,
                    StandardBitmapModeOutputColor(color),
                    StandardTextModeOutputColor(b0c, color)));
        }

        public Expression OutputColor(
            Expression b0c, Expression b1c, Expression b2c, Expression b3c,
            Expression color, Expression bmm, Expression ecm, Expression mcm)
        {
            return Expression.Condition(ecm,
                OutputColorEcmEnabled(b0c, b1c, b2c, b3c, color, bmm, mcm),
                OutputColorEcmDisabled(b0c, b1c, b2c, color, bmm, mcm));
        }
    }
}
