using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Breadbox.Chips.Vic6567
{
    public class GraphicsAddressGenerator
    {
        public Expression C(Expression vc, Expression vm)
        {
            return Expression.Or(vc, Expression.LeftShift(vm, Expression.Constant(10)));
        }

        public Expression TextG(Expression cb, Expression data, Expression rc, Expression isEcm)
        {
            var shiftedData = Expression.LeftShift(Expression.And(data, Expression.Constant(0xFF)), Expression.Constant(3));
            var shiftedCb = Expression.LeftShift(cb, Expression.Constant(11));
            var result = Expression.Or(rc, Expression.Or(shiftedData, shiftedCb));
            return Expression.IfThenElse(isEcm, Expression.And(result, Expression.Constant(0x39FF)), result);
        }

        public Expression BitmapG(Expression cb, Expression vc, Expression rc, Expression isEcm)
        {
            var shiftedCb = Expression.LeftShift(Expression.And(cb, Expression.Constant(0x4)), Expression.Constant(11));
            var shiftedVc = Expression.LeftShift(vc, Expression.Constant(3));
            var result = Expression.Or(rc, Expression.Or(shiftedVc, shiftedCb));
            return Expression.IfThenElse(isEcm, Expression.And(result, Expression.Constant(0x39FF)), result);
        }

        public Expression IdleG(Expression isEcm)
        {
            return Expression.IfThenElse(isEcm, Expression.Constant(0x39FF), Expression.Constant(0x3FFF));
        }
    }
}
