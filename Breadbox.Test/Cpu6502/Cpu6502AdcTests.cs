using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Breadbox.Test.Cpu6502
{
    [TestFixture]
    public class Cpu6502AdcTests : Cpu6502BaseTestFixture
    {
        protected override void SetUpMocks()
        {
            base.SetUpMocks();
            MemoryMock = new Mock<IMemory>();
        }

        [Test]
        public void AdcImm_WithoutDecimalMode([Random(0, 255, 8)] int a, [Random(0, 255, 8)] int operand, [Random(0, 65535, 1)] int address)
        {
            var expectedResult = a + operand;
            var expectedOverflow = ((a < 128) && (expectedResult >= 128)) || ((a >= 128) && (expectedResult >= 384));
            var expectedCarry = expectedResult >= 256;
            var expectedZero = (expectedResult & 0xFF) == 0;
            var expectedSign = expectedResult >= 128;

            Cpu.SetD(false);
            Cpu.SetA(a);
            MockColdStartReadSequence(0x69, operand);
            Cpu.ClockStep();
            Cpu.ClockStep();

            Cpu.V.Should().Be(expectedOverflow);
            Cpu.Z.Should().Be(expectedZero);
            Cpu.N.Should().Be(expectedSign);
            Cpu.C.Should().Be(expectedCarry);
        }
    }
}
