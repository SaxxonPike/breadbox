using System;
using NUnit.Framework;

namespace Breadbox.Test.Vic2
{
    [TestFixture]
    public abstract class Vic2RegionAgnosticBaseTestFixture : Vic2BaseTestFixture
    {
        protected override CommodoreVic2Configuration Config
        {
            get { return GetRandomConfig(); }
        }

        private static CommodoreVic2Configuration GetRandomConfig()
        {
            var rng = new Random();
            var options = new[]
            {
                new {
                    Name = "New NTSC",
                    Config = new CommodoreVic2ConfigurationFactory().CreateNewNtscConfiguration()
                },
                new {
                    Name = "Old NTSC",
                    Config = new CommodoreVic2ConfigurationFactory().CreateOldNtscConfiguration()
                },
                new {
                    Name = "PAL-B",
                    Config = new CommodoreVic2ConfigurationFactory().CreatePalBConfiguration()
                },
                new {
                    Name = "PAL-N",
                    Config = new CommodoreVic2ConfigurationFactory().CreatePalNConfiguration()
                },
                new {
                    Name = "PAL-M",
                    Config = new CommodoreVic2ConfigurationFactory().CreatePalMConfiguration()
                },
            };
            var choice = options[rng.Next(options.Length)];
            Console.WriteLine("Randomly chose config: {0}", choice.Name);
            return choice.Config;
        }


    }
}
