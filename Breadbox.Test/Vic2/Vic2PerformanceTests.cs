using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BreadboxF;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Compatibility;

namespace Breadbox.Test.Vic2
{
    [TestFixture]
    public class Vic2PerformanceTests : Vic2RegionAgnosticBaseTestFixture
    {
        private void EnableAllSprites()
        {
            Vic.WriteRegister(0x15, 0xFF);
        }

        [Test]
        public void ClockSecond_ExecutesInUnderOneSecond()
        {
            SetDisplayEnable(true);
            EnableAllSprites();
            var sw = new Stopwatch();
            sw.Start();
            Vic.ClockSecond();
            sw.Stop();
            sw.ElapsedMilliseconds.Should().BeLessOrEqualTo(1000);
        }
    }
}
