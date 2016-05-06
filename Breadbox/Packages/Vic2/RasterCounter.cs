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

        public RasterCounter(State state, Config config)
        {
            _state = state;
            _config = config;
        }

        private Expression Cycle1
        {
            get
            {
                var rastery = _state.RASTER;
                var isNewFrame = Expression.Equal(rastery, _config.RastersPerFrame);

                return Expression.Block(
                    Expression.PreIncrementAssign(_state.RASTER),
                    Expression.IfThen(isNewFrame, Raster0)
                    );
            }
        }

        private Expression Raster0
        {
            get
            {
                return Expression.Block(
                    Expression.Empty()
                    );
            }
        }

        private Expression ResetX(Expression rasterx, Expression rasterxc)
        {
            return Expression.Block(Expression.Assign(rasterx, Expression.Constant(0)),
                Expression.Assign(rasterxc, Expression.Constant(0)));
        }

        public Expression Clock(Expression clock8mhz, Expression clock2mhz, Expression phi1, Expression phi2)
        {
            var rasterx = _state.RASTERX;
            var rasterxcounter = _state.RASTERXC;
            var shouldWrapX = Expression.Equal(rasterxcounter, _config.ClocksPerRaster);
            var shouldHoldX = _config.ClocksPerRasterValue > 512 ? Util.All(Expression.GreaterThan(rasterxcounter, _config.HBlankSetX),
                Expression.LessThanOrEqual(rasterxcounter,
                    Expression.Constant(_config.HBlankSetXValue + (_config.ClocksPerRasterValue - 512)))) : null;

            var block = new[]
            {
                Expression.PreIncrementAssign(rasterxcounter),
                shouldHoldX != null ? (Expression)Expression.IfThen(Expression.Not(shouldHoldX), Expression.PreIncrementAssign(rasterx)) : Expression.PreIncrementAssign(rasterx),
                Expression.Switch(rasterx,
                    Expression.SwitchCase(Cycle1, _config.RasterStartX)
                ), 
                Expression.IfThen(shouldWrapX, ResetX(rasterx, rasterxcounter)),
                clock8mhz,
                clock2mhz != null ? Expression.IfThen(Expression.Equal(Expression.And(rasterxcounter, Expression.Constant(0x3)), Expression.Constant(0)), clock2mhz) : null,
                phi1 != null ? Expression.IfThen(Expression.Equal(Expression.And(rasterxcounter, Expression.Constant(0x7)), Expression.Constant(0)), phi1) : null,
                phi2 != null ? Expression.IfThen(Expression.Equal(Expression.And(rasterxcounter, Expression.Constant(0x7)), Expression.Constant(4)), phi2) : null,
            };

            return Util.Void(block.Where(e => e != null).ToArray());
        }
    }
}
