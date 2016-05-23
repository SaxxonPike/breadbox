using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BreadboxF;
using NUnit.Framework;

namespace Breadbox.Test.Vic2.MemoryAccess
{
    [TestFixture]
    public class Vic2MemoryAccessNewNtscTests : Vic2MemoryAccessBaseTestFixture
    {
        protected override CommodoreVic2Configuration Config
        {
            get { return new CommodoreVic2ConfigurationFactory().CreateNewNtscConfiguration(); }
        }
    }
}
