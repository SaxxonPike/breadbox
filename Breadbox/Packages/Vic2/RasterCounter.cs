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
        private readonly AddressGenerator _addressGenerator;
        private readonly GraphicsGenerator _graphicsGenerator;
        private readonly SpriteGenerator _spriteGenerator;

        public RasterCounter(State state, Config config)
        {
            _state = state;
            _config = config;
            _addressGenerator = new AddressGenerator(_state, _config);
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

                return Util.Decode(verticalDecodes, Expression.PreIncrementAssign(rastery));
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
            return Expression.Assign(rasterx, Expression.Assign(rasterxc, Expression.Constant(0)));
        }

        public Expression Clock(Func<Expression, Expression> readData, Func<Expression, Expression> readColor, Expression phi1, Expression phi2, Expression clockOutput)
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

            decodes.AddRange(_addressGenerator.GetDecodes(readData, readColor));

            var notHeldX = Util.Void(
                Expression.PreIncrementAssign(rasterx),
                Util.Invoke(_graphicsGenerator.Clock),
                Util.Invoke(_spriteGenerator.Clock(0)),
                Util.Invoke(_spriteGenerator.Clock(1)),
                Util.Invoke(_spriteGenerator.Clock(2)),
                Util.Invoke(_spriteGenerator.Clock(3)),
                Util.Invoke(_spriteGenerator.Clock(4)),
                Util.Invoke(_spriteGenerator.Clock(5)),
                Util.Invoke(_spriteGenerator.Clock(6)),
                Util.Invoke(_spriteGenerator.Clock(7))
                );

            var block = new[]
            {
                Expression.PreIncrementAssign(rasterxcounter),
                Util.Decode(decodes, rasterxcounter),
                shouldHoldX != null ? Expression.IfThen(Expression.Not(shouldHoldX), notHeldX) : notHeldX,
                phi1 == null ? null : Util.Decode(Enumerable.Range(0, _config.ClocksPerRasterValue / 8).Select(c => new Tuple<Expression, Expression>(Expression.Constant((c * 8) + 0), Util.Invoke(phi1))), rasterxcounter),
                phi2 == null ? null : Util.Decode(Enumerable.Range(0, _config.ClocksPerRasterValue / 8).Select(c => new Tuple<Expression, Expression>(Expression.Constant((c * 8) + 4), Util.Invoke(phi2))), rasterxcounter),
                clockOutput
            };

            return Util.Void(block);
        }
    }
}
