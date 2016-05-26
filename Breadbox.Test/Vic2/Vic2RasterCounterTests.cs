using BreadboxF;
using FluentAssertions;
using NUnit.Framework;

namespace Breadbox.Test.Vic2
{
    [TestFixture]
    public class Vic2RasterCounterTests : Vic2RegionAgnosticBaseTestFixture
    {
        [Test]
        public void RasterLineCounter_AdvancesPerClock()
        {
            // Arrange
            var expectedValue = Config.RasterIncrement;

            // Act
            Vic.Clock();

            // Assert
            Vic.RasterLineCounter.Should().Be(expectedValue);
        }

        [Test]
        public void RasterLineCounter_ClocksCorrectNumberOfPixelsPerLine()
        {
            // Arrange
            var expectedValue = Config.RasterIncrement - 1;

            // Act
            Vic.ClockMultiple(Config.RasterWidth);

            // Assert
            Vic.RasterLineCounter.Should().Be(expectedValue);
        }

        [Test]
        public void RasterLineCounter_ClocksCorrectNumberOfRasterLines()
        {
            // Arrange
            var expectedValue = 0;

            // Act
            Vic.ClockMultiple(Config.RasterWidth * Config.RasterLinesPerFrame);
            Vic.Clock();

            // Assert
            Vic.RasterY.Should().Be(expectedValue);
        }
    }
}
