using BreadboxF;
using Moq;
using NUnit.Framework;

namespace Breadbox.Test.Vic2
{
    [TestFixture]
    public abstract class Vic2BaseTestFixture
    {
        private int[] memory;

        protected Mock<CommodoreVic2ClockInterface> ClockMock;
        protected Mock<CommodoreVic2MemoryInterface> MemoryMock;
        protected Mock<CommodoreVic2VideoInterface> VideoMock;
        protected CommodoreVic2Chip Vic { get; private set; }

        [SetUp]
        public void Initialize()
        {
            ClockMock = new Mock<CommodoreVic2ClockInterface>();
            MemoryMock = new Mock<CommodoreVic2MemoryInterface>();
            VideoMock = new Mock<CommodoreVic2VideoInterface>();
            SetUpMocks();
            Vic = new CommodoreVic2Chip(Config, MemoryMock.Object, VideoMock.Object, ClockMock.Object);
        }

        protected abstract CommodoreVic2Configuration Config { get; }

        protected virtual void SetUpMocks()
        {
            ClockMock.Setup(m => m.ClockPhi1());
            ClockMock.Setup(m => m.ClockPhi2());
            MemoryMock.Setup(m => m.Read(It.IsAny<int>())).Returns(0x00);
            MemoryMock.Setup(m => m.Write(It.IsAny<int>(), It.IsAny<int>()));
            VideoMock.Setup(m => m.Output(It.IsAny<CommodoreVic2VideoOutput>()));
        }

        protected void PatchPersistentMemory(int address, params int[] data)
        {
            if (memory == null) return;

            foreach (var d in data)
            {
                memory[address & 0x3FFF] = d;
                address++;
            }
        }

        protected void EnablePersistentMemory()
        {
            if (memory != null) return;

            memory = new int[0x3FFF];
            MemoryMock.Setup(m => m.Read(It.IsAny<int>()))
                .Returns<int>(a => memory[a & 0x3FFF]);
            MemoryMock.Setup(m => m.Write(It.IsAny<int>(), It.IsAny<int>()))
                .Callback<int, int>((a, d) => memory[a & 0x3FFF] = d & 0xFF);
        }
    }
}
