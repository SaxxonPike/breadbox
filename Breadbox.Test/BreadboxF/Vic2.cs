using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BreadboxF;
using NUnit.Framework;

namespace Breadbox.Test.BreadboxF
{
    [TestFixture]
    public class Vic2
    {
        [Test]
        public void Test1()
        {
            var config = new Vic2Configuration(13, 65, 263);
        }
    }
}
