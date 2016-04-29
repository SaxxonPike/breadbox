using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Extensions;
using Breadbox.Interfaces;

namespace Breadbox.Builders
{
    public class MemoryBuilder
    {
        private class ReadOnlyMemory : IReadableAddressable
        {
            private readonly int[] _memory;
            private readonly int _indexBits;

            public ReadOnlyMemory(int indexBits, ICollection<int> initialData, int dataBits)
            {
                _indexBits = indexBits;
                _memory = new int[1 << _indexBits];
                if (initialData != null && initialData.Count > 0)
                {
                    Array.Copy(initialData.Select(i => i & ((1 << dataBits) - 1)).ToArray(), _memory, Math.Min(initialData.Count, _memory.Length));
                }
            }

            public Expression GetReadIndexExpression(Expression indexParameter)
            {
                indexParameter.AssertType<int>();

                var indexMask = Expression.Constant((1 << _indexBits) - 1);
                var maskedIndex = Expression.And(indexParameter, indexMask);
                return Expression.ArrayAccess(Util.Member(() => _memory), maskedIndex);
            }
        }

        private class RandomAccessMemory : ReadOnlyMemory, IReadWriteableAddressable
        {
            private readonly int _dataBits;

            public RandomAccessMemory(int indexBits, ICollection<int> initialData, int dataBits) : base(indexBits, initialData, dataBits)
            {
                _dataBits = dataBits;
            }

            public Expression GetWriteIndexExpression(Expression indexParameter, Expression valueParameter)
            {
                indexParameter.AssertType<int>();
                valueParameter.AssertType<int>();

                var dataMask = Expression.Constant((1 << _dataBits) - 1);
                var maskedData = Expression.And(dataMask, valueParameter);
                return Expression.Assign(GetReadIndexExpression(indexParameter), maskedData);
            }
        }

        private int _indexBits = 16;
        private int _dataBits = 8;
        private int[] _initialData;

        private MemoryBuilder Clone()
        {
            return (MemoryBuilder)MemberwiseClone();
        }

        public MemoryBuilder WithIndexBitCount(int indexBits)
        {
            var result = Clone();
            result._indexBits = indexBits;
            return result;
        }

        public MemoryBuilder WithDataBitCount(int dataBits)
        {
            var result = Clone();
            result._dataBits = dataBits;
            return result;
        }

        public MemoryBuilder WithInitialData(IEnumerable<int> data)
        {
            var result = Clone();
            result._initialData = data.ToArray();
            return result;
        }

        public MemoryBuilder WithInitialData(IEnumerable<byte> data)
        {
            var result = Clone();
            result._initialData = data.Select(i => (int) i).ToArray();
            return result;
        }

        public IReadableAddressable BuildReadOnly()
        {
            return new ReadOnlyMemory(_indexBits, _initialData, _dataBits);
        }

        public IReadWriteableAddressable Build()
        {
            return new RandomAccessMemory(_indexBits, _initialData, _dataBits);
        }
    }
}
