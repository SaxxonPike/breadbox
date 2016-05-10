using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Breadbox.Packages.Vic2
{
    public class RasterCounter
    {
        private readonly State _state;
        private readonly Config _config;
        private readonly GraphicsGenerator _graphicsGenerator;
        private readonly SpriteGenerator _spriteGenerator;

        public RasterCounter(State state, Config config)
        {
            _state = state;
            _config = config;
            _graphicsGenerator = new GraphicsGenerator(_state, _config);
            _spriteGenerator = new SpriteGenerator(_state, _config);
        }

        private Expression GenerateDecoder(IEnumerable<Tuple<int, Expression>> decodes, Expression comparedValue)
        {
            var presentDecodes = decodes.Where(d => d.Item2 != null).ToList();
            if (presentDecodes.Count == 0)
            {
                return Expression.Empty();
            }
            if (presentDecodes.Count == 1)
            {
                return Expression.IfThen(Expression.Equal(Expression.Constant(presentDecodes[0].Item1), comparedValue), presentDecodes[0].Item2);
            }
            return Expression.Switch(comparedValue, GenerateDecodes(presentDecodes));
        }

        private SwitchCase[] GenerateDecodes(IEnumerable<Tuple<int, Expression>> decodes)
        {
            return
                decodes.GroupBy(d => d.Item1).Select(
                    g =>
                        Expression.SwitchCase(Util.Void(g.Select(pair => pair.Item2).ToArray()),
                            Expression.Constant(g.Key))).ToArray();
        }

        private Expression Cycle1
        {
            get
            {
                var rastery = _state.RASTER;
                var vblank = _state.VBLANK;

                var verticalDecodes = new[]
                {
                    new Tuple<int, Expression>(_config.RastersPerFrameValue, Raster0),
                    new Tuple<int, Expression>(_config.VBlankSetYValue, Expression.Assign(vblank, Expression.Constant(true))),
                    new Tuple<int, Expression>(_config.VBlankClearYValue, Expression.Assign(vblank, Expression.Constant(false)))
                };

                return Util.Void(
                    Expression.PreIncrementAssign(rastery),
                    GenerateDecoder(verticalDecodes, rastery)
                    );
            }
        }

        private Expression Raster0
        {
            get
            {
                return Util.Void(
                    Expression.Assign(_state.RASTER, Expression.Constant(0))
                    );
            }
        }

        private Expression ResetX(Expression rasterx, Expression rasterxc)
        {
            return Util.Void(
                Expression.Assign(rasterx, Expression.Constant(0)),
                Expression.Assign(rasterxc, Expression.Constant(0)));
        }

        public Expression Clock(Expression clock2mhz, Expression phi1, Expression phi2, Expression clockOutput)
        {
            var rasterx = _state.RASTERX;
            var rasterxcounter = _state.RASTERXC;
            var shouldHoldX = _config.ClocksPerRasterValue > 512 ? Util.All(Expression.GreaterThan(rasterxcounter, _config.HBlankSetX),
                Expression.LessThanOrEqual(rasterxcounter,
                    Expression.Constant(_config.HBlankSetXValue + (_config.ClocksPerRasterValue - 512)))) : null;

            var counterDecodes = new[]
            {
                new Tuple<int, Expression>(_config.ClocksPerRasterValue, ResetX(rasterx, rasterxcounter)),
            };

            var positionDecodes = new[]
            {
                new Tuple<int, Expression>(_config.HBlankSetXValue, Expression.Assign(_state.HBLANK, Expression.Constant(true))),
                new Tuple<int, Expression>(_config.HBlankClearXValue, Expression.Assign(_state.HBLANK, Expression.Constant(false))),
                new Tuple<int, Expression>(_config.RasterStartXValue, Cycle1),
            };

            var notHeldX = Util.Void(
                Expression.PreIncrementAssign(rasterx),
                GenerateDecoder(positionDecodes, rasterx),
                _graphicsGenerator.Clock,
                _spriteGenerator.Clock(0),
                _spriteGenerator.Clock(1),
                _spriteGenerator.Clock(2),
                _spriteGenerator.Clock(3),
                _spriteGenerator.Clock(4),
                _spriteGenerator.Clock(5),
                _spriteGenerator.Clock(6),
                _spriteGenerator.Clock(7)
                );

            var block = new[]
            {
                Expression.PreIncrementAssign(rasterxcounter),
                GenerateDecoder(counterDecodes, rasterxcounter), 
                shouldHoldX != null ? Expression.IfThen(Expression.Not(shouldHoldX), notHeldX) : notHeldX,
                clock2mhz != null ? Expression.IfThen(Expression.Equal(Expression.And(rasterxcounter, Expression.Constant(0x3)), Expression.Constant(0)), clock2mhz) : null,
                phi1 != null ? Expression.IfThen(Expression.Equal(Expression.And(rasterxcounter, Expression.Constant(0x7)), Expression.Constant(0)), phi1) : null,
                phi2 != null ? Expression.IfThen(Expression.Equal(Expression.And(rasterxcounter, Expression.Constant(0x7)), Expression.Constant(4)), phi2) : null,
                clockOutput
            };

            return Util.Void(block.Where(e => e != null).ToArray());
        }
    }
}
