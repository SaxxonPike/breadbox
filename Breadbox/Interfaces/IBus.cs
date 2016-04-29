using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Breadbox.Interfaces
{
    public interface IBus : IReadable
    {
        void Connect(IReadable source);
        void Connect(IReadable source, int lowestBitNumber, int bitCount);
        void Connect(IReadable source, int lowestBitNumber, int bitCount, int targetLowestBitNumber);
    }
}
