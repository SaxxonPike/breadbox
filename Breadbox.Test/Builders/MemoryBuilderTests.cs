using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Breadbox.Builders;
using Breadbox.Extensions;
using FluentAssertions;
using NUnit.Framework;

namespace Breadbox.Test.Builders
{
    [TestFixture]
    public class MemoryBuilderTests
    {
        [Test]
        public void Build_UsesCorrectAddressAndDataMask()
        {
            var memory = new MemoryBuilder()
                .WithIndexBitCount(12)
                .WithDataBitCount(4)
                .Build();

            var read = memory.GetReadFunction();
            var write = memory.GetWriteFunction();

            write(0x0000, 0xFF);
            write(0x1000, 0xF8);
            write(0x1800, 0xF4);
            read(0x0000).Should().Be(0x08);
            read(0x0800).Should().Be(0x04);
        }

        [Test]
        public void Build_UsesCorrectInitialValues()
        {
            var initialValues = Any.Ints(16);
            var memory = new MemoryBuilder()
                .WithIndexBitCount(4)
                .WithDataBitCount(4)
                .WithInitialData(initialValues)
                .Build();

            var read = memory.GetReadFunction();
            var allValues = Enumerable.Range(0, initialValues.Length).Select(read).ToArray();
            var expectedInitialValues = initialValues.Select(i => i & 0x0F).ToArray();
            allValues.ShouldAllBeEquivalentTo(expectedInitialValues);
        }
    }
}
