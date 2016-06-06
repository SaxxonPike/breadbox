using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Breadbox.Test.Cpu6502.Opcode
{
    [TestFixture]
    public class Cpu6502AddressingTests : Cpu6502ExecutionBaseTestFixture
    {
        private enum AccessType
        {
            Read,
            Write
        }

        private class AccessEntry
        {
            public AccessEntry(AccessType accessType, int address)
            {
                AccessType = accessType;
                Address = address;
            }
            public AccessType AccessType { get; private set; }
            public int Address { get; private set; }
        }

        private void Verify(IEnumerable<AccessEntry> accesses)
        {
            Cpu.ForceOpcodeSync();
            MemoryMock.ResetCalls();
            foreach (var access in accesses)
            {
                var address = access.Address;
                Cpu.Clock();
                switch (access.AccessType)
                {
                    case AccessType.Read:
                        Console.WriteLine("Verifying READ at ${0:x4}", address);
                        MemoryMock.Verify(m => m.Read(It.IsIn(address)), Times.Once);
                        MemoryMock.Verify(m => m.Read(It.IsNotIn(address)), Times.Never);
                        MemoryMock.Verify(m => m.Write(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
                        break;
                    case AccessType.Write:
                        Console.WriteLine("Verifying WRITE at ${0:x4}", address);
                        MemoryMock.Verify(m => m.Write(It.IsIn(address), It.IsAny<int>()), Times.Once);
                        MemoryMock.Verify(m => m.Write(It.IsNotIn(address), It.IsAny<int>()), Times.Never);
                        MemoryMock.Verify(m => m.Read(It.IsAny<int>()), Times.Never);
                        break;
                }
                MemoryMock.ResetCalls();
            }
            Cpu.Sync.Should().BeTrue("Sync must occur after opcode finishes.");
        }

        [Test]
        public void Implied([Values(0xCA, 0x88, 0xE8)] int opcode, [Random(0x0000, 0xFFFE, 1)] int address)
        {
            // Arrange
            Cpu.SetPC(address);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            var accesses = new[]
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
            };

            // Assert
            Verify(accesses);
        }

        [Test]
        public void Immediate([Values(0x09, 0x29, 0xE9)] int opcode, [Random(0x0000, 0xFFFE, 1)] int address)
        {
            // Arrange
            Cpu.SetPC(address);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            var accesses = new[]
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
            };

            // Assert
            Verify(accesses);
        }

        [Test]
        public void ZpRead([Values(0x05, 0x25, 0xE5)] int opcode, [Random(0x0000, 0xFFFE, 1)] int address, [Random(0x00, 0xFF, 1)] int zpAddress)
        {
            // Arrange
            Cpu.SetPC(address);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            MemoryMock.Setup(m => m.Read(address + 1)).Returns(zpAddress);
            var accesses = new[]
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
                new AccessEntry(AccessType.Read, zpAddress),
            };

            // Assert
            Verify(accesses);
        }

        [Test]
        public void ZpWrite([Values(0x84, 0x85, 0x86)] int opcode, [Random(0x0000, 0xFFFE, 1)] int address, [Random(0x00, 0xFF, 1)] int zpAddress)
        {
            // Arrange
            Cpu.SetPC(address);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            MemoryMock.Setup(m => m.Read(address + 1)).Returns(zpAddress);
            var accesses = new[]
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
                new AccessEntry(AccessType.Write, zpAddress),
            };

            // Assert
            Verify(accesses);
        }

        [Test]
        public void ZpRmw([Values(0x06, 0x07, 0x26, 0x27, 0x46, 0x47, 0x66, 0x67, 0xC6, 0xC7, 0xE6, 0xE7)] int opcode, [Random(0x0000, 0xFFFE, 1)] int address, [Random(0x00, 0xFF, 1)] int zpAddress)
        {
            // Arrange
            Cpu.SetPC(address);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            MemoryMock.Setup(m => m.Read(address + 1)).Returns(zpAddress);
            var accesses = new[]
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
                new AccessEntry(AccessType.Read, zpAddress),
                new AccessEntry(AccessType.Write, zpAddress),
                new AccessEntry(AccessType.Write, zpAddress)
            };

            // Assert
            Verify(accesses);
        }

        [Test]
        public void ZpxRead([Values(0x15, 0x35, 0x55)] int opcode, [Random(0x0000, 0xFFFE, 1)] int address, [Random(0x00, 0xFF, 1)] int zpAddress, [Random(0x00, 0xFF, 1)] int x)
        {
            // Arrange
            Cpu.SetPC(address);
            Cpu.SetX(x);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            MemoryMock.Setup(m => m.Read(address + 1)).Returns(zpAddress);
            var accesses = new[]
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
                new AccessEntry(AccessType.Read, zpAddress),
                new AccessEntry(AccessType.Read, (zpAddress + x) & 0xFF),
            };

            // Assert
            Verify(accesses);
        }

        [Test]
        public void ZpxWrite([Values(0x94, 0x95)] int opcode, [Random(0x0000, 0xFFFE, 1)] int address, [Random(0x00, 0xFF, 1)] int zpAddress, [Random(0x00, 0xFF, 1)] int x)
        {
            // Arrange
            Cpu.SetPC(address);
            Cpu.SetX(x);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            MemoryMock.Setup(m => m.Read(address + 1)).Returns(zpAddress);
            var accesses = new[]
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
                new AccessEntry(AccessType.Read, zpAddress),
                new AccessEntry(AccessType.Write, (zpAddress + x) & 0xFF),
            };

            // Assert
            Verify(accesses);
        }

        [Test]
        public void ZpxRmw([Values(0x16, 0x36, 0x56)] int opcode, [Random(0x0000, 0xFFFE, 1)] int address, [Random(0x00, 0xFF, 1)] int zpAddress, [Random(0x00, 0xFF, 1)] int x)
        {
            // Arrange
            Cpu.SetPC(address);
            Cpu.SetX(x);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            MemoryMock.Setup(m => m.Read(address + 1)).Returns(zpAddress);
            var accesses = new[]
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
                new AccessEntry(AccessType.Read, zpAddress),
                new AccessEntry(AccessType.Read, (zpAddress + x) & 0xFF),
                new AccessEntry(AccessType.Write, (zpAddress + x) & 0xFF),
                new AccessEntry(AccessType.Write, (zpAddress + x) & 0xFF),
            };

            // Assert
            Verify(accesses);
        }

        [Test]
        public void ZpyRead([Values(0xB6, 0xB7)] int opcode, [Random(0x0000, 0xFFFE, 1)] int address, [Random(0x00, 0xFF, 1)] int zpAddress, [Random(0x00, 0xFF, 1)] int y)
        {
            // Arrange
            Cpu.SetPC(address);
            Cpu.SetY(y);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            MemoryMock.Setup(m => m.Read(address + 1)).Returns(zpAddress);
            var accesses = new[]
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
                new AccessEntry(AccessType.Read, zpAddress),
                new AccessEntry(AccessType.Read, (zpAddress + y) & 0xFF),
            };

            // Assert
            Verify(accesses);
        }

        [Test]
        public void ZpyWrite([Values(0x96, 0x97)] int opcode, [Random(0x0000, 0xFFFE, 1)] int address, [Random(0x00, 0xFF, 1)] int zpAddress, [Random(0x00, 0xFF, 1)] int y)
        {
            // Arrange
            Cpu.SetPC(address);
            Cpu.SetY(y);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            MemoryMock.Setup(m => m.Read(address + 1)).Returns(zpAddress);
            var accesses = new[]
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
                new AccessEntry(AccessType.Read, zpAddress),
                new AccessEntry(AccessType.Write, (zpAddress + y) & 0xFF),
            };

            // Assert
            Verify(accesses);
        }

        [Test]
        public void AbsRead([Values(0x0D, 0x2D, 0x4D, 0x6D, 0xAD, 0xCD, 0xED)] int opcode, [Random(0x0000, 0xFFFE, 1)] int address, [Random(0x0000, 0xFFFF, 1)] int absAddress)
        {
            // Arrange
            Cpu.SetPC(address);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            MemoryMock.Setup(m => m.Read(address + 1)).Returns(absAddress & 0xFF);
            MemoryMock.Setup(m => m.Read(address + 2)).Returns((absAddress >> 8) & 0xFF);
            var accesses = new[]
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
                new AccessEntry(AccessType.Read, address + 2),
                new AccessEntry(AccessType.Read, absAddress)
            };

            // Assert
            Verify(accesses);
        }

        [Test]
        public void AbsWrite([Values(0x8C, 0x8D, 0x8E, 0x8F)] int opcode, [Random(0x0000, 0xFFFE, 1)] int address, [Random(0x0000, 0xFFFF, 1)] int absAddress)
        {
            // Arrange
            Cpu.SetPC(address);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            MemoryMock.Setup(m => m.Read(address + 1)).Returns(absAddress & 0xFF);
            MemoryMock.Setup(m => m.Read(address + 2)).Returns((absAddress >> 8) & 0xFF);
            var accesses = new[]
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
                new AccessEntry(AccessType.Read, address + 2),
                new AccessEntry(AccessType.Write, absAddress)
            };

            // Assert
            Verify(accesses);
        }

        [Test]
        public void AbsRmw([Values(0x0E, 0x0F, 0x2E, 0x2F, 0x4E, 0x4F, 0x6E, 0x6F, 0xCE, 0xCF, 0xEE, 0xEF)] int opcode, [Random(0x0000, 0xFFFE, 1)] int address, [Random(0x0000, 0xFFFF, 1)] int absAddress)
        {
            // Arrange
            Cpu.SetPC(address);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            MemoryMock.Setup(m => m.Read(address + 1)).Returns(absAddress & 0xFF);
            MemoryMock.Setup(m => m.Read(address + 2)).Returns((absAddress >> 8) & 0xFF);
            var accesses = new[]
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
                new AccessEntry(AccessType.Read, address + 2),
                new AccessEntry(AccessType.Read, absAddress),
                new AccessEntry(AccessType.Write, absAddress),
                new AccessEntry(AccessType.Write, absAddress)
            };

            // Assert
            Verify(accesses);
        }

        [Test]
        public void AbxRead([Values(0x1C, 0x1D, 0x3C, 0x3D, 0x5C, 0x5D, 0x7C, 0x7D, 0xBC, 0xBD, 0xDC, 0xDD, 0xFC, 0xFD)] int opcode, [Random(0x0000, 0xFFFE, 1)] int address, [Random(0x0000, 0xFFFF, 1)] int absAddress, [Values(0x00, 0xFF)] int x)
        {
            // Arrange
            address |= 0x80;
            Cpu.SetPC(address);
            Cpu.SetX(x);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            MemoryMock.Setup(m => m.Read(address + 1)).Returns(absAddress & 0xFF);
            MemoryMock.Setup(m => m.Read(address + 2)).Returns((absAddress >> 8) & 0xFF);
            var accesses = new List<AccessEntry>
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
                new AccessEntry(AccessType.Read, address + 2),
                new AccessEntry(AccessType.Read, (absAddress & 0xFF00) | ((absAddress + x) & 0xFF))
            };
            if ((absAddress & 0xFF) + x >= 0x100)
            {
                accesses.Add(new AccessEntry(AccessType.Read, (absAddress + x) & 0xFFFF));
            }

            // Assert
            Verify(accesses);
        }

        [Test]
        public void AbxWrite([Values(0x9D)] int opcode, [Random(0x0000, 0xFFFE, 1)] int address, [Random(0x0000, 0xFFFF, 1)] int absAddress, [Values(0x00, 0xFF)] int x)
        {
            // Arrange
            address |= 0x80;
            Cpu.SetPC(address);
            Cpu.SetX(x);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            MemoryMock.Setup(m => m.Read(address + 1)).Returns(absAddress & 0xFF);
            MemoryMock.Setup(m => m.Read(address + 2)).Returns((absAddress >> 8) & 0xFF);
            var accesses = new List<AccessEntry>
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
                new AccessEntry(AccessType.Read, address + 2),
                new AccessEntry(AccessType.Read, (absAddress & 0xFF00) | ((absAddress + x) & 0xFF)),
                new AccessEntry(AccessType.Write, (absAddress + x) & 0xFFFF)
            };

            // Assert
            Verify(accesses);
        }

        [Test]
        public void AbxRmw([Values(0x1E, 0x1F, 0x3E, 0x3F, 0x5E, 0x5F, 0x7E, 0x7F, 0xDE, 0xDF, 0xFE, 0xFF)] int opcode, [Random(0x0000, 0xFFFE, 1)] int address, [Random(0x0000, 0xFFFF, 1)] int absAddress, [Values(0x00, 0xFF)] int x)
        {
            // Arrange
            address |= 0x80;
            Cpu.SetPC(address);
            Cpu.SetX(x);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            MemoryMock.Setup(m => m.Read(address + 1)).Returns(absAddress & 0xFF);
            MemoryMock.Setup(m => m.Read(address + 2)).Returns((absAddress >> 8) & 0xFF);
            var accesses = new List<AccessEntry>
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
                new AccessEntry(AccessType.Read, address + 2),
                new AccessEntry(AccessType.Read, (absAddress & 0xFF00) | ((absAddress + x) & 0xFF)),
                new AccessEntry(AccessType.Read, (absAddress + x) & 0xFFFF),
                new AccessEntry(AccessType.Write, (absAddress + x) & 0xFFFF),
                new AccessEntry(AccessType.Write, (absAddress + x) & 0xFFFF)
            };

            // Assert
            Verify(accesses);
        }

        [Test]
        public void AbyRead([Values(0x19, 0x39, 0x59, 0x79, 0xB9, 0xBB, 0xBE, 0xBF, 0xD9, 0xF9)] int opcode, [Random(0x0000, 0xFFFE, 1)] int address, [Random(0x0000, 0xFFFF, 1)] int absAddress, [Values(0x00, 0xFF)] int y)
        {
            // Arrange
            address |= 0x80;
            Cpu.SetPC(address);
            Cpu.SetY(y);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            MemoryMock.Setup(m => m.Read(address + 1)).Returns(absAddress & 0xFF);
            MemoryMock.Setup(m => m.Read(address + 2)).Returns((absAddress >> 8) & 0xFF);
            var accesses = new List<AccessEntry>
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
                new AccessEntry(AccessType.Read, address + 2),
                new AccessEntry(AccessType.Read, (absAddress & 0xFF00) | ((absAddress + y) & 0xFF))
            };
            if ((absAddress & 0xFF) + y >= 0x100)
            {
                accesses.Add(new AccessEntry(AccessType.Read, (absAddress + y) & 0xFFFF));
            }

            // Assert
            Verify(accesses);
        }

        [Test]
        public void AbyWrite([Values(0x99, 0x9B)] int opcode, [Random(0x0000, 0xFFFE, 1)] int address, [Random(0x0000, 0xFFFF, 1)] int absAddress, [Values(0x00, 0xFF)] int y)
        {
            // Arrange
            address |= 0x80;
            Cpu.SetPC(address);
            Cpu.SetY(y);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            MemoryMock.Setup(m => m.Read(address + 1)).Returns(absAddress & 0xFF);
            MemoryMock.Setup(m => m.Read(address + 2)).Returns((absAddress >> 8) & 0xFF);
            var accesses = new List<AccessEntry>
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
                new AccessEntry(AccessType.Read, address + 2),
                new AccessEntry(AccessType.Read, (absAddress & 0xFF00) | ((absAddress + y) & 0xFF)),
                new AccessEntry(AccessType.Write, (absAddress + y) & 0xFFFF)
            };

            // Assert
            Verify(accesses);
        }

        [Test]
        public void AbyRmw([Values(0x1B, 0x3B, 0x5B, 0x7B, 0xDB, 0xFB)] int opcode, [Random(0x0000, 0xFFFE, 1)] int address, [Random(0x0000, 0xFFFF, 1)] int absAddress, [Values(0x00, 0xFF)] int y)
        {
            // Arrange
            address |= 0x80;
            Cpu.SetPC(address);
            Cpu.SetY(y);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            MemoryMock.Setup(m => m.Read(address + 1)).Returns(absAddress & 0xFF);
            MemoryMock.Setup(m => m.Read(address + 2)).Returns((absAddress >> 8) & 0xFF);
            var accesses = new List<AccessEntry>
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
                new AccessEntry(AccessType.Read, address + 2),
                new AccessEntry(AccessType.Read, (absAddress & 0xFF00) | ((absAddress + y) & 0xFF)),
                new AccessEntry(AccessType.Read, (absAddress + y) & 0xFFFF),
                new AccessEntry(AccessType.Write, (absAddress + y) & 0xFFFF),
                new AccessEntry(AccessType.Write, (absAddress + y) & 0xFFFF)
            };

            // Assert
            Verify(accesses);
        }

        [Test]
        public void IzxRead([Values(0x01, 0x21, 0x41, 0x61, 0xA1, 0xA3, 0xC1, 0xE1)] int opcode, [Random(0x0100, 0xFFFE, 1)] int address, [Random(0x00, 0xFF, 1)] int zpAddress, [Random(0x0000, 0xFFFF, 1)] int absAddress, [Values(0x00, 0xFF)] int x)
        {
            // Arrange
            address |= 0x80;
            Cpu.SetPC(address);
            Cpu.SetX(x);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            MemoryMock.Setup(m => m.Read(address + 1)).Returns(zpAddress);
            MemoryMock.Setup(m => m.Read((zpAddress + x) & 0xFF)).Returns(absAddress & 0xFF);
            MemoryMock.Setup(m => m.Read((zpAddress + x + 1) & 0xFF)).Returns((absAddress >> 8) & 0xFF);

            var accesses = new List<AccessEntry>
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
                new AccessEntry(AccessType.Read, zpAddress),
                new AccessEntry(AccessType.Read, (zpAddress + x) & 0xFF),
                new AccessEntry(AccessType.Read, (zpAddress + x + 1) & 0xFF),
                new AccessEntry(AccessType.Read, absAddress)
            };

            // Assert
            Verify(accesses);
        }

        [Test]
        public void IzxWrite([Values(0x81, 0x83)] int opcode, [Random(0x0100, 0xFFFE, 1)] int address, [Random(0x00, 0xFF, 1)] int zpAddress, [Random(0x0100, 0xFFFF, 1)] int absAddress, [Values(0x00, 0xFF)] int x)
        {
            // Arrange
            address |= 0x80;
            Cpu.SetPC(address);
            Cpu.SetX(x);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            MemoryMock.Setup(m => m.Read(address + 1)).Returns(zpAddress);
            MemoryMock.Setup(m => m.Read((zpAddress + x) & 0xFF)).Returns(absAddress & 0xFF);
            MemoryMock.Setup(m => m.Read((zpAddress + x + 1) & 0xFF)).Returns((absAddress >> 8) & 0xFF);

            var accesses = new List<AccessEntry>
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
                new AccessEntry(AccessType.Read, zpAddress),
                new AccessEntry(AccessType.Read, (zpAddress + x) & 0xFF),
                new AccessEntry(AccessType.Read, (zpAddress + x + 1) & 0xFF),
                new AccessEntry(AccessType.Write, absAddress)
            };

            // Assert
            Verify(accesses);
        }

        [Test]
        public void IzxRmw([Values(0x03, 0x23, 0x43, 0x63, 0xC3, 0xE3)] int opcode, [Random(0x0100, 0xFFFE, 1)] int address, [Random(0x00, 0xFF, 1)] int zpAddress, [Random(0x0000, 0xFFFF, 1)] int absAddress, [Values(0x00, 0xFF)] int x)
        {
            // Arrange
            address |= 0x80;
            Cpu.SetPC(address);
            Cpu.SetX(x);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            MemoryMock.Setup(m => m.Read(address + 1)).Returns(zpAddress);
            MemoryMock.Setup(m => m.Read((zpAddress + x) & 0xFF)).Returns(absAddress & 0xFF);
            MemoryMock.Setup(m => m.Read((zpAddress + x + 1) & 0xFF)).Returns((absAddress >> 8) & 0xFF);

            var accesses = new List<AccessEntry>
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
                new AccessEntry(AccessType.Read, zpAddress),
                new AccessEntry(AccessType.Read, (zpAddress + x) & 0xFF),
                new AccessEntry(AccessType.Read, (zpAddress + x + 1) & 0xFF),
                new AccessEntry(AccessType.Read, absAddress),
                new AccessEntry(AccessType.Write, absAddress),
                new AccessEntry(AccessType.Write, absAddress)
            };

            // Assert
            Verify(accesses);
        }

        [Test]
        public void IzyRead([Values(0x11, 0x31, 0x51, 0x71, 0xB1, 0xB3, 0xD1, 0xF1)] int opcode, [Random(0x0000, 0xFFFE, 1)] int address, [Random(0x00, 0xFF, 1)] int zpAddress, [Random(0x00, 0xFF, 1)] int absAddress, [Values(0x00, 0xFF)] int y)
        {
            // Arrange
            address |= 0x80;
            Cpu.SetPC(address);
            Cpu.SetY(y);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            MemoryMock.Setup(m => m.Read(address + 1)).Returns(zpAddress);
            MemoryMock.Setup(m => m.Read(zpAddress)).Returns(absAddress & 0xFF);
            MemoryMock.Setup(m => m.Read((zpAddress + 1) & 0xFF)).Returns((absAddress >> 8) & 0xFF);

            var accesses = new List<AccessEntry>
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
                new AccessEntry(AccessType.Read, zpAddress),
                new AccessEntry(AccessType.Read, (zpAddress + 1) & 0xFF),
                new AccessEntry(AccessType.Read, (absAddress & 0xFF00) | ((absAddress + y) & 0xFF))
            };
            if ((absAddress & 0xFF) + y >= 0x100)
            {
                accesses.Add(new AccessEntry(AccessType.Read, (absAddress + y) & 0xFFFF));
            }

            // Assert
            Verify(accesses);
        }

        [Test]
        public void IzyWrite([Values(0x91)] int opcode, [Random(0x0000, 0xFFFE, 1)] int address, [Random(0x00, 0xFF, 1)] int zpAddress, [Random(0x00, 0xFF, 1)] int absAddress, [Values(0x00, 0xFF)] int y)
        {
            // Arrange
            address |= 0x80;
            Cpu.SetPC(address);
            Cpu.SetY(y);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            MemoryMock.Setup(m => m.Read(address + 1)).Returns(zpAddress);
            MemoryMock.Setup(m => m.Read(zpAddress)).Returns(absAddress & 0xFF);
            MemoryMock.Setup(m => m.Read((zpAddress + 1) & 0xFF)).Returns((absAddress >> 8) & 0xFF);

            var accesses = new List<AccessEntry>
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
                new AccessEntry(AccessType.Read, zpAddress),
                new AccessEntry(AccessType.Read, (zpAddress + 1) & 0xFF),
                new AccessEntry(AccessType.Read, (absAddress & 0xFF00) | ((absAddress + y) & 0xFF)),
                new AccessEntry(AccessType.Write, (absAddress + y) & 0xFFFF)
            };

            // Assert
            Verify(accesses);
        }

        [Test]
        public void IzyRmw([Values(0x13, 0x33, 0x53, 0x73, 0xD3, 0xF3)] int opcode, [Random(0x0000, 0xFFFE, 1)] int address, [Random(0x00, 0xFF, 1)] int zpAddress, [Random(0x00, 0xFF, 1)] int absAddress, [Values(0x00, 0xFF)] int y)
        {
            // Arrange
            address |= 0x80;
            Cpu.SetPC(address);
            Cpu.SetY(y);
            MemoryMock.Setup(m => m.Read(address)).Returns(opcode);
            MemoryMock.Setup(m => m.Read(address + 1)).Returns(zpAddress);
            MemoryMock.Setup(m => m.Read(zpAddress)).Returns(absAddress & 0xFF);
            MemoryMock.Setup(m => m.Read((zpAddress + 1) & 0xFF)).Returns((absAddress >> 8) & 0xFF);

            var accesses = new List<AccessEntry>
            {
                new AccessEntry(AccessType.Read, address),
                new AccessEntry(AccessType.Read, address + 1),
                new AccessEntry(AccessType.Read, zpAddress),
                new AccessEntry(AccessType.Read, (zpAddress + 1) & 0xFF),
                new AccessEntry(AccessType.Read, (absAddress & 0xFF00) | ((absAddress + y) & 0xFF)),
                new AccessEntry(AccessType.Read, (absAddress + y) & 0xFFFF),
                new AccessEntry(AccessType.Write, (absAddress + y) & 0xFFFF),
                new AccessEntry(AccessType.Write, (absAddress + y) & 0xFFFF)
            };

            // Assert
            Verify(accesses);
        }

    }
}
