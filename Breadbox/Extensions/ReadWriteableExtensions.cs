using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Interfaces;

namespace Breadbox.Extensions
{
    public static class ReadWriteableExtensions
    {
        public static Expression GetReadExpression(this IReadableAddressable readable, int index)
        {
            return readable.GetReadIndexExpression(Expression.Constant(index));
        }

        public static Expression GetWriteExpression(this IWriteableAddressable writeable, int index, Expression valueExpression)
        {
            return writeable.GetWriteIndexExpression(Expression.Constant(index), valueExpression);
        }

        public static Expression GetReadBitExpression(this IReadable readable, int bitNumber)
        {
            var source = readable.GetReadExpression();
            var testMask = Expression.Constant(1 << bitNumber);
            return Expression.NotEqual(Expression.Constant(0), Expression.And(source, testMask));
        }

        public static Expression GetReadBitsExpression(this IReadable readable, int lowestBitNumber, int bitCount)
        {
            var source = readable.GetReadExpression();
            var sourceMask = Expression.Constant(((1 << bitCount) - 1) << lowestBitNumber);
            var sourceValue = Expression.And(source, sourceMask);
            if (lowestBitNumber > 0)
            {
                sourceValue = Expression.RightShift(sourceValue, Expression.Constant(lowestBitNumber));
            }
            return sourceValue;
        }

        public static Expression GetReadBitsExpression(this IReadableAddressable readable, int index, int lowestBitNumber, int bitCount)
        {
            return readable.GetReadExpression(index);
        }

        public static Expression GetWriteBitExpression(this IReadWriteable writeable, Expression bitValue, int bitNumber)
        {
            bitValue.AssertType<int>();

            var source = Expression.NotEqual(Expression.Constant(0), bitValue);
            var targetMask = Expression.Constant(1 << bitNumber);
            var currentValue = writeable.GetReadExpression();
            return writeable.GetWriteExpression(Expression.IfThenElse(source, Expression.Or(currentValue, targetMask),
                Expression.ExclusiveOr(currentValue, targetMask)));
        }

        public static Expression GetWriteBitsExpression(this IReadWriteable writeable, Expression bitsValue,
            int lowestBitNumber, int bitCount)
        {
            bitsValue.AssertType<int>();

            var maskValue = ((1 << bitCount) - 1) << lowestBitNumber;
            var source = bitsValue;
            if (lowestBitNumber > 0)
            {
                source = Expression.LeftShift(source, Expression.Constant(bitCount));
            }
            source = Expression.And(source, Expression.Constant(maskValue));
            return writeable.GetWriteExpression(Expression.Or(Expression.And(Expression.Constant(~maskValue), writeable.GetReadExpression()), source));

        }

        public static Func<int> GetReadFunction(this IReadable readable)
        {
            return readable
                .GetReadExpression()
                .CompileToFunc<int>();
        }

        public static Func<int, int> GetReadFunction(this IReadableAddressable readable)
        {
            var indexParameter = Expression.Parameter(typeof(int));
            return readable
                .GetReadIndexExpression(indexParameter)
                .CompileToFunc<int, int>(indexParameter);
        }

        public static Action<int> GetWriteFunction(this IWriteable writeable)
        {
            var valueParameter = Expression.Parameter(typeof(int));
            return writeable
                .GetWriteExpression(valueParameter)
                .CompileToAction<int>(valueParameter);
        }

        public static Action<int, int> GetWriteFunction(this IWriteableAddressable writeable)
        {
            var indexParameter = Expression.Parameter(typeof(int));
            var valueParameter = Expression.Parameter(typeof(int));
            return writeable
                .GetWriteIndexExpression(indexParameter, valueParameter)
                .CompileToAction<int, int>(indexParameter, valueParameter);
        }
    }
}
