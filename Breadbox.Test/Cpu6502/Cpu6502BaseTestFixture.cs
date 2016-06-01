using System;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Compatibility;

namespace Breadbox.Test.Cpu6502
{
    [TestFixture]
    public abstract class Cpu6502BaseTestFixture
    {
        private Mos6502Configuration _config;
        private Stopwatch _stopwatch;

        protected Mock<IMemory> MemoryMock;
        protected Mock<IReadySignal> ReadySignalMock;
        protected Mos6502 Cpu { get; private set; }

        [SetUp]
        public void Initialize()
        {
            _config = Config;
            SetUpMocks();

            var memory = MemoryMock != null ? MemoryMock.Object : new MemoryNull();
            var ready = ReadySignalMock != null ? ReadySignalMock.Object : new ReadySignalNull();

            Cpu = new Mos6502(_config, memory, ready);
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _stopwatch.Stop();
            Console.WriteLine("Elapsed time: {0}ms", _stopwatch.ElapsedMilliseconds);
        }

        protected virtual Mos6502Configuration Config
        {
            get
            {
                return new Mos6502Configuration(0xFF, true);
            }
        }

        protected virtual void SetUpMocks()
        {
        }
    }
}
