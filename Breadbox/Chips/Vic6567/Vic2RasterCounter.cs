﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Extensions;

namespace Breadbox.Chips.Vic6567
{
    /// <summary>
    /// Emulates behavior of X/Y raster counters.
    /// </summary>
    public static class Vic2RasterCounter
    {
        public static Expression Clock(MemberExpression rasterX, MemberExpression rasterY, Expression width, Expression lines)
        {
            return Expression.Block(
                Expression.PreIncrementAssign(rasterX),
                Expression.IfThen(Expression.Equal(rasterX, width), Expression.Block(
                    Expression.Assign(rasterX, Expression.Constant(0)),
                    Expression.PreIncrementAssign(rasterY),
                    Expression.IfThen(Expression.Equal(rasterY, lines), Expression.Assign(rasterY, Expression.Constant(0))))));
        }
    }
}
