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

        private const int TopRowSelectOff = 0x037;
        private const int TopRowSelectOn = 0x033;
        private const int BottomRowSelectOff = 0x0F7;
        private const int BottomRowSelectOn = 0x0FB;
        private const int LeftColumnSelectOn = 0x018;
        private const int LeftColumnSelectOff = 0x01F;
        private const int RightColumnSelectOff = 0x14F;
        private const int RightColumnSelectOn = 0x158;

        private static int GetLeftX(bool columnSelect)
        {
            return columnSelect ? LeftColumnSelectOn : LeftColumnSelectOff;
        }

        private static int GetRightX(bool columnSelect)
        {
            return columnSelect ? RightColumnSelectOn : RightColumnSelectOff;
        }

        private static int GetTopY(bool rowSelect)
        {
            return rowSelect ? TopRowSelectOn : TopRowSelectOff;
        }

        private static int GetBottomY(bool rowSelect)
        {
            return rowSelect ? BottomRowSelectOn : BottomRowSelectOff;
        }

        private int GetEndCycle()
        {
            return Config.RasterIncrement - 8;
        }

        [SetUp]
        public void Configure()
        {
            SetDisplayEnable(true);
        }

        [Test]
        [Description("1. If the X coordinate reaches the right comparison value, the main border flip flop is set.")]
        public void BorderUnit_Rule1([Values(true, false)] bool columnSelect, [Values(true, false)] bool rowSelect)
        {
            // Arrange
            SetColumnSelect(columnSelect);
            SetRowSelect(rowSelect);

            // Act
            Vic.ClockTo(GetRightX(columnSelect), GetTopY(rowSelect));

            // Assert
            Vic.BorderMainEnabled.Should().BeTrue();
        }

        [Test]
        [Description("2. If the Y coordinate reaches the bottom comparison value in cycle 63, the vertical border flip flop is set.")]
        public void BorderUnit_Rule2([Values(true, false)] bool columnSelect, [Values(true, false)] bool rowSelect)
        {
            // Arrange
            SetColumnSelect(columnSelect);
            SetRowSelect(rowSelect);

            // Act
            Vic.ClockTo(GetEndCycle(), GetBottomY(rowSelect));

            // Assert
            Vic.BorderVerticalEnabled.Should().BeTrue();
        }

        [Test]
        [Description("3. If the Y coordinate reaches the top comparison value in cycle 63 and the DEN bit in register $d011 is set, the vertical border flip flop is reset.")]
        public void BorderUnit_Rule3([Values(true, false)] bool columnSelect, [Values(true, false)] bool rowSelect, [Values(true, false)] bool displayEnable)
        {
            // Arrange
            SetDisplayEnable(displayEnable);
            SetColumnSelect(columnSelect);
            SetRowSelect(rowSelect);

            // Act
            Vic.ClockTo(GetEndCycle(), GetTopY(rowSelect));

            // Assert
            Vic.BorderVerticalEnabled.Should().Be(!displayEnable);
        }

        [Test]
        [Description("4. If the X coordinate reaches the left comparison value and the Y coordinate reaches the bottom one, the vertical border flip flop is set.")]
        public void BorderUnit_Rule4([Values(true, false)] bool columnSelect, [Values(true, false)] bool rowSelect)
        {
            // Arrange
            SetColumnSelect(columnSelect);
            SetRowSelect(rowSelect);

            // Act
            Vic.ClockTo(GetLeftX(columnSelect), GetBottomY(rowSelect));

            // Assert
            Vic.BorderVerticalEnabled.Should().BeTrue();
        }

        [Test]
        [Description("5. If the X coordinate reaches the left comparison value and the Y coordinate reaches the top one and the DEN bit in register $d011 is set, the vertical border flip flop is reset.")]
        public void BorderUnit_Rule5([Values(true, false)] bool columnSelect, [Values(true, false)] bool rowSelect, [Values(true, false)] bool displayEnable)
        {
            // Arrange
            SetDisplayEnable(displayEnable);
            SetColumnSelect(columnSelect);
            SetRowSelect(rowSelect);

            // Act
            Vic.ClockTo(GetLeftX(columnSelect), GetTopY(rowSelect));

            // Assert
            Vic.BorderVerticalEnabled.Should().Be(!displayEnable);
        }

        [Test]
        [Description("6. If the X coordinate reaches the left comparison value and the vertical border flip flop is not set, the main flip flop is reset.")]
        public void BorderUnit_Rule6([Values(true, false)] bool columnSelect, [Values(true, false)] bool rowSelect)
        {
            // Arrange
            SetColumnSelect(columnSelect);
            SetRowSelect(rowSelect);

            // Act
            Vic.ClockTo(GetLeftX(columnSelect), GetTopY(rowSelect));

            // Assert
            Vic.BorderMainEnabled.Should().BeFalse();
        }

        [Test]
        [Description("Section 3.14.1 RSEL")]
        public void RowSelectHyperscreenTop()
        {
            // Arrange
            SetRowSelect(false);

            // Act
            Vic.ClockToRasterY(GetTopY(true) + 1);
            SetRowSelect(true);
            Vic.ClockToRasterY(0x40);

            // Assert
            Vic.BorderVerticalEnabled.Should().BeTrue();
        }

        [Test]
        [Description("Section 3.14.1 RSEL")]
        public void RowSelectHyperscreenBottom()
        {
            // Arrange
            SetRowSelect(true);

            // Act
            Vic.ClockToRasterY(GetBottomY(true) - 1);
            SetRowSelect(false);
            Vic.ClockToRasterY(0);

            // Assert
            Vic.BorderVerticalEnabled.Should().BeFalse();
        }

        [Test]
        [Description("Section 3.14.1 CSEL")]
        public void ColumnSelectHyperscreenLeft()
        {
            // Arrange
            SetColumnSelect(false);

            // Act
            Vic.ClockTo(0x40, GetLeftX(false) + 1);
            SetColumnSelect(true);
            Vic.ClockMultiple(16);

            // Assert
            Vic.BorderMainEnabled.Should().BeTrue();
        }

        [Test]
        [Description("Section 3.14.1 CSEL")]
        public void ColumnSelectHyperscreenRight()
        {
            // Arrange
            SetColumnSelect(true);

            // Act
            Vic.ClockTo(0x40, GetRightX(true) - 1);
            SetColumnSelect(false);
            Vic.ClockMultiple(16);

            // Assert
            Vic.BorderMainEnabled.Should().BeFalse();
        }
    }
}
