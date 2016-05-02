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

        private static Expression ClockNegativeEdgePhi0(
            Expression rasterY,
            Expression badLineEnable,
            Expression den,
            Expression yscroll,
            Expression badLine)
        {
            return Expression.Block(
                Expression.IfThen(Expression.Not(badLineEnable), Expression.Assign(badLineEnable, ShouldEnableBadLines(rasterY, den))),
                Expression.IfThen(Expression.AndAlso(badLineEnable, Expression.AndAlso(IsRasterWithinBadlineRange(rasterY), IsRasterMatchingYscroll(rasterY, yscroll))), Util.Set(badLine))
                );
        }

        public static Expression Clock(
            MemberExpression rasterX,
            MemberExpression rasterY,
            Expression width,
            Expression lines,
            Expression badLineEnable)
        {
            return Expression.Block(
                Vic2RasterCounter.Clock(rasterX, rasterY, width, lines)
                );
        }
    }
}
