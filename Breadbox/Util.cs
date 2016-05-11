using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
            if (expressions.Length == 1 && expressions.First().Type == typeof(void))
            {
                return expressions.First();
            }
            if (expressions.All(e => e is BlockExpression))
            {
                var mainBlock = new List<Expression>();
                var mainBlockVariables = new List<ParameterExpression>();
                mainBlock.AddRange(expressions.Cast<BlockExpression>().SelectMany(e => e.Expressions));
                mainBlockVariables.AddRange(expressions.Cast<BlockExpression>().SelectMany(e => e.Variables));
                if (mainBlock.Last().Type != typeof(void))
                {
                    mainBlock.Add(Expression.Empty());
                }
                return Expression.Block(mainBlockVariables, mainBlock);
            }
            if (expressions.Last().Type != typeof(void))
            {
                return Expression.Block(new ParameterExpression[0], expressions.Concat(new[] { Expression.Empty() }).ToArray());
            }
            return Expression.Block(new ParameterExpression[0], expressions);
        }

        public static Expression Action(Expression<Action> actionLambda)
        {
            return actionLambda.Body;
        }

        public static Expression Func<TResult>(Expression<Func<TResult>> functionLambda)
        {
            return functionLambda.Body;
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

        public static Expression Decode(IEnumerable<Tuple<int, Expression>> conditions, Expression valueToDecode)
        {
            return
                Decode(
                    conditions.Select(c => new Tuple<Expression, Expression>(Expression.Constant(c.Item1), c.Item2)),
                    valueToDecode);
        }

        public static Expression Decode(IEnumerable<Tuple<Expression, Expression>> conditions, Expression valueToDecode)
        {
            var presentDecodes = conditions.Where(d => d.Item2 != null).ToList();
            if (presentDecodes.Count == 0)
            {
                return Expression.Empty();
            }
            if (presentDecodes.Count == 1)
            {
                return Expression.IfThen(Expression.Equal(presentDecodes[0].Item1, valueToDecode), presentDecodes[0].Item2);
            }
            return Simplify(Expression.Switch(valueToDecode, GenerateDecodes(presentDecodes)));
        }

        public static Expression Simplify(SwitchExpression expression)
        {
            var cases = expression.Cases.Where(c => c.TestValues.Count > 0).OrderBy(c => ((ConstantExpression)c.TestValues.First()).Value).ToArray();
            var newCases = new List<SwitchCase>();
            var debugViewCache = new List<string>();

            foreach (var switchCase in cases)
            {
                var switchCaseDebugView = switchCase.Body.GetDebugView();
                var existingIndex = debugViewCache.IndexOf(switchCaseDebugView);
                if (existingIndex >= 0)
                {
                    var existingCase = newCases[existingIndex];
                    newCases[existingIndex] = Expression.SwitchCase(existingCase.Body, existingCase.TestValues.Concat(switchCase.TestValues));
                }
                else
                {
                    newCases.Add(switchCase.Update(switchCase.TestValues.OrderBy(c => ((ConstantExpression)c).Value), switchCase.Body));
                    debugViewCache.Add(switchCaseDebugView);
                }
            }

            return expression.Update(expression.SwitchValue, newCases, expression.DefaultBody);
        }

        private static SwitchCase[] GenerateDecodes(IEnumerable<Tuple<Expression, Expression>> decodes)
        {
            return
                decodes.GroupBy(d => ((ConstantExpression)d.Item1).Value).Select(
                    g =>
                        Expression.SwitchCase(Void(g.Select(pair => pair.Item2).ToArray()),
                            Expression.Constant(g.Key))).ToArray();
        }

        public static string GetDebugView(this Expression exp)
        {
            if (exp == null)
                return null;

            var propertyInfo = typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic);
            var debugView = propertyInfo.GetValue(exp, null) as string;
            return debugView;
        }
    }
}
