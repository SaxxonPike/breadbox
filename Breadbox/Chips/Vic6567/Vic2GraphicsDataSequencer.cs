using System.Linq.Expressions;
using Breadbox.Extensions;

namespace Breadbox.Chips.Vic6567
{
    /// <summary>
    /// Emulates graphics data sequencer (non-sprite).
    /// </summary>
    public static class Vic2GraphicsDataSequencer
    {
        public static Expression Load(MemberExpression graphicsBuffer, MemberExpression graphicsMultiColorFlipFlop, Expression value)
        {
            return Expression.Block(
                Expression.Assign(graphicsBuffer, value),
                Util.Reset(graphicsMultiColorFlipFlop));
        }

        public static Expression Shift(MemberExpression graphicsBuffer, MemberExpression graphicsMultiColorFlipFlop, Expression mcm, Expression bmm, Expression graphicsColor)
        {
            return Expression.IfThenElse(IsMulticolorOutput(mcm, bmm, graphicsColor),
                Expression.Block(
                    Expression.IfThen(graphicsMultiColorFlipFlop, Expression.LeftShiftAssign(graphicsBuffer, Expression.Constant(2))),
                    Expression.Assign(graphicsMultiColorFlipFlop, Expression.Not(graphicsMultiColorFlipFlop))),
                Expression.LeftShiftAssign(graphicsBuffer, Expression.Constant(1)));
        }

        private static Expression HasMulticolorBitSet(Expression graphicsColor)
        {
            return Expression.NotEqual(Expression.And(graphicsColor, Expression.Constant(0x80)), Expression.Constant(0));
        }

        private static Expression StandardTextModeOutputColor(Expression graphicsBuffer, Expression b0c, Expression graphicsColor)
        {
            return Expression.Switch(DataStandard(graphicsBuffer), Expression.Constant(0),
                Expression.SwitchCase(b0c, Expression.Constant(0x0)),
                Expression.SwitchCase(Expression.And(Expression.RightShift(graphicsColor, Expression.Constant(8)), Expression.Constant(0xF)), Expression.Constant(0x3)));
        }

        private static Expression MulticolorTextModeOutputColor(Expression graphicsBuffer, Expression b0c, Expression b1c, Expression b2c,
            Expression graphicsColor)
        {
            var upperBitsOfColorData = Expression.And(Expression.RightShift(graphicsColor, Expression.Constant(8)), Expression.Constant(0x7));
            return Expression.Condition(HasMulticolorBitSet(graphicsColor),
                Expression.Switch(DataMulticolor(graphicsBuffer), Expression.Constant(0),
                    Expression.SwitchCase(b0c, Expression.Constant(0x0)),
                    Expression.SwitchCase(b1c, Expression.Constant(0x1)),
                    Expression.SwitchCase(b2c, Expression.Constant(0x2)),
                    Expression.SwitchCase(upperBitsOfColorData, Expression.Constant(0x3))),
                Expression.Switch(DataStandard(graphicsBuffer), Expression.Constant(0),
                    Expression.SwitchCase(b0c, Expression.Constant(0x0)),
                    Expression.SwitchCase(upperBitsOfColorData, Expression.Constant(0x2))));
        }

        private static Expression StandardBitmapModeOutputColor(Expression graphicsBuffer, Expression graphicsColor)
        {
            return Expression.Switch(DataStandard(graphicsBuffer), Expression.Constant(0),
                Expression.SwitchCase(Expression.And(graphicsColor, Expression.Constant(0xF)), Expression.Constant(0x0)),
                Expression.SwitchCase(Expression.And(Expression.RightShift(graphicsColor, Expression.Constant(4)), Expression.Constant(0xF)), Expression.Constant(0x2)));
        }

        private static Expression MulticolorBitmapModeOutputColor(Expression graphicsBuffer, Expression b0c, Expression graphicsColor)
        {
            return Expression.Switch(DataMulticolor(graphicsBuffer), Expression.Constant(0),
                Expression.SwitchCase(b0c, Expression.Constant(0x0)),
                Expression.SwitchCase(Expression.And(Expression.RightShift(graphicsColor, Expression.Constant(4)), Expression.Constant(0xF)), Expression.Constant(0x1)),
                Expression.SwitchCase(Expression.And(graphicsColor, Expression.Constant(0xF)), Expression.Constant(0x2)),
                Expression.SwitchCase(Expression.And(Expression.RightShift(graphicsColor, Expression.Constant(8)), Expression.Constant(0xF)), Expression.Constant(0x3)));
        }

