using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Breadbox.Packages.Vic2;

namespace Breadbox.Packages
{
    public class Mos6569 : Package
    {
        public Mos6569() : base(new Config(
            504,
            312,
            0x17C,
            0x1E0,
            0x12C,
            0x00F,
            0x14C,
            0x194,
            17734472,
            18
            ))
        {
        }
    }
}
