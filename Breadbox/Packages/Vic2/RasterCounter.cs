using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Breadbox.Packages.Vic2
{
    public class RasterCounter
    {
        private readonly State _state;
        private readonly Config _config;
        private readonly AddressGenerator _addressGenerator;
        private readonly GraphicsGenerator _graphicsGenerator;
        private readonly SpriteGenerator _spriteGenerator;

        private readonly Expression _updateBadLineEnable;
        private readonly Expression _updateBadLine;
        private readonly Expression _updateSpriteYExpansionPerCycle;
        private readonly Expression _updateSpriteDma;

        public RasterCounter(State state, Config config)
        {
            _state = state;
            _config = config;
            _addressGenerator = new AddressGenerator(_state, _config);
            _graphicsGenerator = new GraphicsGenerator(_state, _config);
            _spriteGenerator = new SpriteGenerator(_state, _config);

            var rastery = _state.RASTER;

            _updateBadLineEnable = Util.Simplify(Expression.Switch(_state.RASTER,
                Expression.SwitchCase(
                    Util.Void(Expression.Assign(_state.BADLINEENABLE,
                        Expression.OrElse(_state.BADLINEENABLE, _state.DEN))), Expression.Constant(0x030)),
                Expression.SwitchCase(
                    Util.Void(Expression.Assign(_state.BADLINE,
                        Expression.Assign(_state.BADLINEENABLE, Expression.Constant(false)))),
                    Expression.Constant(0x0F8))));

            _updateBadLine = Expression.IfThen(Util.And(_state.BADLINEENABLE,
                Expression.GreaterThanOrEqual(rastery, Expression.Constant(0x30)),
                Expression.LessThanOrEqual(rastery, Expression.Constant(0xF7)),
                Expression.Equal(Expression.And(Expression.Constant(0x7), rastery), _state.YSCROLL)),
                Expression.Assign(_state.BADLINE, Expression.Constant(true)));

            _updateSpriteYExpansionPerCycle = Util.Void(
                Enumerable.Range(0, 8)
                    .Select(i => (Expression) Expression.OrAssign(_state.MnYET[i], Expression.Not(_state.MnYE[i])))
                    .ToArray());

            _updateSpriteDma = Util.Invoke(Util.Void(
                Enumerable.Range(0, 8)
                    .Select(i => Util.Void(
                        Expression.IfThen(_state.MnE[i],
                            Expression.IfThen(
                                Expression.Equal(Expression.And(_state.RASTER, Expression.Constant(0xFF)),
                                    Expression.And(_state.MnY[i], Expression.Constant(0xFF))),
                                Expression.IfThen(Expression.Not(_state.MnDMA[i]),
                                    Util.Void(Expression.Assign(_state.MnDMA[i], Expression.Constant(true)),
                                        Expression.Assign(_state.MCBASEn[i], Expression.Constant(0)),
                                        Expression.IfThen(_state.MnYE[i],
                                            Expression.Assign(_state.MnYET[i], Expression.Constant(false)))))))))
                    .ToArray()));
        }

        private Expression Cycle1 // MAC 18
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

        private Expression Cycle14 // MAC 44
        {
            get
            {
                return Util.Void(
                    Expression.Assign(_state.VC, _state.VCBASE),
                    Expression.Assign(_state.VMLI, Expression.Constant(0)),
                    Expression.IfThen(_state.BADLINE, Expression.Assign(_state.RC, Expression.Constant(0))));
            }
        }

        private Expression Cycle15 // MAC 46
        {
            get
            {
                return Util.Void();
            }
        }

        private Expression Cycle16 // MAC 48
        {
            get
            {
                return Util.Void();
            }
        }

        private Expression Cycle55 // MAC 0
        {
            get
            {
                var expressions = new List<Expression>();
                expressions.AddRange(
                    Enumerable.Range(0, 8)
                        .Select(
                            i => Expression.ExclusiveOrAssign(_state.MnYET[i], _state.MnYE[i])));
                expressions.Add(Cycle56);
                return Util.Void(expressions.ToArray());
            }
        }

        private Expression Cycle56 // MAC 0+2
        {
            get
            {
                return _updateSpriteDma;
            }
        }

        private Expression Cycle58 // MAC 6
        {
            get
            {
                return Util.Void(
                    Expression.IfThen(Expression.Equal(_state.RC, Expression.Constant(0x7)), Util.Void(Expression.Assign(_state.IDLE, Expression.Constant(true)), Expression.Assign(_state.VCBASE, _state.VC))),
                    Expression.IfThen(Expression.Not(_state.IDLE), Expression.PreIncrementAssign(_state.RC)));
            }
        }

        private Expression Raster0
        {
            get
            {
                return Expression.Assign(_state.VCBASE, Expression.Assign(_state.RASTER, Expression.Constant(0)));
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
                new Tuple<Expression, Expression>(_config.MacToCounterX(18), Cycle1),
                new Tuple<Expression, Expression>(_config.MacToCounterX(44), Cycle14),
                new Tuple<Expression, Expression>(_config.MacToCounterX(46), Cycle15),
                new Tuple<Expression, Expression>(_config.MacToCounterX(48), Cycle16),
                new Tuple<Expression, Expression>(_config.MacToCounterX(0), Cycle55),
                new Tuple<Expression, Expression>(_config.MacToCounterX(2), Cycle56),
                new Tuple<Expression, Expression>(_config.MacToCounterX(6), Cycle58),
            };

            decodes.AddRange(Enumerable.Range(0, 43).Select(c => new Tuple<Expression, Expression>(_config.MacToCounterX(c * 2 + 41), Util.Void(_updateBadLineEnable, _updateBadLine))));
            decodes.AddRange(_addressGenerator.GetDecodes(readData, readColor));
            decodes.AddRange(Enumerable.Range(0, _config.ClocksPerRasterValue / 8).Select(c => new Tuple<Expression, Expression>(Expression.Constant(c * 8), _updateSpriteYExpansionPerCycle)));

            if (phi1 != null)
            {
                decodes.AddRange(
                    Enumerable.Range(0, _config.ClocksPerRasterValue/8)
                        .Select(
                            c => new Tuple<Expression, Expression>(Expression.Constant(c * 8), phi1)));
            }

            if (phi2 != null)
            {
                decodes.AddRange(
                    Enumerable.Range(0, _config.ClocksPerRasterValue/8)
                        .Select(
                            c => new Tuple<Expression, Expression>(Expression.Constant(c * 8 + 4), phi2)));
            }

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
                Util.Decode(decodes, Expression.PreIncrementAssign(rasterxcounter)),
                shouldHoldX != null ? Expression.IfThen(Expression.Not(shouldHoldX), notHeldX) : notHeldX,
                clockOutput
            };

            return Util.Void(block);
        }
    }
}
