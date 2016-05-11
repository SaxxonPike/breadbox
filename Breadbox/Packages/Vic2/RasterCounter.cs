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

        private Expression Cycle1
        {
            get
            {
                var rastery = _state.RASTER;
                var vblank = _state.VBLANK;

                var verticalDecodes = new[]
                {
                    new Tuple<Expression, Expression>(_config.RastersPerFrame, Raster0),
                    new Tuple<Expression, Expression>(_config.VBlankSetY, Expression.Assign(vblank, Expression.Constant(true))),
                    new Tuple<Expression, Expression>(_config.VBlankClearY, Expression.Assign(vblank, Expression.Constant(false)))
                };

                return Util.Void(
                    Expression.PreIncrementAssign(rastery),
                    Util.Decode(verticalDecodes, rastery)
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

            var decodes = new List<Tuple<Expression, Expression>>
            {
                new Tuple<Expression, Expression>(_config.ClocksPerRaster, ResetX(rasterx, rasterxcounter)),
                new Tuple<Expression, Expression>(_config.RasterXToCounterX(_config.HBlankSetXValue), Expression.Assign(_state.HBLANK, Expression.Constant(true))),
                new Tuple<Expression, Expression>(_config.RasterXToCounterX(_config.HBlankClearXValue), Expression.Assign(_state.HBLANK, Expression.Constant(false))),
                new Tuple<Expression, Expression>(_config.RasterXToCounterX(_config.RasterStartXValue), Cycle1)
            };

            var notHeldX = Util.Void(
                Expression.PreIncrementAssign(rasterx),
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
                Util.Decode(decodes, rasterxcounter),
                shouldHoldX != null ? Expression.IfThen(Expression.Not(shouldHoldX), notHeldX) : notHeldX,
                clock2mhz == null ? null : Util.Decode(Enumerable.Range(0, _config.ClocksPerRasterValue / 4).Select(c => new Tuple<Expression, Expression>(Expression.Constant(c * 4), clock2mhz)), rasterxcounter),
                phi1 == null ? null : Util.Decode(Enumerable.Range(0, _config.ClocksPerRasterValue / 8).Select(c => new Tuple<Expression, Expression>(Expression.Constant((c * 8) + 0), phi1)), rasterxcounter),
                phi2 == null ? null : Util.Decode(Enumerable.Range(0, _config.ClocksPerRasterValue / 8).Select(c => new Tuple<Expression, Expression>(Expression.Constant((c * 8) + 4), phi2)), rasterxcounter),
                clockOutput
            };

            return Util.Void(block.Where(e => e != null).ToArray());
        }
    }
}
