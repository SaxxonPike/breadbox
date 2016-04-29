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
    public class CellBuilderTests
    {
        [Test]
        public void Build_UsesCorrectDataMask()
        {
            var cell = new CellBuilder()
                .WithDataBitCount(4)
                .Build();

            var read = cell.GetReadFunction();
            var write = cell.GetWriteFunction();

            write(0xFF);
            read().Should().Be(0x0F);
        }

        [Test]
        public void Build_UsesCorrectInitialValue()
        {
            var cell = new CellBuilder()
                .WithDataBitCount(6)
                .WithInitialValue(0xFF)
                .Build();

            var read = cell.GetReadFunction();
            read().Should().Be(0x3F);
        }
    }
}
