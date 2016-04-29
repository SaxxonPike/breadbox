using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Extensions;

namespace Breadbox.Chips.Vic6567
{
    /// <summary>
    /// Emulates behavior of MC and MCBASE.
    /// </summary>
    public class MobCounter
    {
        private int _mc;
        private int _mcBase;

        public MemberExpression Mc
        {
            get { return Util.Member(() => _mc); }
        }

        private MemberExpression McBase
        {
            get { return Util.Member(() => _mcBase); }
        }

        public Expression Increment
        {
            get { return Expression.Assign(Mc, Expression.And(Expression.Increment(Mc), Expression.Constant(0x3F))); }
        }

        public Expression ResetBase
        {
            get { return Expression.Assign(McBase, Expression.Constant(0)); }
        }

        public Expression StoreValueToBase(Expression isCrunched)
        {
            return Expression.Assign(McBase, Expression.IfThenElse(isCrunched, Mc, NewCrunchedBaseValue));
        }

        private Expression NewCrunchedBaseValue
        {
            get
            {
                return Expression.Or(Expression.And(Expression.Constant(0x2A), Expression.And(McBase, Mc)),
                    Expression.And(Expression.Constant(0x15), Expression.Or(McBase, Mc)));
            }
        }

        public Expression LoadValueFromBase
        {
            get { return Expression.Assign(Mc, McBase); }
        }
    }
}
