using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BreadboxF;
using NUnit.Framework;
using NUnit.Framework.Compatibility;

namespace Breadbox.Test.BreadboxF
{
    [TestFixture]
    public class Vic2
    {
        [Test]
        public void Test1()
        {
            Action<int, bool> videoOutput = (color, blank) => { };
            var ram = new Commodore64SystemRam();
            var colorRam = new Commodore64ColorRam();
            Func<int, int> vic2Read = address => ram.Read(address) | (colorRam.Read(address) << 8);
            var config = new CommodoreVic2Configuration(13, 65, 263);
            var vic = new CommodoreVic2Chip(config, vic2Read, videoOutput, null, null);

            vic.ClockFrame();
            var sw = new Stopwatch();
            sw.Start();
            vic.ClockFrame();
            sw.Stop();
            Console.WriteLine("{0}ms", sw.ElapsedMilliseconds);
        }
    }
}
