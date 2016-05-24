using BreadboxF;
using FluentAssertions;
using NUnit.Framework;

namespace Breadbox.Test.Vic2
{
    [TestFixture]
    public class Vic2BadLineTests : Vic2BaseTestFixture
    {
        protected override CommodoreVic2Configuration Config
        {
            get { return new CommodoreVic2ConfigurationFactory().CreateNewNtscConfiguration(); }
        }

        [Test]
        public void BadLineAndEnable([Values(true, false)] bool displayEnable, [Range(0, 7)] int yScroll, [Random(0, 7, 2)] int yRaster)
        {
            // Arrange
            SetDisplayEnable(displayEnable);
            SetYScroll(yScroll);

            // Act
            Vic.ClockToRasterY(0x30 + yRaster);

            // Assert
            Vic.BadLinesEnabled.Should().Be(displayEnable);
            Vic.BadLine.Should().Be(displayEnable && yScroll == (Vic.RasterY & 0x7));
        }
    }
}
