﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Breadbox
{
    public static class Util
    {
        public static Expression Repeat(int times, params Expression[] expressions)
        {
            if (expressions.Length < 1)
            {
                return Expression.Empty();
            }

            var loopBlock = new List<Expression>();
            var block = new List<Expression>();
            var counter = Expression.Variable(typeof(int));
            var endLoopLabel = Expression.Label(typeof(void));
            var loopBlockVariables = new List<ParameterExpression>();
            loopBlock.Add(Expression.IfThen(Expression.Equal(Expression.Constant(0), Expression.PostDecrementAssign(counter)), Expression.Break(endLoopLabel)));

            if (expressions.All(e => e is BlockExpression))
            {
                loopBlock.AddRange(expressions.Cast<BlockExpression>().SelectMany(e => e.Expressions));
                loopBlockVariables.AddRange(expressions.Cast<BlockExpression>().SelectMany(e => e.Variables));
            }
            else
            {
                loopBlock.AddRange(expressions);
            }

            var innerBlockExpression = loopBlockVariables.Count > 0
                ? Expression.Block(loopBlockVariables, loopBlock)
                : Void(loopBlock.ToArray());

            block.Add(Expression.Assign(counter, Expression.Constant(times)));
            block.Add(Expression.Loop(innerBlockExpression, endLoopLabel));

            return Expression.Block(new[] { counter }, block);
        }

        public static Expression Void(params Expression[] expressions)
        {
            if (expressions.Length == 0)
            {
                return Expression.Empty();
            }
            if (expressions.Last().Type != typeof(void))
            {
                return Expression.Block(new ParameterExpression[0], expressions.Concat(new[] { Expression.Empty() }).ToArray());
            }
            return Expression.Block(new ParameterExpression[0], expressions);
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
