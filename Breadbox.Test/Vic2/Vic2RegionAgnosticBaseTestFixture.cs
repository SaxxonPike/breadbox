using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BreadboxF;
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
                new CommodoreVic2ConfigurationFactory().CreateNewNtscConfiguration(),
                new CommodoreVic2ConfigurationFactory().CreateOldNtscConfiguration(),
                new CommodoreVic2ConfigurationFactory().CreatePalBConfiguration(),
                new CommodoreVic2ConfigurationFactory().CreatePalNConfiguration(),
                new CommodoreVic2ConfigurationFactory().CreatePalMConfiguration()
            };
            return options[rng.Next(options.Length)];
        }


    }
}
