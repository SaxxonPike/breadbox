using System;
using System.Linq;
using BreadboxF;
using FluentAssertions;
using NUnit.Framework;

namespace Breadbox.Test.Vic2
{
    [TestFixture]
    public class Vic2RegisterTests : Vic2RegionAgnosticBaseTestFixture
    {
        private void TestSpriteRegister(int register, int value, Func<int, bool> test)
        {
            // Arrange
            var expectedValue = value & 0xFF;
            var expectedBits = Enumerable.Range(0, 8).Select(i => (value & (1 << i)) != 0).ToArray();

            // Act
            Vic.WriteRegister(register, value);

            // Assert
            Vic.ReadRegister(register).Should().Be(expectedValue);
            var observedBits = Enumerable.Range(0, 8).Select(test);
            observedBits.ShouldAllBeEquivalentTo(expectedBits);
        }

        private void TestColorRegister(int register, int value)
        {
            // Act
            Vic.WriteRegister(register, value);

            // Assert
            Vic.ReadRegister(register).Should().Be(0xF0 | (value & 0xF));
        }

        [Test]
        public void MobXRegisters_StoreAndLoad([Range(0, 7)] int index, [Random(1)] int value)
        {
            // Arrange
            var expectedValue = value & 0xFF;
            var register = index*2;

            // Act
            Vic.WriteRegister(register, value);

            // Assert
            Vic.ReadRegister(register).Should().Be(expectedValue);
            Vic.SpriteX(index).Should().Be(expectedValue);
        }

        [Test]
        public void MobYRegisters_StoreAndLoad([Range(0, 7)] int index, [Random(1)] int value)
        {
            // Arrange
            var expectedValue = value & 0xFF;
            var register = (index * 2) + 1;

            // Act
            Vic.WriteRegister(register, value);

            // Assert
            Vic.ReadRegister(register).Should().Be(expectedValue);
            Vic.SpriteY(index).Should().Be(expectedValue);
        }

        [Test]
        public void MobX8Register_StoreAndLoad([Random(1)] int value)
        {
            // Arrange
            var expectedValue = value & 0xFF;
            var expectedX = Enumerable.Range(0, 8).Select(i => (value & (1 << i)) != 0 ? 0x100 : 0x000).ToArray();
            const int register = 0x10;

            // Act
            Vic.WriteRegister(register, value);

            // Assert
            Vic.ReadRegister(register).Should().Be(expectedValue);
            var observedX = Enumerable.Range(0, 8).Select(Vic.SpriteX);
            observedX.ShouldAllBeEquivalentTo(expectedX);
        }

