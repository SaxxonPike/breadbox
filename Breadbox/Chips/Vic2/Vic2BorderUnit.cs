using System.Linq.Expressions;
using Breadbox.Extensions;

namespace Breadbox.Chips.Vic2
{
    public static class Vic2BorderUnit
    {
        public static Expression Clock(MemberExpression mainBorderFlipFlop, MemberExpression verticalBorderFlipFlop,
            Expression rasterX, Expression rasterY, Expression csel, Expression rsel, Expression cycle63X, Expression den)
        {
            var leftBorder = Expression.Condition(csel, Expression.Constant(0x018), Expression.Constant(0x01F));
            var rightBorder = Expression.Condition(csel, Expression.Constant(0x158), Expression.Constant(0x14F));
            var topBorder = Expression.Condition(rsel, Expression.Constant(0x033), Expression.Constant(0x037));
            var bottomBorder = Expression.Condition(rsel, Expression.Constant(0x0FB), Expression.Constant(0x0F7));

            var isCycle63 = Expression.Equal(rasterX, cycle63X);
            var isLeftBorder = Expression.Equal(rasterX, leftBorder);
            var isRightBorder = Expression.Equal(rasterX, rightBorder);
            var isTopBorder = Expression.Equal(rasterY, topBorder);
            var isBottomBorder = Expression.Equal(rasterY, bottomBorder);

            var cachedIsCycle63 = Expression.Parameter(typeof(bool));
            var cachedIsLeftBorder = Expression.Parameter(typeof(bool));
            var cachedIsTopBorder = Expression.Parameter(typeof(bool));
            var cachedIsBottomBorder = Expression.Parameter(typeof(bool));

            return Expression.Block(new[] { cachedIsCycle63, cachedIsLeftBorder, cachedIsTopBorder, cachedIsBottomBorder },
                Expression.Assign(cachedIsCycle63, isCycle63),
                Expression.Assign(cachedIsLeftBorder, isLeftBorder),
                Expression.Assign(cachedIsTopBorder, isTopBorder),
                Expression.Assign(cachedIsBottomBorder, isBottomBorder),
                Expression.IfThen(isRightBorder, Util.Set(mainBorderFlipFlop)),
                Expression.IfThen(Expression.AndAlso(cachedIsBottomBorder, cachedIsCycle63), Util.Set(verticalBorderFlipFlop)),
                Expression.IfThen(Expression.AndAlso(cachedIsTopBorder, cachedIsCycle63), Util.Reset(verticalBorderFlipFlop)),
                Expression.IfThen(Expression.AndAlso(cachedIsLeftBorder, cachedIsBottomBorder), Util.Set(verticalBorderFlipFlop)),
                Expression.IfThen(Expression.AndAlso(Expression.AndAlso(den, cachedIsLeftBorder), cachedIsTopBorder), Util.Reset(verticalBorderFlipFlop)),
                Expression.IfThen(Expression.AndAlso(cachedIsLeftBorder, Expression.IsFalse(verticalBorderFlipFlop)), Util.Reset(mainBorderFlipFlop)));
        }

        public static Expression OutputColor(Expression mainBorderFlipFlop, Expression verticalBorderFlipFlop, Expression inputColor, Expression borderColor)
        {
            return Expression.Condition(Expression.OrElse(mainBorderFlipFlop, verticalBorderFlipFlop), borderColor, inputColor);
        }
    }
}
