﻿using Moq;
using NUnit.Framework;

namespace Breadbox.Test.Cpu6502.Opcode
{
    public abstract class Cpu6502OpcodeBaseTestFixture : Cpu6502BaseTestFixture
    {
        private readonly int _opcode;

        protected Cpu6502OpcodeBaseTestFixture(int opcode)
        {
            _opcode = opcode;
        }

        protected override void SetUpMocks()
        {
            base.SetUpMocks();
            MemoryMock = new Mock<IMemory>();
        }

        [SetUp]
        public void SetupOpcode()
        {
            Cpu.SetOpcode(_opcode);
        }
    }
}