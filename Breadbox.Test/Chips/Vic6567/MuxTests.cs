using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
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
            var width = 504;
            var lines = 312;
            var package = new Vic2Package(width, lines, 0x14C);
            var address = Expression.Parameter(typeof(int));
            var data = Expression.Parameter(typeof(int));
            var rw = Expression.Parameter(typeof(bool));

            var peek = Expression.Lambda<Func<int, int>>(package.PeekRegister(address).Reduce(), address);
            var poke = Expression.Lambda<Action<int, int>>(package.PokeRegister(address, data).Reduce(), address, data);
            var clock = Expression.Lambda<Action<int, int, bool>>(package.Clock(address, data, rw).Reduce(), address, data, rw);
            var outAddr = Expression.Lambda<Func<int>>(package.Address);
            var outVideo = Expression.Lambda<Func<int>>(package.OutputVideo);

            var peekFunc = peek.Compile();
            var pokeFunc = poke.Compile();
            var addrFunc = outAddr.Compile();
            var clockFunc = clock.Compile(DebugInfoGenerator.CreatePdbGenerator());
            var videoFunc = outVideo.Compile();

            var sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i < width * lines * 50; i++)
            {
                clockFunc(0xFFFF, 0xFF, true);
                videoFunc();
            }
            sw.Stop();
            Console.WriteLine("{0}ms, {1}t", sw.ElapsedMilliseconds, sw.ElapsedTicks);
        }
    }
}