        private static Expression ExtraColorModeOutputColor(Expression graphicsBuffer, Expression b0c, Expression b1c, Expression b2c, Expression b3c,
            Expression graphicsColor)
        {
            return Expression.Switch(DataStandard(graphicsBuffer), Expression.Constant(0),
                Expression.SwitchCase(ExtraColorModeOutputColorSwitch(b0c, b1c, b2c, b3c, graphicsColor), Expression.Constant(0x0)),
                Expression.SwitchCase(Expression.And(Expression.RightShift(graphicsColor, Expression.Constant(8)), Expression.Constant(0xF)), Expression.Constant(0x2)));
        }

        private static Expression ExtraColorModeOutputColorSwitch(Expression b0c, Expression b1c, Expression b2c,
            Expression b3c, Expression color)
        {
            return Expression.Switch(Expression.And(color, Expression.Constant(0xC0)), Expression.Constant(0),
                Expression.SwitchCase(b0c, Expression.Constant(0x00)),
                Expression.SwitchCase(b1c, Expression.Constant(0x40)),
                Expression.SwitchCase(b2c, Expression.Constant(0x80)),
                Expression.SwitchCase(b3c, Expression.Constant(0xC0)));
        }

        private static Expression IsMulticolorOutput(Expression mcm, Expression bmm, Expression color)
        {
            return Expression.AndAlso(mcm, Expression.OrElse(bmm, HasMulticolorBitSet(color)));
        }

        private static Expression DataStandard(Expression graphicsBuffer)
        {
            return Expression.RightShift(Expression.And(graphicsBuffer, Expression.Constant(0x80)), Expression.Constant(6));
        }

        private static Expression DataMulticolor(Expression graphicsBuffer)
        {
            return Expression.RightShift(Expression.And(graphicsBuffer, Expression.Constant(0xC0)), Expression.Constant(6));
        }

        public static Expression OutputData(Expression graphicsBuffer, Expression bmm, Expression mcm, Expression color)
        {
            return Expression.Condition(IsMulticolorOutput(mcm, bmm, color), DataMulticolor(graphicsBuffer), DataStandard(graphicsBuffer));
        }

        private static Expression OutputColorEcmEnabled(Expression graphicsBuffer, Expression b0c, Expression b1c, Expression b2c, Expression b3c, Expression color, Expression bmm, Expression mcm)
        {
            return Expression.Condition(Expression.OrElse(bmm, mcm),
                Expression.Constant(0), ExtraColorModeOutputColor(graphicsBuffer, b0c, b1c, b2c, b3c, color));
        }

        private static Expression OutputColorEcmDisabled(Expression graphicsBuffer, Expression b0c, Expression b1c, Expression b2c, Expression graphicsColor, Expression bmm, Expression mcm)
        {
            return Expression.Condition(mcm,
                Expression.Condition(bmm,
                    MulticolorBitmapModeOutputColor(graphicsBuffer, b0c, graphicsColor),
                    MulticolorTextModeOutputColor(graphicsBuffer, b0c, b1c, b2c, graphicsColor)),
                Expression.Condition(bmm,
                    StandardBitmapModeOutputColor(graphicsBuffer, graphicsColor),
                    StandardTextModeOutputColor(graphicsBuffer, b0c, graphicsColor)));
        }

        public static Expression OutputColor(Expression graphicsBuffer,
            Expression b0c, Expression b1c, Expression b2c, Expression b3c,
            Expression graphicsColor, Expression bmm, Expression ecm, Expression mcm)
        {
            return Expression.Condition(ecm,
                OutputColorEcmEnabled(graphicsBuffer, b0c, b1c, b2c, b3c, graphicsColor, bmm, mcm),
                OutputColorEcmDisabled(graphicsBuffer, b0c, b1c, b2c, graphicsColor, bmm, mcm));
        }
    }
}
