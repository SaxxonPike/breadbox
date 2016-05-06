using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Breadbox.Packages.Vic2
{
    public class Mux
    {
        private readonly State _state;
        private readonly Config _config;
        private readonly GraphicsGenerator _graphics;
        private readonly SpriteGenerator _sprite;

        public Mux(State state, Config config)
        {
            _state = state;
            _config = config;
            _graphics = new GraphicsGenerator(_state, _config);
            _sprite = new SpriteGenerator(state, config);
        }

        private Expression SpriteOutputIsNonTransparent(int spriteNumber)
        {
            return Expression.NotEqual(Expression.Constant(0), _sprite.OutputData(spriteNumber));
        }

        public Expression OutputColor
        {
            get
            {
                var graphicsData = Expression.Variable(typeof(int));
                var graphicsColor = Expression.Variable(typeof(int));
                var graphicsIsForeground = Expression.GreaterThanOrEqual(graphicsData, Expression.Constant(0x80));

                var inner = (Expression)graphicsColor;
                var muxedColor = Enumerable.Range(0, 8).Select(i => 7 - i).Aggregate(inner, (expression, i) =>
                    Expression.Condition(SpriteOutputIsNonTransparent(i),
                        Expression.Condition(Expression.AndAlso(graphicsIsForeground, _state.MnDP[i]), graphicsColor,
                            _sprite.OutputColor(i)), expression));

                return Expression.Block(new[] { graphicsData, graphicsColor },
                    Expression.Assign(graphicsData, _graphics.OutputData),
                    Expression.Assign(graphicsColor, _graphics.OutputColor),
                    muxedColor);
            }
        }
    }
}
