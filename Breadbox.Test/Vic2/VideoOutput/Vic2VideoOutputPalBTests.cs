using BreadboxF;
using NUnit.Framework;

namespace Breadbox.Test.Vic2.VideoOutput
{
    [TestFixture]
    public class Vic2VideoOutputPalBTests : Vic2VideoOutputBaseTestFixture
    {
        protected override CommodoreVic2Configuration Config
        {
            get { return new CommodoreVic2ConfigurationFactory().CreatePalBConfiguration(); }
        }
    }
}
