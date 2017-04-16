using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Moq.Language;
using NUnit.Framework;

namespace Breadbox
{
    [Parallelizable(ParallelScope.Fixtures)]
    public abstract class BreadboxBaseTestFixture : BaseTestFixture
    {
        private Lazy<Mos6502> _cpu;
        private Lazy<Mos6502Configuration> _cpuConfig;
        private Lazy<Mos6567Configuration> _gpuConfig;
        private Lazy<Mock<ISystem>> _system;
        private Lazy<Mos6567> _gpu;

        [SetUp]
        public void Initialize()
        {
            _system = new Lazy<Mock<ISystem>>(() =>
            {
                var system = Mock<ISystem>();
                system.Setup(x => x.Ready).Returns(true);
                system.Setup(x => x.Nmi).Returns(false);
                system.Setup(x => x.Irq).Returns(false);
                system.Setup(x => x.Read(It.IsAny<int>())).Returns(0xFF);
                system.Setup(x => x.Write(It.IsAny<int>(), It.IsAny<int>()));
                return system;
            });

            _cpuConfig = new Lazy<Mos6502Configuration>(() =>
            {
                return new Mos6502Configuration(0xFF, true, System.Object.Read, System.Object.Write,
                    () => System.Object.Ready, () => System.Object.Irq, () => System.Object.Nmi);
            });

            _gpuConfig = new Lazy<Mos6567Configuration>(() => new Mos6567Configuration(System.Object.Read, 263, 65, 0x00D, 0x029, 0x19C, 0x00D));

            _cpu = new Lazy<Mos6502>(() => new Mos6502(CpuConfig));

            _gpu = new Lazy<Mos6567>(() => new Mos6567(GpuConfig));
        }

        protected Mos6502 Cpu => _cpu.Value;

        protected virtual Mos6502Configuration CpuConfig => _cpuConfig.Value;

        protected Mos6567 Gpu => _gpu.Value;

        protected virtual Mos6567Configuration GpuConfig => _gpuConfig.Value;

        protected Mock<ISystem> System => _system.Value;

        protected IEnumerable<int> GetColdStartReadSequence(int address, params int[] sequence)
        {
            var coldStartSequence = new[] { 0x00, 0x00, 0x00, address & 0xFF, (address >> 8) & 0xFF };
            return coldStartSequence.Concat(sequence);
        }

        protected ISetupSequentialResult<int> MockColdStartReadSequence(int address, params int[] sequence)
        {
            var reads = GetColdStartReadSequence(address, sequence);
            return reads.Aggregate(System.SetupSequence(m => m.Read(It.IsAny<int>())),
                (seq, item) => seq.Returns(item));
        }
    }
}

