using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Extensions;

namespace Breadbox.Chips.Vic6567
{
    public static class Vic2RefreshAddressGenerator
    {
        public static Expression Generate(Expression refreshCounter)
        {
            return Expression.Or(Expression.Constant(0x3F00), refreshCounter);
        }

        public static Expression Decrement(MemberExpression refreshCounter)
        {
            return Expression.Assign(refreshCounter, Expression.And(Expression.Decrement(refreshCounter), Expression.Constant(0xFF)));
        }
    }
}
