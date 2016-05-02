using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Chips.Vic2;
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
            //var registers = new Vic2State();

            //var spriteDatas = new[]
            //{
            //    Vic2MobDataSequencer.OutputData(registers.MobBuffer0, registers.M0MC),
            //    Vic2MobDataSequencer.OutputData(registers.MobBuffer1, registers.M1MC),
            //    Vic2MobDataSequencer.OutputData(registers.MobBuffer2, registers.M2MC),
            //    Vic2MobDataSequencer.OutputData(registers.MobBuffer3, registers.M3MC),
            //    Vic2MobDataSequencer.OutputData(registers.MobBuffer4, registers.M4MC),
            //    Vic2MobDataSequencer.OutputData(registers.MobBuffer5, registers.M5MC),
            //    Vic2MobDataSequencer.OutputData(registers.MobBuffer6, registers.M6MC),
            //    Vic2MobDataSequencer.OutputData(registers.MobBuffer7, registers.M7MC),
            //};

            //var spriteColors = new[]
            //{
            //    Vic2MobDataSequencer.OutputColor(registers.MobBuffer0, registers.M0MC, registers.M0C, registers.MM0, registers.MM1),
            //    Vic2MobDataSequencer.OutputColor(registers.MobBuffer1, registers.M1MC, registers.M1C, registers.MM0, registers.MM1),
            //    Vic2MobDataSequencer.OutputColor(registers.MobBuffer2, registers.M2MC, registers.M2C, registers.MM0, registers.MM1),
            //    Vic2MobDataSequencer.OutputColor(registers.MobBuffer3, registers.M3MC, registers.M3C, registers.MM0, registers.MM1),
            //    Vic2MobDataSequencer.OutputColor(registers.MobBuffer4, registers.M4MC, registers.M4C, registers.MM0, registers.MM1),
            //    Vic2MobDataSequencer.OutputColor(registers.MobBuffer5, registers.M5MC, registers.M5C, registers.MM0, registers.MM1),
            //    Vic2MobDataSequencer.OutputColor(registers.MobBuffer6, registers.M6MC, registers.M6C, registers.MM0, registers.MM1),
            //    Vic2MobDataSequencer.OutputColor(registers.MobBuffer7, registers.M7MC, registers.M7C, registers.MM0, registers.MM1)
            //};

            //var spritePriorities = new Expression[]
            //{
            //    registers.M0DP,
            //    registers.M1DP,
            //    registers.M2DP,
            //    registers.M3DP,
            //    registers.M4DP,
            //    registers.M5DP,
            //    registers.M6DP,
            //    registers.M7DP
            //};

            //var gColor = Expression.Parameter(typeof(int), "GColor");

            //var graphicsDataOutput = Vic2GraphicsDataSequencer.OutputData(registers.GraphicsBuffer, registers.BMM, registers.MCM, gColor);
            //var graphicsColorOutput = Vic2GraphicsDataSequencer.OutputColor(registers.GraphicsBuffer, registers.B0C, registers.B1C, registers.B2C, registers.B3C, gColor, registers.BMM, registers.ECM, registers.MCM);

            //var muxOutput = Vic2Mux.OutputColor(spriteDatas, spriteColors, spritePriorities, graphicsDataOutput, graphicsColorOutput);
            //var borderOutput = Vic2BorderUnit.OutputColor(registers.MainBorderFlipFlop, registers.VerticalBorderFlipFlop, muxOutput, registers.EC);
            //var videoOutput = Vic2VideoOutput.OutputRGB(borderOutput);

            //var f = Expression.Lambda<Func<int, int>>(borderOutput, gColor).Compile();
            //f(0);

            //var timer = new Stopwatch();
            //timer.Start();
            //for (var i = 0; i < 8000000; i++)
            //{
            //    f(0);
            //}
            //timer.Stop();
            //Console.WriteLine(timer.ElapsedMilliseconds);

            var package = new Vic2Package();
            var address = Expression.Parameter(typeof(int));
            var data = Expression.Parameter(typeof(int));

            var peek = Expression.Lambda<Func<int, int>>(package.PeekRegister(address), address);
            var poke = Expression.Lambda<Action<int, int>>(package.PokeRegister(address, data), address, data);

            var peekFunc = peek.Compile();
            var pokeFunc = poke.Compile();

            for (var i = 0; i < 0x80; i++)
            {
                Console.WriteLine("{0:X2} = {1:X2}", i, peekFunc(i));
            }
        }
    }
}
