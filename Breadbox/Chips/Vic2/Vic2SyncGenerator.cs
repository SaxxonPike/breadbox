﻿using System.Linq.Expressions;
using Breadbox.Extensions;

namespace Breadbox.Chips.Vic2
{
    public static class Vic2SyncGenerator
    {
        public static Expression Clock(MemberExpression hsync, MemberExpression vsync, Expression x, Expression y, Expression hsyncSetX,
            Expression hsyncClearX, Expression vsyncSetY, Expression vsyncClearY)
        {
            return Expression.Block(
                Expression.IfThenElse(Expression.Equal(x, hsyncSetX), Util.Set(hsync),
                    Expression.IfThen(Expression.Equal(x, hsyncClearX),
                        Util.Reset(hsync))),
                Expression.IfThenElse(Expression.Equal(y, vsyncSetY), Util.Set(vsync),
                    Expression.IfThen(Expression.Equal(y, vsyncClearY),
                        Util.Reset(vsync))));
        }
    }
}