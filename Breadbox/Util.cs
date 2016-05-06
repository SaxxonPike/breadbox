using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Breadbox
{
    public static class Util
    {
        public static Expression Void(params Expression[] expressions)
        {
            if (expressions.Length == 0)
            {
                return Expression.Empty();
            }
            if (expressions.Last().Type != typeof(void))
            {
                return Expression.Block(expressions.Concat(new[] { Expression.Empty() }).ToArray());
            }
            return Expression.Block(expressions);
        }

        public static IndexExpression ArrayMember<TItem>(Expression<Func<TItem[]>> arrayLambda, Expression index)
        {
            return Expression.ArrayAccess(arrayLambda.Body, index);
        }

        public static MemberExpression Member<TProperty>(Expression<Func<TProperty>> propertyLambda)
        {
            return propertyLambda.Body as MemberExpression;
        }

        public static Expression AssignUsing(this IEnumerable<Expression> expressions, Expression selector,
            Expression value)
        {
            var enumerable = expressions as Expression[] ?? expressions.ToArray();
            var temp = Expression.Variable(enumerable.First().Type);
            return Expression.Block(new[] { temp },
                Expression.Assign(temp, value),
                SelectUsing(enumerable.Select(e => Expression.Assign(e, temp)), selector)
                );
        }

        public static Expression SelectUsing(this IEnumerable<Expression> expressions, Expression selector)
        {
            var enumerable = expressions as Expression[] ?? expressions.ToArray();
            var cases = enumerable.Select((e, i) => Expression.SwitchCase(e, Expression.Constant(i))).ToArray();
            return Expression.Switch(selector, Expression.Default(enumerable.First().Type), cases);
        }

        public static Expression All(params Expression[] expressions)
        {
            return expressions.Aggregate(Expression.AndAlso);
        }

        public static Expression Any(params Expression[] expressions)
        {
            return expressions.Aggregate(Expression.OrElse);
        }

        public static Expression Or(params Expression[] expressions)
        {
            return expressions.Aggregate(Expression.Or);
        }

        public static Expression And(params Expression[] expressions)
        {
            return expressions.Aggregate(Expression.And);
        }

        public static Expression IfThenElseChain(params Expression[][] expressions)
        {
            var reversedExpressions = expressions.Reverse().ToList();
            Expression innermost = reversedExpressions.Select(expression => Expression.IfThen(expression[0], expression[1])).First();
            return reversedExpressions.Skip(1)
                .Aggregate(innermost,
                    (baseExpression, testExpression) =>
                        Expression.IfThenElse(testExpression[0], testExpression[1], baseExpression));
        }
    }
}
