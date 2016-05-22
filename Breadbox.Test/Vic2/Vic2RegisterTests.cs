using System.Linq;
using BreadboxF;
using FluentAssertions;
using NUnit.Framework;

namespace Breadbox.Test.Vic2
{
    [TestFixture]
    public class Vic2RegisterTests : Vic2BaseTestFixture
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

            // Act
            Vic.WriteRegister(register, value);

            // Assert
            var observedValue = Vic.ReadRegister(register);
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
        public void Raster8_Load(int rasterLine)
        {
            // Arrange
            const int register = 0x11;
            var expectedValue = ((rasterLine%263) & 0x100) != 0;

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


    }
}
