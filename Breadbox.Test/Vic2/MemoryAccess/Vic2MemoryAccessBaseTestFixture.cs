using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BreadboxF;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Breadbox.Test.Vic2
{
    [TestFixture]
    public abstract class Vic2MemoryAccessBaseTestFixture : Vic2BaseTestFixture
    {
        [SetUp]
        public void Configure()
        {
            EnablePersistentMemory();
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
        public void SpritePointerAccessAddresses([Random(0, 15, 1)] int videoMemoryPointer, [Range(0, 7)] int index)
        {
            // Arrange
            var expectedAddress = (videoMemoryPointer << 10) | (0x3F8) | index;
            var x = GetXForMac(6 + (index*4));
            SetVideoMemoryPointer(videoMemoryPointer);

            // Act
            Vic.ClockToCounterX(x);

            // Assert
            LastAccessedAddress.Should().Be(expectedAddress);
        }

        [Test]
        public void SpriteDataAccessAddresses([Random(0, 255, 1)] int mobPointer, [Range(0, 2)] int counter, [Range(0, 7)] int index)
        {
            // Arrange
            var address = -1;
            var expectedAddress = (mobPointer << 6) | counter;
            var x = GetXForMac(7 + counter + (index * 4));
            MemoryMock.Setup(m => m.Read(It.IsAny<int>())).Returns(mobPointer).Callback<int>(a => address = a);

            // Act
            Vic.ClockToCounterX(0x000);
            Vic.PokeRegister(index * 2, 0x100);
            Vic.PokeRegister((index * 2) + 1, Vic.RasterY);
            Vic.PokeRegister(0x15, 1 << index);
            Vic.ClockToCounterX(x);

            // Assert
            address.Should().Be(expectedAddress);
        }

        [Test]
        public void RefreshAccessAddresses([Range(0, 4)] int index)
        {
            // Arrange
            var expectedAddress = 0x3FFF - index;
            var x = GetXForMac(38 + (index * 2));

            // Act
            Vic.ClockToCounterX(x);

            // Assert
            LastAccessedAddress.Should().Be(expectedAddress);
        }

        [Test]
        public void ColorAccessAddresses([Range(0, 39)] int index, [Range(0, 1)] int row, [Random(0, 15, 1)] int videoMemoryPointer, [Random(0, 4095, 1)] int cData)
        {
            // Arrange
            SetDisplayEnable(true);
            SetVideoMemoryPointer(videoMemoryPointer);
            var address = -1;
            var expectedAddress = (videoMemoryPointer << 10) | (index + (row * 40));
            var x = GetXForMac(47 + (index * 2));
            var y = 0x030 + (row*8);
            MemoryMock.Setup(m => m.Read(It.IsAny<int>())).Returns(cData).Callback<int>(a => address = a);

            // Act
            Vic.ClockTo(x, y);

            // Assert
            address.Should().Be(expectedAddress);
            Vic.VideoMatrix(index).Should().Be(cData);
        }

        [Test]
        public void GraphicsAccessAddresses_StandardText([Range(0, 39)] int index, [Range(0, 1)] int row,
            [Random(0, 15, 1)] int videoMemoryPointer, [Random(0, 7, 1)] int characterBankPointer, [Random(0, 4095, 1)] int cData)
        {
            // Arrange
            SetDisplayEnable(true);
            SetCharacterBankPointer(characterBankPointer);
            var address = -1;
            var x = GetXForMac(48 + (index * 2));
            var y = 0x030 + (row * 8);
            var expectedAddress = (characterBankPointer << 11) | ((cData & 0xFF) << 3) | 0;
            MemoryMock.Setup(m => m.Read(It.IsAny<int>())).Returns(cData).Callback<int>(a => address = a);

            // Act
            Vic.ClockTo(x, y);

            // Assert
            address.Should().Be(expectedAddress);
        }


    }
}