        [Test]
        public void ControlRegister1_StoreAndLoad([Random(1)] int value)
        {
            // Arrange
            const int register = 0x11;
            var expectedValue = value & 0x7F;
            Vic.Clock();

            // Act
            Vic.WriteRegister(register, value);

            // Assert
            var observedValue = Vic.ReadRegister(register);
            observedValue.Should().Be(expectedValue);
            Vic.ExtraColorMode.Should().Be((value & 0x40) != 0);
            Vic.BitmapMode.Should().Be((value & 0x20) != 0);
            Vic.DisplayEnabled.Should().Be((value & 0x10) != 0);
            Vic.RowSelect.Should().Be((value & 0x08) != 0);
            Vic.YScroll.Should().Be(value & 0x07);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(255)]
        [TestCase(256)]
        [TestCase(257)]
        [TestCase(262)]
        [TestCase(263)]
        public void Raster8_Load(int rasterLine)
        {
            // Arrange
            const int register = 0x11;
            var expectedValue = ((rasterLine%263) & 0x100) != 0;
            Vic.Clock();

            // Act
            Vic.ClockMultiple(rasterLine * Config.RasterWidth);

            // Assert
            var observedValue = (Vic.ReadRegister(register) & 0x80) != 0;
            observedValue.Should().Be(expectedValue);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void Raster8_Store(bool expectedValue)
        {
            // Arrange
            const int register = 0x11;

            // Act
            Vic.WriteRegister(register, expectedValue ? 0x80 : 0x00);

            // Assert
            var observedValue = (Vic.RasterYCompareValue & 0x100) != 0;
            observedValue.Should().Be(expectedValue);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(255)]
        [TestCase(256)]
        [TestCase(257)]
        [TestCase(262)]
        [TestCase(263)]
        public void Raster_Load(int rasterLine)
        {
            // Arrange
            const int register = 0x12;
            var expectedValue = (rasterLine%263) & 0xFF;
            Vic.Clock();

            // Act
            Vic.ClockMultiple(rasterLine * Config.RasterWidth);

            // Assert
            Vic.ReadRegister(register).Should().Be(expectedValue);
        }

        [Test]
        public void Raster_Store([Random(1)] int value)
        {
            // Arrange
            const int register = 0x12;
            var expectedValue = value & 0xFF;

            // Act
            Vic.WriteRegister(register, value);

            // Assert
            var observedValue = Vic.RasterYCompareValue & 0xFF;
            observedValue.Should().Be(expectedValue);
        }

        [Test]
        public void SpriteEnable_StoreAndLoad([Random(1)] int value)
        {
            TestSpriteRegister(0x15, value, Vic.SpriteEnabled);
        }

        [Test]
        public void ControlRegister2_StoreAndLoad([Random(1)] int value)
        {
            // Arrange
            const int register = 0x16;
            var expectedValue = (value & 0x3F) | 0xC0;

            // Act
            Vic.WriteRegister(register, value);

            // Assert
            var observedValue = Vic.ReadRegister(register);
            observedValue.Should().Be(expectedValue);
            Vic.Res.Should().Be((value & 0x20) != 0);
            Vic.MultiColorMode.Should().Be((value & 0x10) != 0);
            Vic.ColumnSelect.Should().Be((value & 0x08) != 0);
            Vic.XScroll.Should().Be(value & 0x07);
        }

        [Test]
        public void SpriteYExpansion_StoreAndLoad([Random(1)] int value)
        {
            TestSpriteRegister(0x17, value, Vic.SpriteYExpansionEnabled);
        }

        [Test]
        public void SpriteDataPriority_StoreAndLoad([Random(1)] int value)
        {
            TestSpriteRegister(0x1B, value, Vic.SpriteDataPriority);
        }

        [Test]
        public void SpriteMultiColorEnable_StoreAndLoad([Random(1)] int value)
        {
            TestSpriteRegister(0x1C, value, Vic.SpriteMultiColorEnabled);
        }

        [Test]
        public void SpriteXExpansion_StoreAndLoad([Random(1)] int value)
        {
            TestSpriteRegister(0x1D, value, Vic.SpriteXExpansionEnabled);
        }

        [Test]
        public void SpriteBorderColor_StoreAndLoad([Random(1)] int value)
        {
            TestColorRegister(0x20, value);
        }

        [Test]
        public void BackgroundColor0_StoreAndLoad([Random(1)] int value)
        {
            TestColorRegister(0x21, value);
        }

        [Test]
        public void BackgroundColor1_StoreAndLoad([Random(1)] int value)
        {
            TestColorRegister(0x22, value);
        }

        [Test]
        public void BackgroundColor2_StoreAndLoad([Random(1)] int value)
        {
            TestColorRegister(0x23, value);
        }

        [Test]
        public void BackgroundColor3_StoreAndLoad([Random(1)] int value)
        {
            TestColorRegister(0x24, value);
        }

        [Test]
        public void SpriteMultiColor0_StoreAndLoad([Random(1)] int value)
        {
            TestColorRegister(0x25, value);
        }

        [Test]
        public void SpriteMultiColor1_StoreAndLoad([Random(1)] int value)
        {
            TestColorRegister(0x26, value);
        }

        [Test]
        public void SpriteColors_StoreAndLoad([Range(0x27, 0x2E)] int register, [Random(1)] int value)
        {
            TestColorRegister(register, value);
        }


    }
}
