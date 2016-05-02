using System.Linq.Expressions;

namespace Breadbox.Chips.Vic2
{
    /// <summary>
    /// Emulates the behavior of RC.
    /// </summary>
    public static class Vic2RowCounter
    {
        public static Expression Reset(MemberExpression rc)
        {
            return Expression.Assign(rc, Expression.Constant(0));
        }

        public static Expression Increment(MemberExpression rc)
        {
            return Expression.Assign(rc, Expression.And(Expression.Increment(rc), Expression.Constant(0x7)));
        }
    }
}
