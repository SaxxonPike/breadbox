using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Chips.Vic6567;
using NUnit.Framework;
using Stopwatch = NUnit.Framework.Compatibility.Stopwatch;

namespace Breadbox.Test.Chips.Vic6567
{
    [TestFixture]
    public class MuxTests
    {
        [Test]
        public void Test1()
        {
            var registers = new Registers();
            var border = new BorderUnit();
            var mux = new Mux();
            var graphicsDataSequencer = new GraphicsDataSequencer();
            var videoMemory = new VideoMatrixMemory();
            var videoCounter = new VideoMatrixCounter();

            var spriteDataSequencers = new[]
            {
                new MobDataSequencer(),
                new MobDataSequencer(),
                new MobDataSequencer(),
                new MobDataSequencer(),
                new MobDataSequencer(),
                new MobDataSequencer(),
                new MobDataSequencer(),
                new MobDataSequencer()
            };

            var spriteDatas = new Expression[]
            {
                spriteDataSequencers[0].OutputData(registers.M0MC),
                spriteDataSequencers[1].OutputData(registers.M1MC),
                spriteDataSequencers[2].OutputData(registers.M2MC),
                spriteDataSequencers[3].OutputData(registers.M3MC),
                spriteDataSequencers[4].OutputData(registers.M4MC),
                spriteDataSequencers[5].OutputData(registers.M5MC),
                spriteDataSequencers[6].OutputData(registers.M6MC),
                spriteDataSequencers[7].OutputData(registers.M7MC)
            };

            var spriteColors = new Expression[]
            {
                spriteDataSequencers[0].OutputColor(registers.M0MC, registers.M0C, registers.MM0, registers.MM1),
                spriteDataSequencers[1].OutputColor(registers.M1MC, registers.M1C, registers.MM0, registers.MM1),
                spriteDataSequencers[2].OutputColor(registers.M2MC, registers.M2C, registers.MM0, registers.MM1),
                spriteDataSequencers[3].OutputColor(registers.M3MC, registers.M3C, registers.MM0, registers.MM1),
                spriteDataSequencers[4].OutputColor(registers.M4MC, registers.M4C, registers.MM0, registers.MM1),
                spriteDataSequencers[5].OutputColor(registers.M5MC, registers.M5C, registers.MM0, registers.MM1),
                spriteDataSequencers[6].OutputColor(registers.M6MC, registers.M6C, registers.MM0, registers.MM1),
                spriteDataSequencers[7].OutputColor(registers.M7MC, registers.M7C, registers.MM0, registers.MM1)
            };

            var spritePriorities = new Expression[]
            {
                registers.M0DP,
                registers.M1DP,
                registers.M2DP,
                registers.M3DP,
                registers.M4DP,
                registers.M5DP,
                registers.M6DP,
                registers.M7DP
            };

            var gColor = Expression.Parameter(typeof(int), "GColor");

            var graphicsDataOutput = graphicsDataSequencer.OutputData(registers.BMM, registers.MCM, gColor);
            var graphicsColorOutput = graphicsDataSequencer.OutputColor(registers.B0C, registers.B1C, registers.B2C, registers.B3C, gColor, registers.BMM, registers.ECM, registers.MCM);

            var muxOutput = mux.OutputColor(spriteDatas, spriteColors, spritePriorities, graphicsDataOutput, graphicsColorOutput);
            var borderOutput = border.MuxBorderUnit(muxOutput, registers.EC);

            var f = Expression.Lambda<Func<int, int>>(borderOutput, gColor).Compile();
            f(0);

            var timer = new Stopwatch();
            timer.Start();
            for (var i = 0; i < 8000000; i++)
            {
                f(0);
            }
            timer.Stop();
            Console.WriteLine(timer.ElapsedMilliseconds);
        }
    }
}
