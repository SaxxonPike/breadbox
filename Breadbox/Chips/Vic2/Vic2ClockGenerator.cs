using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Extensions;

namespace Breadbox.Chips.Vic2
{
    public static class Vic2ClockGenerator
    {
        private static Expression ShouldEnableBadLines(Expression rasterY, Expression den)
        {
            return Expression.AndAlso(den, Expression.Equal(rasterY, Expression.Constant(0x30)));
        }

        private static Expression IsRasterWithinBadlineRange(Expression rasterY)
        {
            return Expression.AndAlso(Expression.GreaterThanOrEqual(rasterY, Expression.Constant(0x30)),
                Expression.LessThanOrEqual(rasterY, Expression.Constant(0xF7)));
        }

        private static Expression IsRasterMatchingYscroll(Expression rasterY, Expression yscroll)
        {
            return Expression.Equal(Expression.And(rasterY, Expression.Constant(0x7)), yscroll);
        }

        private static Expression IsBadLine(Expression rasterY, Expression badLineEnable, Expression yscroll)
        {
            return Expression.AndAlso(badLineEnable, Expression.AndAlso(IsRasterMatchingYscroll(rasterY, yscroll), IsRasterWithinBadlineRange(rasterY)));
        }

        private static Expression Cycle14VmliClear(MemberExpression vc, MemberExpression vmli, MemberExpression rc,
            Expression vcbase)
        {
            return Expression.Block(
                Vic2VideoMatrixCounter.LoadValueFromBase(vcbase, vc, vmli),
                Vic2RowCounter.Reset(rc),
                Expression.Empty()
                );
        }

        private static Expression Cycle58RcIncrement(MemberExpression idle, MemberExpression rc, MemberExpression vc,
            MemberExpression vcbase)
        {
            return Expression.Block(
                Expression.IfThen(Expression.Equal(rc, Expression.Constant(0x7)), Expression.Block(
                    Util.Set(idle),
                    Vic2VideoMatrixCounter.StoreValueToBase(vcbase, vc)
                    )),
                Expression.IfThen(Expression.Not(idle), Vic2RowCounter.Increment(rc)),
                Expression.Empty()
                );
        }

        private static Expression ClockNegativeEdgePhi0(
            Expression rasterY,
            MemberExpression badLineEnable,
            Expression den,
            Expression yscroll,
            MemberExpression badLine,
            MemberExpression aec,
            Expression ba,
            Expression baCounter,
            Expression clockAddress)
        {
            // First Phase
            return Expression.Block(
                Expression.IfThen(Expression.Not(badLineEnable), Expression.Assign(badLineEnable, ShouldEnableBadLines(rasterY, den))),
                Expression.IfThen(Expression.AndAlso(badLineEnable, Expression.AndAlso(IsRasterWithinBadlineRange(rasterY), IsRasterMatchingYscroll(rasterY, yscroll))), Util.Set(badLine)),
                clockAddress,
                Expression.Assign(badLine, IsBadLine(rasterY, badLineEnable, yscroll)),
                Vic2AecSignal.ClockNegativeEdge(aec, ba, baCounter),
                Expression.Empty()
                );
        }

        private static Expression ClockPositiveEdgePhi0(
            MemberExpression aec,
            Expression ba,
            Expression baCounter,
            Expression clockAddress)
        {
            // Second Phase
            return Expression.Block(
                clockAddress,
                Vic2AecSignal.ClockPositiveEdge(aec, ba, baCounter),
                Expression.Empty()
                );
        }

        public static Expression Clock(
            MemberExpression rasterX,
            MemberExpression rasterY,
            ConstantExpression width,
            Expression lines,
            MemberExpression badLineEnable,
            MemberExpression badLine,
            Expression den,
            Expression yscroll,
            MemberExpression ba,
            MemberExpression aec,
            Expression baCounter,
            MemberExpression fetchCounter,
            ConstantExpression sprite0BaX,
            MemberExpression address,
            IList<MemberExpression> mobDma,
            Expression vm,
            IList<MemberExpression> mobPointer,
            IList<MemberExpression> mobMc,
            Expression bmm,
            Expression ecm,
            MemberExpression idle,
            Expression cb,
            MemberExpression vc,
            MemberExpression rc,
            Func<Expression, Expression> videoMemory,
            MemberExpression refreshCounter,
            MemberExpression vmli,
            MemberExpression vcbase)
        {
            var clockAddress = Vic2AddressGenerator.Clock(fetchCounter, rasterX, sprite0BaX, ba, address, badLine, mobDma, vm, mobPointer, mobMc, bmm, ecm, idle, cb, vc, rc, videoMemory, refreshCounter, vmli, width);
            var negativePhi0 = ClockNegativeEdgePhi0(rasterY, badLineEnable, den, yscroll, badLine, aec, ba, baCounter, clockAddress);
            var positivePhi0 = ClockPositiveEdgePhi0(aec, ba, baCounter, clockAddress);

            return Expression.Block(
                Vic2RasterCounter.Clock(rasterX, rasterY, width, lines),
                Expression.Switch(rasterX,
                    Expression.SwitchCase(Cycle14VmliClear(vc, vmli, rc, vcbase), Expression.Constant(0x000)),
                    Expression.SwitchCase(Cycle58RcIncrement(idle, rc, vc, vcbase), Expression.Constant(((int)sprite0BaX.Value) + 24))
                ),
                Expression.Switch(Expression.And(rasterX, Expression.Constant(0x7)),
                    Expression.SwitchCase(negativePhi0, Expression.Constant(0x0)),
                    Expression.SwitchCase(positivePhi0, Expression.Constant(0x4)))
                );
        }
    }
}
