using System;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Compatibility;

namespace Breadbox.Test.Vic2
{
    [TestFixture]
    public abstract class Vic2BaseTestFixture
    {
        private CommodoreVic2Configuration _config;
        private Stopwatch _stopwatch;

        protected Mock<IClock> ClockMockPhi1;
        protected Mock<IClock> ClockMockPhi2;
        protected Mock<IMemory> MemoryMock;
        protected CommodoreVic2Chip Vic { get; private set; }

        [SetUp]
        public void Initialize()
        {
            _config = Config;
            SetUpMocks();

            var memory = MemoryMock != null ? MemoryMock.Object : new MemoryNull();
            var clockPhi1 = ClockMockPhi1 != null ? ClockMockPhi1.Object : new ClockNull();
            var clockPhi2 = ClockMockPhi2 != null ? ClockMockPhi2.Object : new ClockNull();

            Vic = new CommodoreVic2Chip(_config, memory, clockPhi1, clockPhi2);
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _stopwatch.Stop();
            Console.WriteLine("Elapsed time: {0}ms", _stopwatch.ElapsedMilliseconds);
        }

        protected abstract CommodoreVic2Configuration Config { get; }

        protected virtual void SetUpMocks()
        {
        }

        protected void SetBorderColor(int color)
        {
            Vic.PokeRegister(0x20, color);
        }

        protected void SetBackgroundColor(int index, int color)
        {
            Vic.PokeRegister(0x21 + (index & 0x3), color);
        }

        protected void SetDisplayEnable(bool value)
        {
            Vic.PokeRegister(0x11, (Vic.PeekRegister(0x11) & 0xEF) | (value ? 0x10 : 0x00));
        }

        protected void SetRowSelect(bool value)
        {
            Vic.PokeRegister(0x11, (Vic.PeekRegister(0x11) & 0xF7) | (value ? 0x08 : 0x00));
        }

        protected void SetColumnSelect(bool value)
        {
            Vic.PokeRegister(0x16, (Vic.PeekRegister(0x16) & 0xF7) | (value ? 0x08 : 0x00));
        }

        protected void SetVideoMemoryPointer(int value)
        {
            Vic.PokeRegister(0x18, (Vic.PeekRegister(0x18) & 0x0E) | ((value & 0xF) << 4));
        }

        protected void SetCharacterBankPointer(int value)
        {
            Vic.PokeRegister(0x18, (Vic.PeekRegister(0x18) & 0xF0) | ((value & 0x7) << 1));
        }

        protected void SetXScroll(int value)
        {
            Vic.PokeRegister(0x16, (Vic.PeekRegister(0x16) & 0x38) | (value & 0x7));
        }

        protected void SetYScroll(int value)
        {
            Vic.PokeRegister(0x11, (Vic.PeekRegister(0x11) & 0xF8) | (value & 0x7));
        }

        protected void SetExtraColorMode(bool value)
        {
            Vic.PokeRegister(0x11, (Vic.PeekRegister(0x11) & 0xBF) | (value ? 0x40 : 0x00));
        }

        protected void SetBitmapMode(bool value)
        {
            Vic.PokeRegister(0x11, (Vic.PeekRegister(0x11) & 0xDF) | (value ? 0x20 : 0x00));
        }

        protected void SetMultiColorMode(bool value)
        {
            Vic.PokeRegister(0x16, (Vic.PeekRegister(0x16) & 0xEF) | (value ? 0x10 : 0x00));
        }
    }
}
