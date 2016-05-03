using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Breadbox.Chips.Vic2
{
    public static class Vic2Mux
    {
        private static Expression IsGraphicsOutputtingForegroundColor(Expression graphicsData)
        {
            return Expression.GreaterThanOrEqual(graphicsData, Expression.Constant(0x02));
        }

        private static Expression IsSpriteOutputtingColor(Expression spriteData)
        {
            return Expression.NotEqual(spriteData, Expression.Constant(0));
        }

        private static Expression MuxedColor(Expression spritePriority, Expression spriteColor, Expression graphicsData, Expression graphicsColor)
        {
            return Expression.Condition(spritePriority,
                Expression.Condition(IsGraphicsOutputtingForegroundColor(graphicsData), graphicsColor, spriteColor),
                spriteColor);
        }

        public static Expression OutputSpriteSpriteCollisions(IEnumerable<Expression> spriteDatas)
        {
            var allCollisions = spriteDatas.Select((sd, i) => Expression.Condition(IsSpriteOutputtingColor(sd), Expression.Constant(1 << i), Expression.Constant(0)) as Expression).Aggregate(Expression.Or);
            return Expression.Switch(allCollisions, allCollisions,
                Expression.SwitchCase(Expression.Constant(0x00), Expression.Constant(0x00), Expression.Constant(0x01), Expression.Constant(0x02), Expression.Constant(0x04), Expression.Constant(0x08),
                Expression.Constant(0x10), Expression.Constant(0x20), Expression.Constant(0x40), Expression.Constant(0x80)));
        }

        public static Expression OutputSpriteBackgroundCollisions(IEnumerable<Expression> spriteDatas, Expression graphicsData)
        {
            var allCollisions = spriteDatas.Select((sd, i) => Expression.Condition(IsSpriteOutputtingColor(sd), Expression.Constant(1 << i), Expression.Constant(0)) as Expression).Aggregate(Expression.Or);
            return Expression.Condition(IsGraphicsOutputtingForegroundColor(graphicsData), allCollisions,
                Expression.Constant(0x00));
        }

        public static Expression OutputColor(IList<Expression> spriteDatas, IList<Expression> spriteColors, IList<MemberExpression> spritePriorities, Expression graphicsData,
            Expression graphicsColor)
        {
            var cachedGraphicsData = Expression.Variable(typeof(int));
            var cachedGraphicsColor = Expression.Variable(typeof(int));

            Expression result = cachedGraphicsColor;
            for (var i = spriteDatas.Count - 1; i >= 0; i--)
            {
                result = Expression.Condition(IsSpriteOutputtingColor(spriteDatas[i]), MuxedColor(spritePriorities[i], spriteColors[i], cachedGraphicsData, cachedGraphicsColor), result);
            }

            return Expression.Block(new[] { cachedGraphicsData, cachedGraphicsColor },
                Expression.Assign(cachedGraphicsData, graphicsData),
                Expression.Assign(cachedGraphicsColor, graphicsColor),
                result);
        }
    }
}
