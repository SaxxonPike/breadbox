using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Extensions;

namespace Breadbox.Chips.Vic6567
{
    /// <summary>
    /// Emulates behavior of VMLI, VC and VCBASE.
    /// </summary>
    public class VideoMatrixCounter
    {
        private int _vc;
        private int _vcBase;
        private int _vmli;

        public Expression Vc
        {
            get { return Util.Member(() => _vc); }
        }

        private Expression VcBase
        {
            get { return Util.Member(() => _vcBase); }
        }

        public Expression Vmli
        {
            get { return Util.Member(() => _vmli); }
        }

        public Expression Increment
        {
            get
            {
                return Expression.Block(
                    Expression.Assign(Vc, Expression.And(Expression.Increment(Vc), Expression.Constant(0x3FF))),
                    Expression.Assign(Vmli, Expression.And(Expression.Increment(Vmli), Expression.Constant(0x3F))));
            }
        }

        public Expression ResetBase
        {
            get { return Expression.Assign(VcBase, Expression.Constant(0)); }
        }

        public Expression StoreValueToBase
        {
            get { return Expression.Assign(VcBase, Vc); }
        }

        public Expression LoadValueFromBase
        {
            get {
                return Expression.Block(
                    Expression.Assign(Vc, VcBase),
                    Expression.Assign(Vmli, Expression.Constant(0)));
            }
        }
    }
}
