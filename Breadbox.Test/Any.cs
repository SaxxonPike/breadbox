using System;
using System.Linq;

namespace Breadbox.Test
{
    public static class Any
    {
        private static readonly Random Rng = new Random();

        public static byte Byte()
        {
            return (byte) (Int() & 0xFF);
        }

        public static byte[] Bytes(int count)
        {
            return Enumerable.Range(0, count).Select(i => Byte()).ToArray();
        }

        public static byte[] Bytes(int minCount, int maxCount)
        {
            return Enumerable.Range(0, Int(minCount, maxCount)).Select(i => Byte()).ToArray();
        }

        public static int Int()
        {
            return Rng.Next();
        }

        public static int Int(int minValue)
        {
            return Rng.Next(minValue);
        }

        public static int Int(int minValue, int maxValue)
        {
            return Rng.Next(minValue, maxValue);
        }

        public static int[] Ints(int count)
        {
            return Enumerable.Range(0, count).Select(i => Int()).ToArray();
        }

        public static int[] Ints(int minCount, int maxCount)
        {
            return Enumerable.Range(0, Int(minCount, maxCount)).Select(i => Int()).ToArray();
        }
    }
}
