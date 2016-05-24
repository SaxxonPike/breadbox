using System;
using BreadboxF;
using NUnit.Framework;

namespace Breadbox.Test.Vic2
{
    public class Vic2IrqTests : Vic2BaseTestFixture
    {
        protected override CommodoreVic2Configuration Config
        {
            get { return new CommodoreVic2ConfigurationFactory().CreateNewNtscConfiguration(); }
        }

        [Test]
        public void RasterIrq()
        {
            // todo
        }
    }
}
