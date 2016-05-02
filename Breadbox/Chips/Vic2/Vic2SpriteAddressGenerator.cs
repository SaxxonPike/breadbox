using System.Linq.Expressions;

namespace Breadbox.Chips.Vic2
{
    public static class Vic2SpriteAddressGenerator
    {
        public static Expression P(Expression vm, int spriteNumber)
        {
            return Expression.Or(Expression.Constant(0x3F8 | spriteNumber),
                Expression.LeftShift(vm, Expression.Constant(10)));
        }

        public static Expression S(Expression p, Expression mc)
        {
            return Expression.Or(mc, Expression.LeftShift(p, Expression.Constant(6)));
        }
    }
}
