using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BreadboxF;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Compatibility;

namespace Breadbox.Test.BreadboxF
{
    [TestFixture]
    public class Vic2
    {
        private Mock<CommodoreVic2ClockInterface> _clockMock;
        private Mock<CommodoreVic2MemoryInterface> _memoryMock;
        private Mock<CommodoreVic2VideoInterface> _videoMock;
        private CommodoreVic2Chip _vic;

        [SetUp]
        public void Initialize()
        {
            var config = new CommodoreVic2Configuration(13, 65, 263);
            _clockMock = new Mock<CommodoreVic2ClockInterface>();
            _memoryMock = new Mock<CommodoreVic2MemoryInterface>();
            _videoMock = new Mock<CommodoreVic2VideoInterface>();
            _vic = new CommodoreVic2Chip(config, _memoryMock.Object, _videoMock.Object, _clockMock.Object);
        }

        [Test]
        public void Test1()
        {
        }
    }
}
