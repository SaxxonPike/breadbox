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
    public class Cpu6502ShiftTests : Cpu6502OpcodeBaseTestFixture
    {
        public Cpu6502ShiftTests() : base(-1)
        {
        }

        [Test]
        public void Asl([Range(0x0, 0xF, 0x5)] int lowOperand, [Range(0x0, 0xF, 0x5)] int highOperand)
        {
            // Arrange
            var operand = lowOperand + (highOperand << 4);
            var expectedResult = operand << 1;
            var expectedSign = (expectedResult & 0x80) != 0;
            var expectedZero = (expectedResult & 0xFF) == 0;
            var expectedOverflow = Cpu.V;
            var expectedCarry = (expectedResult & 0x100) != 0;
            expectedResult &= 0xFF;
            MemoryMock.SetupSequence(m => m.Read(It.IsAny<int>()))
                .Returns(0x00)
                .Returns(operand);
            Cpu.SetOpcode(0x06);

            // Act
            Cpu.ClockStep();
            Console.WriteLine("ASL {0:x2} should = {1:x2} with Carry {2}", operand, expectedResult, expectedCarry);

            // Assert
            Cpu.V.Should().Be(expectedOverflow, "V must not be modified");
            Cpu.Z.Should().Be(expectedZero, "Z must be set correctly");
            Cpu.N.Should().Be(expectedSign, "N must be set correctly");
            Cpu.C.Should().Be(expectedCarry, "C must be set correctly");
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

        [Test]
        public void Lsr([Range(0x0, 0xF, 0x5)] int lowOperand, [Range(0x0, 0xF, 0x5)] int highOperand)
        {
            // Arrange
            var operand = lowOperand + (highOperand << 4);
            var expectedResult = operand >> 1;
            var expectedSign = (expectedResult & 0x80) != 0;
            var expectedZero = (expectedResult & 0xFF) == 0;
            var expectedOverflow = Cpu.V;
            var expectedCarry = (operand & 0x01) != 0;
            MemoryMock.SetupSequence(m => m.Read(It.IsAny<int>()))
                .Returns(0x00)
                .Returns(operand);
            Cpu.SetOpcode(0x46);

            // Act
            Cpu.ClockStep();
            Console.WriteLine("LSR {0:x2} should = {1:x2} with Carry {2}", operand, expectedResult, expectedCarry);

            // Assert
            Cpu.V.Should().Be(expectedOverflow, "V must not be modified");
            Cpu.Z.Should().Be(expectedZero, "Z must be set correctly");
            Cpu.N.Should().Be(expectedSign, "N must be set correctly");
            Cpu.C.Should().Be(expectedCarry, "C must be set correctly");
        }

        [Test]
        public void LsrA([Range(0x0, 0xF, 0x5)] int lowA, [Range(0x0, 0xF, 0x5)] int highA)
        {
            // Arrange
            var a = lowA + (highA << 4);
            var expectedResult = a >> 1;
            var expectedSign = (expectedResult & 0x80) != 0;
            var expectedZero = (expectedResult & 0xFF) == 0;
            var expectedOverflow = Cpu.V;
            var expectedCarry = (a & 0x01) != 0;
            expectedResult &= 0xFF;
            Cpu.SetA(a);
            Cpu.SetOpcode(0x4A);

            // Act
            Cpu.ClockStep();
            Console.WriteLine("LSR A [{0:x2}] should = {1:x2} with Carry {2}", a, expectedResult, expectedCarry);

            // Assert
            Cpu.V.Should().Be(expectedOverflow, "V must not be modified");
            Cpu.Z.Should().Be(expectedZero, "Z must be set correctly");
            Cpu.N.Should().Be(expectedSign, "N must be set correctly");
            Cpu.C.Should().Be(expectedCarry, "C must be set correctly");
            Cpu.A.Should().Be(expectedResult, "A must be set correctly");
        }

        [Test]
        public void Rol([Range(0x0, 0xF, 0x5)] int lowOperand, [Range(0x0, 0xF, 0x5)] int highOperand, [Range(0, 1)] int carry)
        {
            // Arrange
            var operand = lowOperand + (highOperand << 4);
            var expectedResult = (operand << 1) + carry;
            var expectedSign = (expectedResult & 0x80) != 0;
            var expectedZero = (expectedResult & 0xFF) == 0;
            var expectedOverflow = Cpu.V;
            var expectedCarry = (expectedResult & 0x100) != 0;
            MemoryMock.SetupSequence(m => m.Read(It.IsAny<int>()))
                .Returns(0x00)
                .Returns(operand);
            Cpu.SetOpcode(0x26);
            Cpu.SetC(carry != 0);
            expectedResult &= 0xFF;

            // Act
            Cpu.ClockStep();
            Console.WriteLine("ROL {0:x2} should = {1:x2} with Carry {2}", operand, expectedResult, expectedCarry);

            // Assert
            Cpu.V.Should().Be(expectedOverflow, "V must not be modified");
            Cpu.Z.Should().Be(expectedZero, "Z must be set correctly");
            Cpu.N.Should().Be(expectedSign, "N must be set correctly");
            Cpu.C.Should().Be(expectedCarry, "C must be set correctly");
        }

        [Test]
        public void RolA([Range(0x0, 0xF, 0x5)] int lowA, [Range(0x0, 0xF, 0x5)] int highA, [Range(0, 1)] int carry)
        {
            // Arrange
            var a = lowA + (highA << 4);
            var expectedResult = (a << 1) + carry;
            var expectedSign = (expectedResult & 0x80) != 0;
            var expectedZero = (expectedResult & 0xFF) == 0;
            var expectedOverflow = Cpu.V;
            var expectedCarry = (expectedResult & 0x100) != 0;
            expectedResult &= 0xFF;
            Cpu.SetA(a);
            Cpu.SetOpcode(0x2A);
            Cpu.SetC(carry != 0);

            // Act
            Cpu.ClockStep();
            Console.WriteLine("ROL A [{0:x2}] should = {1:x2} with Carry {2}", a, expectedResult, expectedCarry);

            // Assert
            Cpu.V.Should().Be(expectedOverflow, "V must not be modified");
            Cpu.Z.Should().Be(expectedZero, "Z must be set correctly");
            Cpu.N.Should().Be(expectedSign, "N must be set correctly");
            Cpu.C.Should().Be(expectedCarry, "C must be set correctly");
            Cpu.A.Should().Be(expectedResult, "A must be set correctly");
        }

    }
}
