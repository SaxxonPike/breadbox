using System.Linq.Expressions;

namespace Breadbox.Chips.Vic2
{
    /// <summary>
    /// Emulates behavior of MC and MCBASE.
    /// </summary>
    public static class Vic2MobCounter
    {
        public static Expression Increment(MemberExpression mc)
        {
            return Expression.Assign(mc, Expression.And(Expression.Increment(mc), Expression.Constant(0x3F)));
        }

        public static Expression ResetBase(MemberExpression mcBase)
        {
            return Expression.Assign(mcBase, Expression.Constant(0));
        }

        public static Expression StoreMcToBase(MemberExpression mc, MemberExpression mcBase, Expression isCrunched)
        {
            return Expression.Assign(mcBase, Expression.IfThenElse(isCrunched, mc, NewCrunchedBaseValue(mc, mcBase)));
        }

        private static Expression NewCrunchedBaseValue(Expression mc, Expression mcBase)
        {
            return Expression.Or(Expression.And(Expression.Constant(0x2A), Expression.And(mcBase, mc)),
                Expression.And(Expression.Constant(0x15), Expression.Or(mcBase, mc)));
        }

        public static Expression LoadMcFromBase(MemberExpression mc, MemberExpression mcBase)
        {
            return Expression.Assign(mc, mcBase);
        }
    }
}
