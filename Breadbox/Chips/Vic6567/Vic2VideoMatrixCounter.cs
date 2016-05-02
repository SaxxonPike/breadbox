using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Extensions;

namespace Breadbox.Chips.Vic6567
{
    /// <summary>
    /// Emulates behavior of VMLI, VC and VCBASE.
    /// </summary>
    public static class Vic2VideoMatrixCounter
    {
        public static Expression Increment(MemberExpression vc, MemberExpression vmli)
        {
            return Expression.Block(
                Expression.Assign(vc, Expression.And(Expression.Increment(vc), Expression.Constant(0x3FF))),
                Expression.Assign(vmli, Expression.And(Expression.Increment(vmli), Expression.Constant(0x3F))));
        }

        public static Expression ResetBase(MemberExpression vcbase)
        {
            return Expression.Assign(vcbase, Expression.Constant(0));
        }

        public static Expression StoreValueToBase(MemberExpression vcbase, Expression vc)
        {
            return Expression.Assign(vcbase, vc);
        }

        public static Expression LoadValueFromBase(Expression vcbase, MemberExpression vc, MemberExpression vmli)
        {
            return Expression.Block(
                Expression.Assign(vc, vcbase),
                Expression.Assign(vmli, Expression.Constant(0)));
        }
    }
}
