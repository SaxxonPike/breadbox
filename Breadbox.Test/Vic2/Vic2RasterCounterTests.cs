using BreadboxF;
using FluentAssertions;
using NUnit.Framework;

namespace Breadbox.Test.Vic2
{
    [TestFixture]
    public class Vic2RasterCounterTests : Vic2BaseTestFixture
    {
        protected override CommodoreVic2Configuration Config
        {
            get
            {
                // config doesn't matter for these; we're using new NTSC here
                return new CommodoreVic2Configuration(13, 65, 263);
            }
        }

        [Test]
        public void RasterLineCounter_AdvancesPerClock()
        {
            // Arrange
            var expectedValue = Config.RasterIncrement + 1;

            // Act
            Vic.Clock();

            // Assert
            Vic.RasterLineCounter.Should().Be(expectedValue);
        }

        [Test]
        public void RasterLineCounter_ClocksCorrectNumberOfPixelsPerLine()
        {
            // Arrange
            var expectedValue = Config.RasterIncrement;

            // Act
            Vic.ClockMultiple(Config.RasterWidth);

            // Assert
            Vic.RasterLineCounter.Should().Be(expectedValue);
        }
    }
}
