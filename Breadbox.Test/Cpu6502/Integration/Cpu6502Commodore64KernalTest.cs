using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Breadbox.Test.Properties;
using NUnit.Framework;

namespace Breadbox.Test.Cpu6502.Integration
{
    public class Cpu6502Commodore64KernalTest
    {
        private class MockPla : IMemory
        {
            private readonly int[] _memory = new int[0x10000];
            private readonly Random _random = new Random();

            public byte[] Dump()
            {
                return Enumerable.Range(0, 0x10000).Select(Read).Select(b => (byte) (b & 0xFF)).ToArray();
            }

            public int Read(int address)
            {
                switch (address)
                {
                    case 0xD012:
                        return _random.Next(256);
                    default:
                        if (address >= 0xD400 && address < 0xD800)
                            return 0xFF;
                        if (address >= 0xDC00 && address < 0xE000)
                            return 0xFF;
                        if (address >= 0xA000 && address < 0xC000)
                            return Resources.basic_901226_01[address & 0x1FFF];
                        return address >= 0xE000
                            ? Resources.kernal_901227_03[address & 0x1FFF]
                            : _memory[address];
                }
            }

            public void Write(int address, int value)
            {
                _memory[address] = value & 0xFF;
            }

            public int Peek(int address)
            {
                return Read(address);
            }

            public void Poke(int address, int value)
            {
                Write(address, value);
            }
        }

        [Test]
        [Explicit("Manual only.")]
        public void Test1()
        {
            // Arrange
            var traceEnabled = false;
            var memory = new MockPla();
            var memoryTrace = new MemoryTrace(memory,
                address =>
                {
                    if (traceEnabled)
                        Console.WriteLine("READ  ${0:x4} -> #${1:x2}", address, memory.Peek(address));
                },
                (address, value) =>
                {
                    if (traceEnabled)
                        Console.WriteLine("WRITE ${0:x4} <- #${1:x2}", address, value);
                });

            var cpu = new Mos6502(new Mos6502Configuration(0xFF, true), memoryTrace, new ReadySignalNull());

            // Act
            cpu.ClockMultiple(3000000);
            //traceEnabled = true;
            //cpu.ClockMultiple(5000);

            // Assert
            var dump = memory.Dump();
            File.WriteAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "dump.bin"), dump);
        }
    }
}
