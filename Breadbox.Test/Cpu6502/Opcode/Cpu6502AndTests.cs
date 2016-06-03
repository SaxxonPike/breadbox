using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Breadbox.Test.Cpu6502.Opcode
{
    public class Cpu6502AndTests : Cpu6502OpcodeBaseTestFixture
    {
        public Cpu6502AndTests() : base(0x29)
        {
        }

        [Test]
        public void And([Range(0x0, 0xF, 0x5)] int lowA, [Range(0x0, 0xF, 0x5)] int highA, [Range(0x0, 0xF, 0x5)] int lowOperand, [Range(0x0, 0xF, 0x5)] int highOperand)
        {
            // Arrange
            var a = lowA + (highA << 4);
            var operand = lowOperand + (highOperand << 4);
            var expectedResult = a & operand;
            var expectedSign = (expectedResult & 0x80) != 0;
            var expectedZero = (expectedResult & 0xFF) == 0;
            var expectedOverflow = Cpu.V;
            var expectedCarry = Cpu.C;
            Cpu.SetA(a);
            MemoryMock.SetupSequence(m => m.Read(It.IsAny<int>()))
                .Returns(operand);

            // Act
            Cpu.ClockStep();
            Console.WriteLine("{0:x2} & {1:x2} should = {2:x2}", a, operand, expectedResult);

            // Assert
            Cpu.V.Should().Be(expectedOverflow, "V must not be modified");
            Cpu.Z.Should().Be(expectedZero, "Z must be set correctly");
            Cpu.N.Should().Be(expectedSign, "N must be set correctly");
            Cpu.C.Should().Be(expectedCarry, "C must not be modified");
            Cpu.A.Should().Be(expectedResult & 0xFF, "A must be set correctly");
        }
    }
}
