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
            var v2 = new Vic2Configuration(1, 2, 3, 4, 5, 6);
        }
    }
}
