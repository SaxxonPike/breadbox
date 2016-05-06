using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Breadbox.Packages.Vic2;

namespace Breadbox.Packages
{
    public class Mos6567R8 : Package
    {
        public Mos6567R8() : base(new Config(
            520,
            263,
            0x18C,
            0x1F0,
            0x00D,
            0x028,
            0x15C,
            0x19C
            ))
        {
        }
    }
}
