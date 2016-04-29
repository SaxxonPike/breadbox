using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Breadbox.Extensions
{
    internal static class Util
    {
        public static MemberExpression Member<TProperty>(Expression<Func<TProperty>> propertyLambda)
        {
            return propertyLambda.Body as MemberExpression;
        }

        public static Expression WithinRange(Expression expression, int low, int high)
        {
            return Expression.AndAlso(Expression.GreaterThanOrEqual(expression, Expression.Constant(low)),
                Expression.LessThanOrEqual(expression, Expression.Constant(high)));
        }
    }
}
