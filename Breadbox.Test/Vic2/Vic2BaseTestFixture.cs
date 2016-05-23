using System;
using BreadboxF;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Breadbox.Test.Vic2
{
    [TestFixture]
    public abstract class Vic2BaseTestFixture
    {
        private CommodoreVic2Configuration _config;
        private int[] _frameBuffer;
        private int _frameBufferIndex;
        private int[] _memory;

        protected Mock<CommodoreVic2ClockInterface> ClockMock;
        protected Mock<MemoryInterface> MemoryMock;
        protected Mock<CommodoreVic2VideoInterface> VideoMock;
        protected CommodoreVic2Chip Vic { get; private set; }

        [SetUp]
        public void Initialize()
        {
            _config = Config;

            ClockMock = new Mock<CommodoreVic2ClockInterface>();
            MemoryMock = new Mock<MemoryInterface>();
            VideoMock = new Mock<CommodoreVic2VideoInterface>();
            SetUpMocks();
            Vic = new CommodoreVic2Chip(_config, MemoryMock.Object, VideoMock.Object, ClockMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            PixelsOutputToFrameBuffer = 0;
            _frameBuffer = null;
            _frameBufferIndex = 0;
            _memory = null;
            LastAccessedAddress = 0;
        }

        protected abstract CommodoreVic2Configuration Config { get; }

        protected virtual void SetUpMocks()
        {
            ClockMock.Setup(m => m.ClockPhi1());
            ClockMock.Setup(m => m.ClockPhi2());
            MemoryMock.Setup(m => m.Read(It.IsAny<int>())).Returns(0x00)
                .Callback<int>(a => LastAccessedAddress = a);
            MemoryMock.Setup(m => m.Write(It.IsAny<int>(), It.IsAny<int>()))
                .Callback<int, int>((a, d) => LastAccessedAddress = a);
            VideoMock.Setup(m => m.Output(It.IsAny<CommodoreVic2VideoOutput>()));
        }

        protected void PatchPersistentMemory(int address, params int[] data)
        {
            if (_memory == null) return;

            foreach (var d in data)
            {
                _memory[address & 0x3FFF] = d;
                address++;
            }
        }

        protected void EnablePersistentMemory()
        {
            if (_memory != null) return;

            _memory = new int[0x4000];
            MemoryMock.Setup(m => m.Read(It.IsAny<int>()))
                .Returns<int>(a =>
                {
                    LastAccessedAddress = a;
                    return _memory[a & 0x3FFF];
                });
            MemoryMock.Setup(m => m.Write(It.IsAny<int>(), It.IsAny<int>()))
                .Callback<int, int>((a, d) =>
                {
                    LastAccessedAddress = a;
                    _memory[a & 0x3FFF] = d & 0xFF;
                });
        }

        protected void EnableFrameBuffer()
        {
            if (_frameBuffer != null) return;

            PixelsOutputToFrameBuffer = 0;
            var width = _config.VisiblePixelsPerRasterLine;
            var height = _config.VisibleRasterLines;
            var total = height*width;
            _frameBuffer = new int[total];

            VideoMock.Setup(m => m.Output(It.IsAny<CommodoreVic2VideoOutput>()))
                .Callback<CommodoreVic2VideoOutput>(output =>
                {
                    if (!output.VBlank && !output.HBlank)
                    {
                        _frameBuffer[_frameBufferIndex++] = output.Pixel;
                        PixelsOutputToFrameBuffer++;
                        if (_frameBufferIndex > total)
                        {
                            _frameBufferIndex = 0;
                        }
                    }
                });
        }

        protected int[] GetFrameBuffer()
        {
            var result = new int[_frameBuffer.Length];
            Array.Copy(_frameBuffer, result, result.Length);
            return result;
        }

        protected int PixelsOutputToFrameBuffer { get; private set; }

        protected int FrameBufferAt(int x, int y)
        {
            return _frameBuffer[x + (y * _config.RasterWidth)];
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

        protected int LastAccessedAddress { get; private set; }
    }
}
