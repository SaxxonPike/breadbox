using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Extensions;

namespace Breadbox.Chips.Vic6567
{
    /// <summary>
    /// Emulates "40x12 bit video matrix-color line".
    /// </summary>
    public class VideoMatrixMemory
    {
        private int[] Value { get; set; }

        public VideoMatrixMemory()
        {
            Value = new int[40];
        }

        public Expression Read(Expression index)
        {
            return Expression.ArrayAccess(Util.Member(() => Value), index);
        }

        public Expression Write(Expression index, Expression value)
        {
            return Expression.Assign(Expression.ArrayAccess(Util.Member(() => Value), index), Expression.And(value, Expression.Constant(0xFFF)));
        }
    }
}
