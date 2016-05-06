using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Packages;
using NUnit.Framework;
using NUnit.Framework.Compatibility;

namespace Breadbox.Test.Packages
{
    [TestFixture]
    public class Vic2Tests
    {
        [Test]
        public void Test1()
        {
            var vic2 = new Mos6567R8();
            var colorRam = new Ram1x4();
            var systemRam = new Ram64x8();

            var clock = vic2.Clock(systemRam.Read, colorRam.Read, null, null);
            var clockFunc = Expression.Lambda<Action>(clock).Compile();
            var pixel = vic2.OutputPixel;
            var pixelFunc = Expression.Lambda<Func<int>>(pixel).Compile();

            var procsPerSecond = 520*263*60;
            var sw = new Stopwatch();

            clockFunc();
            sw.Start();
            for (var i = 0; i < procsPerSecond; i++)
            {
                clockFunc();
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
            sw.Reset();

            pixelFunc();
            sw.Start();
            for (var i = 0; i < procsPerSecond; i++)
            {
                pixelFunc();
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
            sw.Reset();

            sw.Start();
            for (var i = 0; i < procsPerSecond; i++)
            {
                clockFunc();
                pixelFunc();
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
            sw.Reset();

            var combined = Expression.Block(clock, pixel);
            var combinedFunc = Expression.Lambda<Func<int>>(combined).Compile();
            combinedFunc();
            sw.Start();
            for (var i = 0; i < procsPerSecond; i++)
            {
                combinedFunc();
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
            sw.Reset();


        }
    }
}
