using FluentAssertions;
using NUnit.Framework;

namespace Breadbox.Test.Vic2
{
    public class Vic2IrqTests : Vic2RegionAgnosticBaseTestFixture
    {
        [Test]
        [TestCase(0, false)]
        [TestCase(1, true)]
        public void RasterIrqTriggersOnCycle1(int rasterY, bool expectedIrqState)
        {
            Vic.WriteRegister(0x12, rasterY);
            Vic.WriteRegister(0x1A, 0x01);
            Vic.ClockTo(Config.RasterIncrement, rasterY);
            var observed = Vic.ReadRegister(0x19) & 0x8F;
            var expected = expectedIrqState ? 0x81 : 0x00;
            observed.Should().Be(expected);
        }

        [Test]
        [TestCase(0, true)]
        [TestCase(1, true)]
        public void RasterIrqTriggersOnCycle2(int rasterY, bool expectedIrqState)
        {
            Vic.WriteRegister(0x12, rasterY);
            Vic.WriteRegister(0x1A, 0x01);
            Vic.ClockTo(Config.RasterIncrement + 8, rasterY);
            var observed = Vic.ReadRegister(0x19) & 0x8F;
            var expected = expectedIrqState ? 0x81 : 0x00;
            observed.Should().Be(expected);
        }
    }
}
