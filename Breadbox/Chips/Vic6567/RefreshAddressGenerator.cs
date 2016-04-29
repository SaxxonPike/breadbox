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
        private int _ref { get; set; }

        private Expression Ref
        {
            get { return Util.Member(() => _ref); }
        }

        public Expression Generate
        {
            get { return Expression.Or(Expression.Constant(0x3F00), Ref); }
        }

        public Expression Decrement
        {
            get { return Expression.Assign(Ref, Expression.And(Expression.Decrement(Ref), Expression.Constant(0xFF))); }
        }
    }
}
