using System.Linq.Expressions;
using Breadbox.Extensions;
using Breadbox.Interfaces;

namespace Breadbox.Builders
{
    public class CellBuilder
    {
        private class ReadOnlyCell : IReadable
        {
            // ReSharper disable once FieldCanBeMadeReadOnly.Local
            protected int Value;

            public ReadOnlyCell(int bitCount, int initialValue)
            {
                Value = initialValue & ((1 << bitCount) - 1);
            }

            public Expression GetReadExpression()
            {
                return Util.Member(() => Value);
            }
        }

        private class Cell : ReadOnlyCell, IReadWriteable
        {
            private readonly int _bitCount;

            public Cell(int bitCount, int initialValue) : base(bitCount, initialValue)
            {
                _bitCount = bitCount;
            }

            public Expression GetWriteExpression(Expression valueParameter)
            {
                valueParameter.AssertType<int>();

                var mask = Expression.Constant((1 << _bitCount) - 1);
                var maskedValue = Expression.And(mask, valueParameter);
                return Expression.Assign(Util.Member(() => Value), maskedValue);
            }
        }

        private int _bits = 8;
        private int _initialValue;

        private CellBuilder Clone()
        {
            return (CellBuilder) MemberwiseClone();
        }

        public CellBuilder WithDataBitCount(int bits)
        {
            var result = Clone();
            result._bits = bits;
            return result;
        }

        public CellBuilder WithInitialValue(int initialValue)
        {
            var result = Clone();
            result._initialValue = initialValue;
            return result;
        }

        public IReadable BuildReadOnly()
        {
            return new ReadOnlyCell(_bits, _initialValue);
        }

        public IReadWriteable Build()
        {
            return new Cell(_bits, _initialValue);
        }
    }
}
