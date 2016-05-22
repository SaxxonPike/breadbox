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
    public class Vic2BorderTests : Vic2BaseTestFixture
    {
        protected override CommodoreVic2Configuration Config
        {
            get { return new CommodoreVic2ConfigurationFactory().Create6567R8Configuration(); }
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Border_TestLeftTransition(bool columnSelect)
        {
            // Arrange
            SetDisplayEnable(true);
            SetRowSelect(true);
            SetColumnSelect(columnSelect);

            // Act
            Vic.ClockToRasterY(0x33);
            Vic.ClockToCounterX(columnSelect ? 0x017 : 0x01E);

            // Assert
            Vic.BorderMainEnabled.Should().BeTrue();
            Vic.Clock();
            Vic.BorderMainEnabled.Should().BeFalse();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Border_TestRightTransition(bool columnSelect)
        {
            // Arrange
            SetDisplayEnable(true);
            SetRowSelect(true);
            SetColumnSelect(columnSelect);

            // Act
            Vic.ClockToRasterY(0x33);
            Vic.ClockToCounterX(columnSelect ? 0x157 : 0x14E);

            // Assert
            Vic.BorderMainEnabled.Should().BeFalse();
            Vic.Clock();
            Vic.BorderMainEnabled.Should().BeTrue();
        }
    }
}
