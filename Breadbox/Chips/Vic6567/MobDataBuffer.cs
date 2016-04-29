using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Extensions;

namespace Breadbox.Chips.Vic6567
{
    /// <summary>
    /// Emulates "8x24 bit sprite data buffer".
    /// </summary>
    public class MobDataBuffer
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
            return Expression.Block(
                Expression.IfThen(Expression.AndAlso(MultiColorFlipFlop, DoubleWidthFlipFlop),
                    Expression.LeftShiftAssign(Buffer, Expression.IfThenElse(isMultiColor, Expression.Constant(2), Expression.Constant(1)))),
                Expression.IfThen(isMultiColor, Expression.Assign(MultiColorFlipFlop, Expression.Not(MultiColorFlipFlop))),
                Expression.IfThen(isWide, Expression.Assign(DoubleWidthFlipFlop, Expression.Not(DoubleWidthFlipFlop)))
                );
        }

        public Expression Value(Expression isMultiColor)
        {
            return
                Expression.And(Expression.IfThenElse(isMultiColor, Expression.Constant(0x3), Expression.Constant(0x2)),
                    Expression.RightShift(Buffer, Expression.Constant(22)));
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

        public Expression Reset(Expression isMultiColor, Expression isWide)
        {
            return Expression.Block(
                Expression.Assign(Util.Member(() => MultiColorFlipFlop), Expression.Not(isMultiColor)),
                Expression.Assign(Util.Member(() => DoubleWidthFlipFlop), Expression.Not(isWide)));
        }
    }
}
