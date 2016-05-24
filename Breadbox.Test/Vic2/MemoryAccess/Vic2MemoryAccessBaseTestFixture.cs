using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Breadbox.Test.Vic2
{
    [TestFixture]
    public abstract class Vic2MemoryAccessBaseTestFixture : Vic2BaseTestFixture
    {
        private int _address = 0;
        private int _memoryValue = 0;

        [SetUp]
        public void Configure()
        {
            _address = -1;
            _memoryValue = 0;
            MemoryMock.Setup(m => m.Read(It.IsAny<int>())).Returns(() => _memoryValue).Callback<int>(a => _address = a);
        }

        private int GetXForMac(int index)
        {
            return (Config.RasterOpsX + (index * 4)) % Config.RasterWidth;
        }

        [Test]
        public void RasterLineBeginsOnSprite3PointerAccess()
        {
            Vic.ClockToRasterY(1);
            Vic.MemoryAccessCycle.Should().Be(18);
        }

        [Test]
        public void SpritePointerAccessAddresses([Random(0, 15, 2)] int videoMemoryPointer, [Range(0, 7)] int index)
        {
            // Arrange
            var expectedAddress = (videoMemoryPointer << 10) | (0x3F8) | index;
            var x = GetXForMac(6 + (index*4));
            SetVideoMemoryPointer(videoMemoryPointer);

            // Act
            Vic.ClockToCounterX(x);

            // Assert
            _address.Should().Be(expectedAddress);
        }

        [Test]
        public void SpriteDataAccessAddresses([Random(0, 255, 2)] int mobPointer, [Range(0, 2)] int counter, [Range(0, 7)] int index)
        {
            // Arrange
            var expectedAddress = (mobPointer << 6) | counter;
            var x = GetXForMac(7 + counter + (index * 4));
            _memoryValue = mobPointer;

            // Act
            Vic.ClockToCounterX(0x000);
            Vic.PokeRegister(index * 2, 0x100);
            Vic.PokeRegister((index * 2) + 1, Vic.RasterY);
            Vic.PokeRegister(0x15, 1 << index);
            Vic.ClockToCounterX(x);

            // Assert
            _address.Should().Be(expectedAddress);
        }

        [Test]
        public void RefreshAccessAddresses([Random(0, 4, 2)] int index)
        {
            // Arrange
            var expectedAddress = 0x3FFF - index;
            var x = GetXForMac(38 + (index * 2));

            // Act
            Vic.ClockToCounterX(x);

            // Assert
            _address.Should().Be(expectedAddress);
        }

        [Test]
        public void ColorAccessAddresses([Random(0, 39, 2)] int index, [Random(0, 24, 2)] int row, [Random(0, 15, 2)] int videoMemoryPointer, [Random(0, 4095, 2)] int cData)
        {
            // Arrange
            SetDisplayEnable(true);
            SetVideoMemoryPointer(videoMemoryPointer);
            var expectedAddress = (videoMemoryPointer << 10) | (index + (row * 40));
            var x = GetXForMac(47 + (index * 2));
            var y = 0x030 + (row*8);
            _memoryValue = cData;

            // Act
            Vic.ClockTo(x, y);

            // Assert
            _address.Should().Be(expectedAddress);
            Vic.VideoMatrix(index).Should().Be(cData);
        }

        [Test]
        public void GraphicsAccessAddresses_Text([Random(0, 39, 2)] int index, [Random(0, 24, 2)] int row,
            [Random(0, 15, 2)] int videoMemoryPointer, [Random(0, 7, 2)] int characterBankPointer, [Random(0, 4095, 2)] int cData)
        {
            // Arrange
            SetDisplayEnable(true);
            SetCharacterBankPointer(characterBankPointer);
            var x = GetXForMac(48 + (index * 2));
            var y = 0x030 + (row * 8);
            var expectedAddress = (characterBankPointer << 11) | ((cData & 0xFF) << 3) | 0;
            _memoryValue = cData;

            // Act
            Vic.ClockTo(x, y);

            // Assert
            _address.Should().Be(expectedAddress);
        }

        [Test]
        public void GraphicsAccessAddresses_Bitmap([Random(0, 39, 2)] int index, [Random(0, 24, 2)] int row,
            [Random(0, 15, 2)] int videoMemoryPointer, [Random(0, 7, 2)] int characterBankPointer, [Random(0, 4095, 2)] int cData)
        {
            // Arrange
            SetBitmapMode(true);
            SetDisplayEnable(true);
            SetCharacterBankPointer(characterBankPointer);
            var x = GetXForMac(48 + (index * 2));
            var y = 0x030 + (row * 8);
            var expectedAddress = ((characterBankPointer & 0x4) << 11) | ((index + (row * 40)) << 3) | 0;
            _memoryValue = cData;

            // Act
            Vic.ClockTo(x, y);

            // Assert
            _address.Should().Be(expectedAddress);
        }

        [Test]
        public void GraphicsAccessAddresses_ECM_Text([Random(0, 39, 2)] int index, [Random(0, 24, 2)] int row,
            [Random(0, 15, 2)] int videoMemoryPointer, [Random(0, 7, 2)] int characterBankPointer, [Random(0, 4095, 2)] int cData)
        {
            // Arrange
            SetExtraColorMode(true);
            SetDisplayEnable(true);
            SetCharacterBankPointer(characterBankPointer);
            var x = GetXForMac(48 + (index * 2));
            var y = 0x030 + (row * 8);
            var expectedAddress = (characterBankPointer << 11) | ((cData & 0x3F) << 3) | 0;
            _memoryValue = cData;

            // Act
            Vic.ClockTo(x, y);

            // Assert
            _address.Should().Be(expectedAddress);
        }

        [Test]
        public void GraphicsAccessAddresses_ECM_Bitmap([Random(0, 39, 2)] int index, [Random(0, 24, 2)] int row,
            [Random(0, 15, 2)] int videoMemoryPointer, [Random(0, 7, 2)] int characterBankPointer, [Random(0, 4095, 2)] int cData)
        {
            // Arrange
            SetExtraColorMode(true);
            SetBitmapMode(true);
            SetDisplayEnable(true);
            SetCharacterBankPointer(characterBankPointer);
            var x = GetXForMac(48 + (index * 2));
            var y = 0x030 + (row * 8);
            var expectedAddress = (((characterBankPointer & 0x4) << 11) | ((index + (row * 40)) << 3) | 0) & 0x39FF;
            _memoryValue = cData;

            // Act
            Vic.ClockTo(x, y);

            // Assert
            _address.Should().Be(expectedAddress);
        }
    }
}
