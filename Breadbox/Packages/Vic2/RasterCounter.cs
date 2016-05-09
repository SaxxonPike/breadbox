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
                var isNewFrame = Expression.Equal(rastery, _config.RastersPerFrame);
                var shouldEnableVblank = Expression.Equal(rastery, _config.VBlankSetY);
                var shouldDisableVblank = Expression.Equal(rastery, _config.VBlankClearY);

                return Util.Void(
                    Expression.PreIncrementAssign(rastery),
                    Expression.IfThen(isNewFrame, Raster0),
                    Expression.IfThenElse(shouldDisableVblank, Expression.Assign(vblank, Expression.Constant(false)), Expression.IfThen(shouldEnableVblank, Expression.Assign(vblank, Expression.Constant(true))))
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
            var shouldWrapX = Expression.Equal(rasterxcounter, _config.ClocksPerRaster);
            var shouldHoldX = _config.ClocksPerRasterValue > 512 ? Util.All(Expression.GreaterThan(rasterxcounter, _config.HBlankSetX),
                Expression.LessThanOrEqual(rasterxcounter,
                    Expression.Constant(_config.HBlankSetXValue + (_config.ClocksPerRasterValue - 512)))) : null;
            var shouldEnableHblank = Expression.Equal(rasterx, _config.HBlankSetX);
            var shouldDisableHblank = Expression.Equal(rasterx, _config.HBlankClearX);
            var notHeldX = Util.Void(
                Expression.PreIncrementAssign(rasterx),
                Expression.Switch(rasterx,
                    Expression.SwitchCase(Cycle1, _config.RasterStartX)
                ),
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
                shouldHoldX != null ? Expression.IfThen(Expression.Not(shouldHoldX), notHeldX) : notHeldX,
                Expression.IfThen(shouldWrapX, ResetX(rasterx, rasterxcounter)),
                Expression.IfThenElse(shouldDisableHblank, Expression.Assign(_state.HBLANK, Expression.Constant(false)), Expression.IfThen(shouldEnableHblank, Expression.Assign(_state.HBLANK, Expression.Constant(true)))),
                clock2mhz != null ? Expression.IfThen(Expression.Equal(Expression.And(rasterxcounter, Expression.Constant(0x3)), Expression.Constant(0)), clock2mhz) : null,
                phi1 != null ? Expression.IfThen(Expression.Equal(Expression.And(rasterxcounter, Expression.Constant(0x7)), Expression.Constant(0)), phi1) : null,
                phi2 != null ? Expression.IfThen(Expression.Equal(Expression.And(rasterxcounter, Expression.Constant(0x7)), Expression.Constant(4)), phi2) : null,
                clockOutput
            };

            return Util.Void(block.Where(e => e != null).ToArray());
        }
    }
}
