using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Extensions;
using Breadbox.Interfaces;

namespace Breadbox.Builders
{
    public class BusBuilder
    {
        private class Bus : IBus
        {
            private class Source
            {
                public IReadable ReadableSource;
                public int LowestBitNumber;
                public int TargetLowestBitNumber;
                public int BitCount;
            }

            private readonly int _bitCount;
            private Func<int> _readFunction;
            private readonly List<Source> _sources = new List<Source>();
            private bool _isValidReadFunction;

            public Bus(int bitCount)
            {
                _bitCount = bitCount;
            }

            private void Invalidate()
            {
                _isValidReadFunction = false;
                _readFunction = null;
            }

            public Expression GetReadExpression()
            {
                if (!_isValidReadFunction)
                {
                    RebuildReadFunction();
                }
                return (Expression<Func<int>>) (() => _readFunction());
            }

            private static Expression GetSpecificBitsSourceExpression(Source source)
            {
                var result = source.ReadableSource.GetReadBitsExpression(source.LowestBitNumber, source.BitCount);
                return source.TargetLowestBitNumber > 0
                    ? Expression.Or(Expression.LeftShift(result, Expression.Constant(source.TargetLowestBitNumber)), Expression.Constant((1 << source.TargetLowestBitNumber) - 1))
                    : result;
            }

            private static Expression GetSourceExpression(Source source)
            {
                return source.BitCount > 0
                    ? GetSpecificBitsSourceExpression(source)
                    : source.ReadableSource.GetReadExpression();
            }

            private void RebuildReadFunction()
            {
                Expression dataMask = Expression.Constant((1 << _bitCount) - 1);
                var allSources = _sources.Select(GetSourceExpression).Aggregate(dataMask, Expression.And);
                _readFunction = Expression.Lambda<Func<int>>(allSources).Compile();
            }

            public void Connect(IReadable source)
            {
                _sources.Add(new Source
                {
                    ReadableSource = source
                });
                Invalidate();
            }

            public void Connect(IReadable source, int lowestBitNumber, int bitCount)
            {
                _sources.Add(new Source
                {
                    BitCount = bitCount,
                    LowestBitNumber = lowestBitNumber,
                    ReadableSource = source

                });
                Invalidate();
            }

            public void Connect(IReadable source, int lowestBitNumber, int bitCount, int targetLowestBitNumber)
            {
                _sources.Add(new Source
                {
                    BitCount = bitCount,
                    LowestBitNumber = lowestBitNumber,
                    ReadableSource = source,
                    TargetLowestBitNumber = targetLowestBitNumber

                });
                Invalidate();
            }
        }

        private int _bitCount = 8;

        private BusBuilder Clone()
        {
            return (BusBuilder) MemberwiseClone();
        }

        public BusBuilder WithBitCount(int bitCount)
        {
            var result = Clone();
            result._bitCount = bitCount;
            return result;
        }

        public IBus Build()
        {
            return new Bus(_bitCount);
        }
    }
}
