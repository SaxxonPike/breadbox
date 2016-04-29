using System;
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
    public class RasterCounter
    {
        private int XValue { get; set; }
        private int YValue { get; set; }

        public Expression X
        {
            get { return Util.Member(() => XValue); }
        }

        public Expression Y
        {
            get { return Util.Member(() => YValue); }
        }

        public Expression Increment(Expression width, Expression lines)
        {
            return Expression.Block(
                Expression.PreIncrementAssign(X),
                Expression.IfThen(Expression.Equal(X, width), Expression.Block(
                    Expression.Assign(X, Expression.Constant(0)),
                    Expression.PreIncrementAssign(Y),
                    Expression.IfThen(Expression.Equal(Y, lines), Expression.Assign(Y, Expression.Constant(0))))));
        }
    }
}
