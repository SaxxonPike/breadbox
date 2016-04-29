using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Breadbox.Extensions
{
    internal static class ExpressionExtensions
    {
        public static void AssertType<TExpression>(this Expression expression)
        {
            Debug.Assert(expression.Type == typeof(TExpression), string.Format("Expected expression type {0}, but got {1}.", typeof(TExpression), expression.Type));
        }

        public static void AssertType<TExpression1, TExpression2>(this Expression expression)
        {
            Debug.Assert(expression.Type == typeof(TExpression1) || expression.Type == typeof(TExpression2), string.Format("Expected expression types {0} or {1}, but got {2}.", typeof(TExpression1), typeof(TExpression2), expression.Type));
        }

        public static Action CompileToAction(this Expression expression)
        {
            return Expression.Lambda<Action>(expression).Compile();
        }

        public static Action<T1> CompileToAction<T1>(this Expression expression, params ParameterExpression[] parameters)
        {
            return Expression.Lambda<Action<T1>>(expression, parameters).Compile();
        }

        public static Action<T1, T2> CompileToAction<T1, T2>(this Expression expression, params ParameterExpression[] parameters)
        {
            return Expression.Lambda<Action<T1, T2>>(expression, parameters).Compile();
        }

        public static Func<TResult> CompileToFunc<TResult>(this Expression expression)
        {
            return Expression.Lambda<Func<TResult>>(expression).Compile();
        }

        public static Func<T1, TResult> CompileToFunc<T1, TResult>(this Expression expression, params ParameterExpression[] parameters)
        {
            return Expression.Lambda<Func<T1, TResult>>(expression, parameters).Compile();
        }

        public static Func<T1, T2, TResult> CompileToFunc<T1, T2, TResult>(this Expression expression, params ParameterExpression[] parameters)
        {
            return Expression.Lambda<Func<T1, T2, TResult>>(expression, parameters).Compile();
        }
    }
}
