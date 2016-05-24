using BreadboxF;
using NUnit.Framework;

namespace Breadbox.Test.Vic2.MemoryAccess
{
    [TestFixture]
    public class Vic2MemoryAccessPalNTests : Vic2MemoryAccessBaseTestFixture
    {
        protected override CommodoreVic2Configuration Config
        {
            get { return new CommodoreVic2ConfigurationFactory().CreatePalNConfiguration(); }
        }
    }
}
