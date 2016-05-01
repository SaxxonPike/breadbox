using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Extensions;

namespace Breadbox.Chips.Vic6567
{
    public class SyncGenerator
    {
        private int _hsync;
        private int _vsync;

        public Expression Hsync
        {
            get { return Util.Member(() => _hsync); }
        }

        public Expression Vsync
        {
            get { return Util.Member(() => _vsync); }
        }

        public Expression Clock(Expression x, Expression y, Expression hsyncSet, Expression hsyncClear,
            Expression vsyncSet, Expression vsyncClear)
        {
            return Expression.Block(
                Expression.IfThenElse(Expression.Equal(x, hsyncSet), Expression.Assign(Hsync, Expression.Constant(true)),
                    Expression.IfThen(Expression.Equal(x, hsyncClear),
                        Expression.Assign(Hsync, Expression.Constant(false)))),
                Expression.IfThenElse(Expression.Equal(y, vsyncSet), Expression.Assign(Vsync, Expression.Constant(true)),
                    Expression.IfThen(Expression.Equal(y, vsyncClear),
                        Expression.Assign(Vsync, Expression.Constant(false)))));
        }
    }
}
