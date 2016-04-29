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
        private int Value { get; set; }

        public Expression Reset
        {
            get { return Expression.Assign(Util.Member(() => Value), Expression.Constant(0)); }
        }

        public Expression Increment
        {
            get { return Expression.Assign(Util.Member(() => Value), Expression.And(Expression.Increment(Util.Member(() => Value)), Expression.Constant(0x7))); }
        }
    }
}
