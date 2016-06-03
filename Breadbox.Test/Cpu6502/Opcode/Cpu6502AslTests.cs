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
    public class Cpu6502AslTests : Cpu6502OpcodeBaseTestFixture
    {
        public Cpu6502AslTests() : base(0x06)
        {
        }

        [Test]
        public void Asl([Range(0x0, 0xF, 0x5)] int lowA, [Range(0x0, 0xF, 0x5)] int highA, [Range(0x0, 0xF, 0x5)] int lowOperand, [Range(0x0, 0xF, 0x5)] int highOperand)
        {
            // Arrange
            var a = lowA + (highA << 4);
            var operand = lowOperand + (highOperand << 4);
            var expectedResult = operand << 1;
            var expectedSign = (expectedResult & 0x80) != 0;
            var expectedZero = (expectedResult & 0xFF) == 0;
            var expectedOverflow = Cpu.V;
            var expectedCarry = (expectedResult & 0x100) != 0;
            expectedResult &= 0xFF;
            Cpu.SetA(a);
            MemoryMock.SetupSequence(m => m.Read(It.IsAny<int>()))
                .Returns(0x00)
                .Returns(operand);

            // Act
            Cpu.ClockStep();
            Console.WriteLine("ASL {0:x2} should = {1:x2} with Carry {2}", operand, expectedResult, expectedCarry);

            // Assert
            Cpu.V.Should().Be(expectedOverflow, "V must not be modified");
            Cpu.Z.Should().Be(expectedZero, "Z must be set correctly");
            Cpu.N.Should().Be(expectedSign, "N must be set correctly");
            Cpu.C.Should().Be(expectedCarry, "C must be set correctly");
            Cpu.A.Should().Be(a, "A must not be modified");
        }

        [Test]
        public void AslA([Range(0x0, 0xF, 0x5)] int lowA, [Range(0x0, 0xF, 0x5)] int highA)
        {
            // Arrange
            var a = lowA + (highA << 4);
            var expectedResult = a << 1;
            var expectedSign = (expectedResult & 0x80) != 0;
            var expectedZero = (expectedResult & 0xFF) == 0;
            var expectedOverflow = Cpu.V;
            var expectedCarry = (expectedResult & 0x100) != 0;
            expectedResult &= 0xFF;
            Cpu.SetA(a);
            Cpu.SetOpcode(0x0A);

            // Act
            Cpu.ClockStep();
            Console.WriteLine("ASL A [{0:x2}] should = {1:x2} with Carry {2}", a, expectedResult, expectedCarry);

            // Assert
            Cpu.V.Should().Be(expectedOverflow, "V must not be modified");
            Cpu.Z.Should().Be(expectedZero, "Z must be set correctly");
            Cpu.N.Should().Be(expectedSign, "N must be set correctly");
            Cpu.C.Should().Be(expectedCarry, "C must be set correctly");
            Cpu.A.Should().Be(expectedResult, "A must be set correctly");
        }
    }
}
