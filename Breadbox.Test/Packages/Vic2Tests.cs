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
            var second = vic2.Clock(vic2.CyclesPerSecond, systemRam.Read, colorRam.Read, null, null);
            var sw = new Stopwatch();

            var secondFunc = Expression.Lambda<Action>(second).Compile();
            secondFunc();
            sw.Start();
            secondFunc();
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
            sw.Reset();
        }
    }
}
