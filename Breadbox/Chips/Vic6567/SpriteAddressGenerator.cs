﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Breadbox.Chips.Vic6567
{
    public class SpriteAddressGenerator
    {
        public Expression P(Expression vm, int spriteNumber)
        {
            return Expression.Or(Expression.Constant(0x3F8 | spriteNumber),
                Expression.LeftShift(vm, Expression.Constant(10)));
        }

        public Expression S(Expression p, Expression mc)
        {
            return Expression.Or(mc, Expression.LeftShift(p, Expression.Constant(6)));
        }
    }
}
