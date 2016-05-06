using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Breadbox.Packages.Vic2;

namespace Breadbox.Packages
{
    public class Mos6567R56A : Package
    {
        public Mos6567R56A() : base(new Config(
            512,
            262,
            0x184,
            0x1F0,
            0x00D,
            0x028,
            0x154,
            0x19C
            ))
        {
        }
    }
}
