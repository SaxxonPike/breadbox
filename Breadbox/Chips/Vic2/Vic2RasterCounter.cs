using System.Linq.Expressions;

namespace Breadbox.Chips.Vic2
{
    /// <summary>
    /// Emulates behavior of X/Y raster counters.
    /// </summary>
    public static class Vic2RasterCounter
    {
        public static Expression Clock(MemberExpression rasterX, MemberExpression rasterY, Expression width, Expression lines)
        {
            return Expression.Block(
                Expression.PreIncrementAssign(rasterX),
                Expression.IfThen(Expression.Equal(rasterX, width), Expression.Block(
                    Expression.Assign(rasterX, Expression.Constant(0)),
                    Expression.PreIncrementAssign(rasterY),
                    Expression.IfThen(Expression.Equal(rasterY, lines), Expression.Assign(rasterY, Expression.Constant(0))))));
        }
    }
}
