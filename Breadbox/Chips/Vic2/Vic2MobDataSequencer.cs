using System.Linq.Expressions;

namespace Breadbox.Chips.Vic2
{
    /// <summary>
    /// Emulates "24 bit sprite data buffer" and "sprite data sequencer".
    /// </summary>
    public static class Vic2MobDataSequencer
    {
        public static Expression Shift(MemberExpression mobBuffer, MemberExpression mobMultiColorFlipFlop, MemberExpression mobDoubleWidthFlipFlop, Expression isMultiColor, Expression isWide)
        {
            var singleColorShift = Expression.LeftShift(mobBuffer, Expression.Constant(1));
            var multiColorShift = Expression.Block(
                Expression.IfThen(mobMultiColorFlipFlop, Expression.LeftShiftAssign(mobBuffer, Expression.Constant(2))),
                Expression.Assign(mobMultiColorFlipFlop, Expression.Not(mobMultiColorFlipFlop)));
            var shiftEnabled = Expression.OrElse(Expression.Not(isWide), mobMultiColorFlipFlop);

            return Expression.Block(
                Expression.IfThen(shiftEnabled, Expression.IfThenElse(isMultiColor, multiColorShift, singleColorShift)),
                Expression.IfThen(isWide, Expression.Assign(mobDoubleWidthFlipFlop, Expression.Not(mobDoubleWidthFlipFlop))));
        }

        public static Expression OutputData(MemberExpression mobBuffer, Expression isMultiColor)
        {
            return
                Expression.And(Expression.Condition(isMultiColor, Expression.Constant(0x3), Expression.Constant(0x2)),
                    Expression.RightShift(mobBuffer, Expression.Constant(22)));
        }

        private static Expression OutputSingleColor(Expression mobBuffer, Expression color)
        {
            return
                Expression.Condition(
                    Expression.NotEqual(Expression.Constant(0), Expression.And(mobBuffer, Expression.Constant(0x800000))),
                    color, Expression.Constant(0));
        }

        private static Expression OutputMultiColor(Expression mobBuffer, Expression mobColor, Expression mm0, Expression mm1)
        {
            return Expression.Switch(Expression.And(mobBuffer, Expression.Constant(0xC00000)), Expression.Constant(0x0),
                Expression.SwitchCase(mm0, Expression.Constant(0x400000)),
                Expression.SwitchCase(mobColor, Expression.Constant(0x800000)),
                Expression.SwitchCase(mm1, Expression.Constant(0xC00000))
                );
        }

        public static Expression OutputColor(MemberExpression mobBuffer, Expression isMultiColor, Expression mobColor, Expression mm0, Expression mm1)
        {
            return Expression.Condition(isMultiColor, OutputMultiColor(mobBuffer, mobColor, mm0, mm1), OutputSingleColor(mobBuffer, mobColor));
        }

        public static Expression LoadLowerBits(MemberExpression mobBuffer, Expression value)
        {
            return Expression.OrAssign(mobBuffer, Expression.And(value, Expression.Constant(0xFF)));
        }

        public static Expression LoadMidBits(MemberExpression mobBuffer, Expression value)
        {
            return Expression.OrAssign(mobBuffer, Expression.LeftShift(Expression.And(value, Expression.Constant(0xFF)), Expression.Constant(8)));
        }

        public static Expression LoadUpperBits(MemberExpression mobBuffer, Expression value)
        {
            return Expression.OrAssign(mobBuffer, Expression.LeftShift(Expression.And(value, Expression.Constant(0xFF)), Expression.Constant(16)));
        }
    }
}
