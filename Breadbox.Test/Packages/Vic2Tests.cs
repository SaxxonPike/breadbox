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
            var frame = vic2.Frame(systemRam.Read, colorRam.Read, null, null);
            var framesPerSecond = 60;
            var sw = new Stopwatch();

            var frameFunc = Expression.Lambda<Action>(frame).Compile();
            frameFunc();
            sw.Start();
            for (var i = 0; i < framesPerSecond; i++)
            {
                frameFunc();
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
            sw.Reset();
        }
    }
}
