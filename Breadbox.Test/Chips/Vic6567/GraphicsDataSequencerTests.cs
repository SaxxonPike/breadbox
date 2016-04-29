using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Chips.Vic6567;
using NUnit.Framework;

namespace Breadbox.Test.Chips.Vic6567
{
    [TestFixture]
    public class GraphicsDataSequencerTests
    {
        private readonly ParameterExpression _b0C = Expression.Parameter(typeof(int), "B0C");
        private readonly ParameterExpression _b1C = Expression.Parameter(typeof(int), "B1C");
        private readonly ParameterExpression _b2C = Expression.Parameter(typeof(int), "B2C");
        private readonly ParameterExpression _b3C = Expression.Parameter(typeof(int), "B3C");
        private readonly ParameterExpression _color = Expression.Parameter(typeof(int), "C");
        private readonly ParameterExpression _bmm = Expression.Parameter(typeof(bool), "BMM");
        private readonly ParameterExpression _ecm = Expression.Parameter(typeof(bool), "ECM");
        private readonly ParameterExpression _mcm = Expression.Parameter(typeof(bool), "MCM");

        [Test]
        [Theory]
        public void Test(
            [Random(0x0, 0xF, 1)] int b0c,
            [Random(0x0, 0xF, 1)] int b1c,
            [Random(0x0, 0xF, 1)] int b2c,
            [Random(0x0, 0xF, 1)] int b3c,
            [Random(0x0, 0xF, 1)] int color,
            bool bmm,
            bool ecm,
            bool mcm)
        {
            var gds = new GraphicsDataSequencer();

            var output = gds.OutputColor(_b0C, _b1C, _b2C, _b3C, _color, _bmm, _ecm, _mcm);
            var f = Expression.Lambda<Func<int, int, int, int, int, bool, bool, bool, int>>(output, _b0C, _b1C, _b2C, _b3C, _color, _bmm, _ecm, _mcm).Compile();
            var result = f(b0c, b1c, b2c, b3c, color, bmm, ecm, mcm);
        }
    }
}
