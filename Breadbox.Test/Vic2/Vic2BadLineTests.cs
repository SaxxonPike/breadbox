using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            get { return new CommodoreVic2ConfigurationFactory().Create6567R8Configuration(); }
        }

        [Test]
        public void BadLineAndEnable([Values(true, false)] bool displayEnable, [Range(0, 7)] int yScroll)
        {
            // Arrange
            SetDisplayEnable(displayEnable);
            SetYScroll(yScroll);

            // Act
            Vic.ClockToRasterY(0x30);

            // Assert
            Vic.BadLinesEnabled.Should().Be(displayEnable);
            Vic.BadLine.Should().Be(displayEnable && yScroll == (Vic.RasterY & 0x7));
        }
    }
}
