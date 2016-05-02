using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Breadbox.Chips.Vic6567
{
    public static class Vic2VideoOutput
    {
        public static Expression Enabled(Expression hsync, Expression vsync)
        {
            return Expression.IsFalse(Expression.OrElse(hsync, vsync));
        }

        public static Expression OutputRGB(Expression outputColorValue)
        {
            return Expression.Switch(outputColorValue, Expression.Constant(Color.FromArgb(0x00, 0x00, 0x00).ToArgb()),
                Expression.SwitchCase(Expression.Constant(Color.FromArgb(0xFF, 0xFF, 0xFF).ToArgb()), Expression.Constant(0x1)),
                Expression.SwitchCase(Expression.Constant(Color.FromArgb(0x68, 0x37, 0x2B).ToArgb()), Expression.Constant(0x2)),
                Expression.SwitchCase(Expression.Constant(Color.FromArgb(0x70, 0xA4, 0xB2).ToArgb()), Expression.Constant(0x3)),
                Expression.SwitchCase(Expression.Constant(Color.FromArgb(0x6F, 0x3D, 0x86).ToArgb()), Expression.Constant(0x4)),
                Expression.SwitchCase(Expression.Constant(Color.FromArgb(0x58, 0x8D, 0x43).ToArgb()), Expression.Constant(0x5)),
                Expression.SwitchCase(Expression.Constant(Color.FromArgb(0x35, 0x28, 0x79).ToArgb()), Expression.Constant(0x6)),
                Expression.SwitchCase(Expression.Constant(Color.FromArgb(0xB8, 0xC7, 0x6F).ToArgb()), Expression.Constant(0x7)),
                Expression.SwitchCase(Expression.Constant(Color.FromArgb(0x6F, 0x4F, 0x25).ToArgb()), Expression.Constant(0x8)),
                Expression.SwitchCase(Expression.Constant(Color.FromArgb(0x43, 0x39, 0x00).ToArgb()), Expression.Constant(0x9)),
                Expression.SwitchCase(Expression.Constant(Color.FromArgb(0x9A, 0x67, 0x59).ToArgb()), Expression.Constant(0xA)),
                Expression.SwitchCase(Expression.Constant(Color.FromArgb(0x44, 0x44, 0x44).ToArgb()), Expression.Constant(0xB)),
                Expression.SwitchCase(Expression.Constant(Color.FromArgb(0x6C, 0x6C, 0x6C).ToArgb()), Expression.Constant(0xC)),
                Expression.SwitchCase(Expression.Constant(Color.FromArgb(0x9A, 0xD2, 0x84).ToArgb()), Expression.Constant(0xD)),
                Expression.SwitchCase(Expression.Constant(Color.FromArgb(0x6C, 0x5E, 0xB5).ToArgb()), Expression.Constant(0xE)),
                Expression.SwitchCase(Expression.Constant(Color.FromArgb(0x95, 0x95, 0x95).ToArgb()), Expression.Constant(0xF)));
        }
    }
}
