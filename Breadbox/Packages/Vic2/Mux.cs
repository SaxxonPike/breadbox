using System.Linq;
using System.Linq.Expressions;

namespace Breadbox.Packages.Vic2
{
    public class Mux
    {
        private readonly State _state;
        private readonly Config _config;
        private readonly GraphicsGenerator _graphics;
        private readonly SpriteGenerator _sprite;

        public readonly Expression OutputColor;

        public Mux(State state, Config config)
        {
            _state = state;
            _config = config;
            _graphics = new GraphicsGenerator(_state, _config);
            _sprite = new SpriteGenerator(state, config);
            OutputColor = GetOutputColorExpression();
        }

        private Expression SpriteOutputIsNonTransparent(int spriteNumber)
        {
            return Expression.NotEqual(Expression.Constant(0), _sprite.OutputData(spriteNumber));
        }

        private Expression GetOutputColorExpression()
        {
            var graphicsData = Expression.Parameter(typeof(int));
            var graphicsColor = Expression.Parameter(typeof(int));
            var graphicsIsForeground = Expression.GreaterThanOrEqual(graphicsData, Expression.Constant(0x80));
            var inner = (Expression)graphicsColor;
            var muxedColor = Enumerable.Range(0, 8).Select(i => 7 - i).Aggregate(inner, (expression, i) =>
                Expression.Condition(SpriteOutputIsNonTransparent(i),
                    Expression.Condition(Expression.AndAlso(graphicsIsForeground, _state.MnDP[i]), graphicsColor,
                        _sprite.OutputColor(i)), expression));
            var lambda = Expression.Lambda(muxedColor, true, graphicsData, graphicsColor);
            return Expression.Invoke(lambda, _graphics.OutputData, _graphics.OutputColor);
        }
    }
}
