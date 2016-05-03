using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Breadbox.Chips.Vic2
{
    public static class Vic2AecSignal
    {
        public static Expression ClockPositiveEdge(MemberExpression aec, Expression ba, Expression baCounter)
        {
            return Expression.Assign(aec, Expression.OrElse(ba, Expression.GreaterThan(baCounter, Expression.Constant(0))));
        }

        public static Expression ClockNegativeEdge(MemberExpression aec, Expression ba, Expression baCounter)
        {
            return Expression.Assign(aec, Expression.Constant(false));
        }
    }
}
