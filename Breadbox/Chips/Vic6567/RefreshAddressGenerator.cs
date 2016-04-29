using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Extensions;

namespace Breadbox.Chips.Vic6567
{
    public class RefreshAddressGenerator
    {
        /*
         Refresh addresses

         +----+----+----+----+----+----+----+----+----+----+----+----+----+----+
         | 13 | 12 | 11 | 10 |  9 |  8 |  7 |  6 |  5 |  4 |  3 |  2 |  1 |  0 |
         +----+----+----+----+----+----+----+----+----+----+----+----+----+----+
         |  1 |  1 |  1 |  1 |  1 |  1 |REF7|REF6|REF5|REF4|REF3|REF2|REF1|REF0|
         +----+----+----+----+----+----+----+----+----+----+----+----+----+----+        
        */

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private int Value { get; set; }

        public Expression Generate
        {
            get { return Expression.Or(Expression.Constant(0x3F00), Util.Member(() => Value)); }
        }

        public Expression Decrement
        {
            get { return Expression.Assign(Util.Member(() => Value), Expression.And(Expression.Decrement(Util.Member(() => Value)), Expression.Constant(0xFF))); }
        }
    }
}
