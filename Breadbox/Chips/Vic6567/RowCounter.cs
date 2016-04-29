using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Extensions;

namespace Breadbox.Chips.Vic6567
{
    /// <summary>
    /// Emulates the behavior of RC.
    /// </summary>
    public class RowCounter
    {
        private int _rc;

        private Expression Rc
        {
            get { return Util.Member(() => _rc); }
        }

        public Expression Reset
        {
            get { return Expression.Assign(Rc, Expression.Constant(0)); }
        }

        public Expression Increment
        {
            get { return Expression.Assign(Rc, Expression.And(Expression.Increment(Rc), Expression.Constant(0x7))); }
        }
    }
}
