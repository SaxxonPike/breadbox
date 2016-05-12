using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Breadbox.Packages.Vic2;

namespace Breadbox.Packages
{
    public class Mos6572 : Package
    {
        public Mos6572() : base(new Config(
            520,
            312,
            0x18C,
            0x1F0,
            0x12C,
            0x00F,
            0x15C,
            0x19C,
            14328224,
            14
            ))
        {
        }
    }
}
