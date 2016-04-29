using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Extensions;

namespace Breadbox.Chips.Vic6567
{
    /// <summary>
    /// Emulates "24 bit sprite data buffer" and "sprite data sequencer".
    /// </summary>
    public class MobDataSequencer
    {
        private int _buffer;
        private bool _multiColorFlipFlop;
        private bool _doubleWidthFlipFlop;

        private MemberExpression Buffer
        {
            get { return Util.Member(() => _buffer); }
        }

        private MemberExpression MultiColorFlipFlop
        {
            get { return Util.Member(() => _multiColorFlipFlop); }
        }

        private MemberExpression DoubleWidthFlipFlop
        {
            get { return Util.Member(() => _doubleWidthFlipFlop); }
        }

        public Expression Shift(Expression isMultiColor, Expression isWide)
        {
            var singleColorShift = Expression.LeftShift(Buffer, Expression.Constant(1));
            var multiColorShift = Expression.Block(
                Expression.IfThen(MultiColorFlipFlop, Expression.LeftShiftAssign(Buffer, Expression.Constant(2))),
                Expression.Assign(MultiColorFlipFlop, Expression.Not(MultiColorFlipFlop)));
            var shiftEnabled = Expression.OrElse(Expression.Not(isWide), MultiColorFlipFlop);

            return Expression.Block(
                Expression.IfThen(shiftEnabled, Expression.IfThenElse(isMultiColor, multiColorShift, singleColorShift)),
                Expression.IfThen(isWide, Expression.Assign(DoubleWidthFlipFlop, Expression.Not(DoubleWidthFlipFlop))));
        }

        public Expression OutputData(Expression isMultiColor)
        {
            return
                Expression.And(Expression.Condition(isMultiColor, Expression.Constant(0x3), Expression.Constant(0x2)),
                    Expression.RightShift(Buffer, Expression.Constant(22)));
        }

        private Expression OutputSingleColor(Expression color)
        {
            return
                Expression.Condition(
                    Expression.NotEqual(Expression.Constant(0), Expression.And(Buffer, Expression.Constant(0x800000))),
                    color, Expression.Constant(0));
        }

        private Expression OutputMultiColor(Expression color, Expression mm0, Expression mm1)
        {
            return Expression.Switch(Expression.And(Buffer, Expression.Constant(0xC00000)), Expression.Constant(0x0),
                Expression.SwitchCase(mm0, Expression.Constant(0x400000)),
                Expression.SwitchCase(color, Expression.Constant(0x800000)),
                Expression.SwitchCase(mm1, Expression.Constant(0xC00000))
                );
        }

        public Expression OutputColor(Expression isMultiColor, Expression color, Expression mm0, Expression mm1)
        {
            return Expression.Condition(isMultiColor, OutputMultiColor(color, mm0, mm1), OutputSingleColor(color));
        }

        public Expression LoadLowerBits(Expression value)
        {
            return Expression.OrAssign(Buffer, Expression.And(value, Expression.Constant(0xFF)));
        }

        public Expression LoadMidBits(Expression value)
        {
            return Expression.OrAssign(Buffer, Expression.LeftShift(Expression.And(value, Expression.Constant(0xFF)), Expression.Constant(8)));
        }

        public Expression LoadUpperBits(Expression value)
        {
            return Expression.OrAssign(Buffer, Expression.LeftShift(Expression.And(value, Expression.Constant(0xFF)), Expression.Constant(16)));
        }
    }
}
