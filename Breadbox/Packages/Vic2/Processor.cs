using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Breadbox.Packages.Vic2
{
    public class Processor
    {
        #region MAC LIST (INFO)

        // 000 - idle/g:39 (MxYE check, DMA [55])
        // 001 - no access
        // 002 - idle (DMA [56])
        // 003 - no access
        // 004 - idle
        // 005 - no access

        // 006 - pointer 0 (if RC=7 go idle and VC->VCBASE, if not idle RC++, MCBASE-MC, SPR.DMA check [58])
        // 007 - sprite 0:0
        // 008 - sprite 0:1
        // 009 - sprite 0:2
        // 010 - pointer 1
        // 011 - sprite 1:0
        // 012 - sprite 1:1
        // 013 - sprite 1:2
        // 014 - pointer 2
        // 015 - sprite 2:0
        // 016 - sprite 2:1 (border unit check [63])
        // 017 - sprite 2:2
        // 018 - pointer 3 (advance raster [1])
        // 019 - sprite 3:0
        // 020 - sprite 3:1
        // 021 - sprite 3:2
        // 022 - pointer 4
        // 023 - sprite 4:0
        // 024 - sprite 4:1
        // 025 - sprite 4:2
        // 026 - pointer 5
        // 027 - sprite 5:0
        // 028 - sprite 5:1
        // 029 - sprite 5:2
        // 030 - pointer 6
        // 031 - sprite 6:0
        // 032 - sprite 6:1
        // 033 - sprite 6:2
        // 034 - pointer 7
        // 035 - sprite 7:0
        // 036 - sprite 7:1
        // 037 - sprite 7:2

        // 038 - refresh
        // 039 - no access
        // 040 - refresh (badline BA [12])
        // 041 - no access
        // 042 - refresh
        // 043 - no access
        // 044 - refresh (VCBASE->VC, reset RC on badline [14])
        // 045 - no access
        // 046 - refresh (sprite crunch [15])

        // 047 - c:00
        // 048 - g:00 (sprite crunch [16])
        // ...

        #endregion

        private readonly State _state;
        private readonly Config _config;

        private readonly Expression[] _spriteOutputColor;
        private readonly Expression[] _spriteOutputData;
        private readonly Expression[] _spriteClock;

        private readonly Expression _graphicsOutputColor;
        private readonly Expression _graphicsOutputData;
        private readonly Expression _graphicsClock;

        private readonly Expression _muxOutput;

        private readonly Expression _borderUnitOutput;

        private readonly Expression _updateBadLineEnable;
        private readonly Expression _updateBadLine;
        private readonly Expression _updateSpriteYExpansionPerCycle;
        private readonly Expression _updateSpriteDma;
        private readonly Expression _updateSpriteDispEnable;
        private readonly Expression _updateSpriteMcBase;
        private readonly Expression _disableSpriteShiftReg;

        private readonly Expression _topBorderValue;
        private readonly Expression _bottomBorderValue;
        private readonly Expression _updateLeftBorder;
        private readonly Expression _updateRightBorder;
        private readonly Expression _updateCycle63Border;

        public Processor(State state, Config config)
        {
            _state = state;
            _config = config;

            var rastery = _state.RASTER;

            #region EXPRESSION CACHE

            _topBorderValue = GetTopBorderValue();
            _bottomBorderValue = GetBottomBorderValue();
            _updateLeftBorder = GetUpdateLeftBorder();
            _updateRightBorder = GetUpdateRightBorder();
            _updateCycle63Border = GetUpdateCycle63Border();

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

            _updateSpriteYExpansionPerCycle = Util.Invoke(Util.Void(
                Enumerable.Range(0, 8)
                    .Select(i => (Expression)Expression.OrAssign(_state.MnYET[i], Expression.Not(_state.MnYE[i])))
                    .ToArray()));

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

            _updateSpriteDispEnable = Util.Invoke(Util.Void(
                Enumerable.Range(0, 8)
                    .Select(
                        i =>
                            Util.Void(Expression.Assign(_state.MCn[i], _state.MCBASEn[i]),
                                Expression.IfThen(
                                    Util.All(_state.MnDMA[i],
                                        Expression.Equal(Expression.And(_state.RASTER, Expression.Constant(0xFF)),
                                            Expression.And(_state.MnY[i], Expression.Constant(0xFF)))),
                                    Expression.Assign(_state.MnDISP[i], Expression.Constant(true)))))
                    .ToArray()));

            _disableSpriteShiftReg = Util.Void(
                Enumerable.Range(0, 8)
                    .Select(
                        i =>
                            (Expression)Expression.Assign(_state.MSREn[i], Expression.Constant(false)))
                    .ToArray());

            Func<int, Expression> crunchedMcBase = spriteNumber =>
                Util.Or(
                    Util.And(
                        _state.MCBASEn[spriteNumber],
                        _state.MCn[spriteNumber],
                        Expression.Constant(0x2A)),
                    Expression.And(
                        Expression.Or(
                            _state.MCBASEn[spriteNumber],
                            _state.MCn[spriteNumber]),
                        Expression.Constant(0x15)));

            _updateSpriteMcBase = Util.Invoke(Util.Void(
                Enumerable.Range(0, 8)
                    .Select(
                        i =>
                            Util.Void(
                                Expression.Assign(_state.MCBASEn[i],
                                    Expression.Condition(_state.MnCRUNCH[i], crunchedMcBase(i), _state.MCn[i])),
                                Expression.Assign(_state.MnCRUNCH[i], Expression.Constant(false))
                                )).ToArray()));

            _spriteOutputColor = Enumerable.Range(0, 8).Select(GetSpriteOutputColor).ToArray();
            _spriteOutputData = Enumerable.Range(0, 8).Select(GetSpriteOutputData).ToArray();
            _spriteClock = Enumerable.Range(0, 8).Select(GetSpriteClock).ToArray();

            _graphicsOutputColor = Util.Invoke(GetGraphicsOutputColor());
            _graphicsOutputData = Util.Invoke(GetGraphicsOutputData());
            _graphicsClock = Util.Invoke(GetGraphicsClock());

            _muxOutput = Util.Invoke(GetMuxOutput());

            _borderUnitOutput = Util.Invoke(GetBorderUnitOutput());

            #endregion
        }

        #region ADDRESS GENERATOR AND DATA I/O INTERFACE

        private IEnumerable<Tuple<Expression, Expression>> GetAddressGeneratorDecodes(Func<Expression, Expression> readData, Func<Expression, Expression> readColorMemory)
        {
            var ecm = _state.ECM;
            var vc = _state.VC;
            var vm = _state.VM;
            var vmli = _state.VMLI;
            var rc = _state.RC;
            var cb = _state.CB;
            var mc = _state.MCn;
            var mp = _state.MPn;
            var idle = _state.IDLE;
            var bmm = _state.BMM;
            var md = _state.MDn;
            var gd = _state.GD;
            var refresh = _state.REF;

            // Configuration

            var idleAccess = Expression.Constant(0x3FFF);
            var idleRead = readData(idleAccess);

            // C accesses

            var colorAccess = Expression.Or(Expression.LeftShift(vm, Expression.Constant(10)), vc);
            var colorAssign = Util.Void(_state.Dn.AssignUsing(vmli, Expression.Assign(_state.CBUFFER, Expression.Or(readData(colorAccess), Expression.LeftShift(readColorMemory(colorAccess), Expression.Constant(8))))));
            var colorClear = Util.Void(Expression.Assign(_state.CBUFFER, Expression.Constant(0)));
            var colorAssignOnBadline = Expression.IfThenElse(_state.BADLINE, colorAssign, Util.Void(idleRead));

            // G accesses

            var graphicsAccessBase = Expression.Condition(ecm, Expression.Constant(0x39FF), Expression.Constant(0x3FFF));
            var characterPointer = Expression.And(_state.Dn.SelectUsing(vmli), Expression.Constant(0xFF));
            var characterAccess = Util.Or(Expression.LeftShift(cb, Expression.Constant(11)),
                Expression.LeftShift(characterPointer, Expression.Constant(3)), rc);
            var bitmapAccess =
                Util.Or(Expression.LeftShift(Expression.And(cb, Expression.Constant(0x4)), Expression.Constant(13)),
                    Expression.LeftShift(vc, Expression.Constant(3)), rc);
            var nonIdleGraphicsAccess = Expression.And(graphicsAccessBase, Expression.Condition(bmm, bitmapAccess, characterAccess));
            var graphicsAccess = Expression.Condition(idle, graphicsAccessBase, nonIdleGraphicsAccess);
            var graphicsAssign = Util.Void(Expression.Assign(gd, readData(graphicsAccess)), Expression.PreIncrementAssign(vmli), Expression.PreIncrementAssign(vc));

            // R accesses

            var refreshAccess = Expression.Or(Expression.Constant(0x3F00), refresh);
            var refreshAssign = Util.Void(Expression.Assign(refresh, Expression.And(Expression.Decrement(refresh), Expression.Constant(0xFF))), readData(refreshAccess));

            // S and P accesses

            Func<int, Expression> spritePointerAccess = spriteNumber => Util.Or(Expression.Constant(spriteNumber | 0x1F8), Expression.LeftShift(vm, Expression.Constant(10)));
            Func<int, Expression> spriteDataAccess = spriteNumber => Util.Or(Expression.LeftShift(mp[spriteNumber], Expression.Constant(6)), mc[spriteNumber]);

            Func<int, Expression> spritePointerAssign = spriteNumber => Util.Void(Expression.Assign(mp[spriteNumber], spritePointerAccess(spriteNumber)));
            Func<int, Expression> spriteDataAssign = spriteNumber => Expression.IfThenElse(_state.MnDMA[spriteNumber],
                Util.Void(Expression.LeftShiftAssign(md[spriteNumber], Expression.Constant(8)), Expression.OrAssign(md[spriteNumber], readData(spriteDataAccess(spriteNumber)))),
                Util.Void(readData(idleAccess)));

            // all together now

            var result = new List<Tuple<Expression, Expression>>();

            var invokeI = idleRead;
            var invokeX = idleRead;
            var invokeR = refreshAssign;
            var invokeC = colorAssignOnBadline;
            var invokeG = graphicsAssign;
            var invokeS = Enumerable.Range(0, 8).Select(spriteDataAssign).ToArray();
            var invokeP = Enumerable.Range(0, 8).Select(spritePointerAssign).ToArray();

            result.AddRange(Enumerable.Range(0, 2).Select(i => new Tuple<Expression, Expression>(_config.MacToCounterX(2 + i * 2), invokeI)));
            result.AddRange(Enumerable.Range(0, 2).Select(i => new Tuple<Expression, Expression>(_config.MacToCounterX(3 + i * 2), invokeX)));
            result.AddRange(Enumerable.Range(0, 8).Select(i => new Tuple<Expression, Expression>(_config.MacToCounterX(6 + i * 4), invokeP[i])));
            result.AddRange(Enumerable.Range(0, 8).Select(i => new Tuple<Expression, Expression>(_config.MacToCounterX(7 + i * 4), invokeS[i])));
            result.AddRange(Enumerable.Range(0, 8).Select(i => new Tuple<Expression, Expression>(_config.MacToCounterX(8 + i * 4), invokeS[i])));
            result.AddRange(Enumerable.Range(0, 8).Select(i => new Tuple<Expression, Expression>(_config.MacToCounterX(9 + i * 4), invokeS[i])));
            result.AddRange(Enumerable.Range(0, 5).Select(i => new Tuple<Expression, Expression>(_config.MacToCounterX(38 + i * 2), invokeR)));
            result.AddRange(Enumerable.Range(0, 4).Select(i => new Tuple<Expression, Expression>(_config.MacToCounterX(39 + i * 2), invokeX)));
            result.AddRange(Enumerable.Range(0, 40).Select(i => new Tuple<Expression, Expression>(_config.MacToCounterX(47 + i * 2), invokeC)));
            result.AddRange(Enumerable.Range(0, 40).Select(i => new Tuple<Expression, Expression>(_config.MacToCounterX(48 + i * 2), invokeG)));
            result.Add(new Tuple<Expression, Expression>(_config.MacToCounterX(127), colorClear));
            result.AddRange(Enumerable.Range(0, (_config.ClocksPerRasterValue - 0x1F8) / 8).Select(i => new Tuple<Expression, Expression>(_config.MacToCounterX(127 + i * 2), invokeX)));
            result.AddRange(Enumerable.Range(0, (_config.ClocksPerRasterValue - 0x1F8) / 8).Select(i => new Tuple<Expression, Expression>(_config.MacToCounterX(128 + i * 2), invokeI)));
            return result;
        }

        #endregion

        #region SPRITE UNIT

        private Expression GetSpriteClock(int spriteNumber)
        {
            var multicolor = _state.MnMC[spriteNumber];
            var mxmc = _state.MXMCn[spriteNumber];
            var newMxmc = Expression.Assign(mxmc, Expression.Not(mxmc));
            var shiftAssign = Expression.LeftShiftAssign(_state.MDn[spriteNumber],
                Expression.Condition(multicolor,
                    Expression.Condition(newMxmc, Expression.Constant(2), Expression.Constant(0)),
                    Expression.Constant(1)));
            return Util.Void(
                Expression.IfThen(Expression.Equal(_state.MnX[spriteNumber], _state.RASTERX),
                    Expression.Assign(_state.MSREn[spriteNumber], Expression.Constant(true))),
                Expression.IfThen(_state.MSREn[spriteNumber], shiftAssign)
                );
        }

        private Expression GetSpriteOutputColor(int spriteNumber)
        {
            var mobData = _state.MDn[spriteNumber];
            var multiColorSprite = _state.MnMC[spriteNumber];
            var foregroundColor = _state.MnC[spriteNumber];
            var singleColor =
                Expression.Condition(
                    Expression.NotEqual(Expression.Constant(0), Expression.And(mobData, Expression.Constant(0x800000))),
                    foregroundColor, Expression.Constant(0));
            var multiColor = Expression.Switch(Expression.And(mobData, Expression.Constant(0xC00000)),
                Expression.Constant(0),
                Expression.SwitchCase(_state.MM0, Expression.Constant(0x400000)),
                Expression.SwitchCase(foregroundColor, Expression.Constant(0x800000)),
                Expression.SwitchCase(_state.MM1, Expression.Constant(0xC00000)));
            return Expression.Condition(multiColorSprite, multiColor, singleColor);
        }

        private Expression GetSpriteOutputData(int spriteNumber)
        {
            var mobData = _state.MDn[spriteNumber];
            var multiColorSprite = _state.MnMC[spriteNumber];
            return Expression.And(mobData, Expression.Condition(multiColorSprite, Expression.Constant(0xC00000), Expression.Constant(0x800000)));
        }

        private Expression GetSpriteOutputIsNonTransparent(int spriteNumber)
        {
            return Expression.NotEqual(Expression.Constant(0), _spriteOutputData[spriteNumber]);
        }

        #endregion

        #region GRAPHICS UNIT

        private Expression GetGraphicsClock()
        {
            var isEvenRasterX = Expression.Equal(Expression.And(_state.RASTERXC, Expression.Constant(0x1)),
                Expression.Constant(0));
            return Expression.LeftShiftAssign(_state.GD,
                Expression.Condition(GraphicsCharacterIsMultiColor,
                    Expression.Condition(isEvenRasterX, Expression.Constant(2), Expression.Constant(0)),
                    Expression.Constant(1)));
        }

        private Expression GraphicsCharacterIsMultiColor
        {
            get
            {
                var matrixData = _state.CBUFFER;
                return Util.All(_state.MCM, Expression.NotEqual(Expression.And(matrixData, Expression.Constant(0x800)), Expression.Constant(0)));
            }
        }

        private Expression GetGraphicsCharacterColor()
        {
            var matrixData = _state.CBUFFER;
            var highMatrixData = Expression.RightShift(matrixData, Expression.Constant(8));
            var singleColor =
                Expression.Condition(
                    Expression.NotEqual(Expression.And(_state.GD, Expression.Constant(0x80)), Expression.Constant(0)),
                    highMatrixData, _state.B0C);
            var multiColor = Expression.Switch(Expression.And(_state.GD, Expression.Constant(0xC0)), _state.B0C,
                Expression.SwitchCase(_state.B1C, Expression.Constant(0x40)),
                Expression.SwitchCase(_state.B2C, Expression.Constant(0x80)),
                Expression.SwitchCase(Expression.And(highMatrixData, Expression.Constant(0x7)),
                    Expression.Constant(0xC0)));
            return Expression.Condition(GraphicsCharacterIsMultiColor, multiColor, singleColor);
        }

        private Expression GetGraphicsBitmapColor()
        {
            var matrixData = _state.CBUFFER;
            var lowMatrixData = Expression.And(matrixData, Expression.Constant(0xF));
            var midMatrixData = Expression.And(Expression.RightShift(matrixData, Expression.Constant(4)), Expression.Constant(0xF));
            var highMatrixData = Expression.RightShift(matrixData, Expression.Constant(8));
            var singleColor =
                Expression.Condition(
                    Expression.NotEqual(Expression.And(_state.GD, Expression.Constant(0x80)), Expression.Constant(0)),
                    midMatrixData, lowMatrixData);
            var multiColor = Expression.Switch(Expression.And(_state.GD, Expression.Constant(0xC0)), _state.B0C,
                Expression.SwitchCase(midMatrixData, Expression.Constant(0x40)),
                Expression.SwitchCase(lowMatrixData, Expression.Constant(0x80)),
                Expression.SwitchCase(highMatrixData, Expression.Constant(0xC0))
                );
            return Expression.Condition(_state.MCM, multiColor, singleColor);
        }

        private Expression GetGraphicsExtraColor()
        {
            var matrixData = _state.CBUFFER;
            var backgroundColorIndex = Expression.And(matrixData, Expression.Constant(0xC0));
            var backgroundColor = Expression.Switch(backgroundColorIndex, _state.B0C,
                Expression.SwitchCase(_state.B1C, Expression.Constant(0x40)),
                Expression.SwitchCase(_state.B2C, Expression.Constant(0x80)),
                Expression.SwitchCase(_state.B3C, Expression.Constant(0xC0)));
            var foregroundColor = Expression.RightShift(matrixData, Expression.Constant(8));
            return Expression.Condition(
                Expression.NotEqual(Expression.And(_state.GD, Expression.Constant(0x80)), Expression.Constant(0)),
                foregroundColor, backgroundColor);
        }

        private Expression GetGraphicsOutputData()
        {
            return Expression.And(_state.GD,
                Expression.Condition(_state.MCM, Expression.Constant(0xC0), Expression.Constant(0x80)));
        }

        private Expression GetGraphicsOutputColor()
        {
            var isInvalid = Expression.AndAlso(_state.ECM, Expression.OrElse(_state.MCM, _state.BMM));
            return Expression.Condition(isInvalid, Expression.Constant(0),
                Expression.Condition(_state.BMM, GetGraphicsBitmapColor(),
                    Expression.Condition(_state.ECM, GetGraphicsExtraColor(), GetGraphicsCharacterColor())));
        }

        #endregion

        #region MUX UNIT

        private Expression GetMuxOutput()
        {
            var graphicsData = Expression.Parameter(typeof(int));
            var graphicsColor = Expression.Parameter(typeof(int));
            var graphicsIsForeground = Expression.GreaterThanOrEqual(graphicsData, Expression.Constant(0x80));
            var inner = (Expression)graphicsColor;
            var muxedColor = Enumerable.Range(0, 8).Select(i => 7 - i).Aggregate(inner, (expression, i) =>
                Expression.Condition(GetSpriteOutputIsNonTransparent(i),
                    Expression.Condition(Expression.AndAlso(graphicsIsForeground, _state.MnDP[i]), graphicsColor,
                        _spriteOutputColor[i]), expression));
            var lambda = Expression.Lambda(muxedColor, true, graphicsData, graphicsColor);
            return Expression.Invoke(lambda, _graphicsOutputData, _graphicsOutputColor);
        }

        #endregion

        #region BORDER UNIT

        private Expression GetTopBorderValue()
        {
            return Expression.Condition(_state.RSEL, Expression.Constant(0x033), Expression.Constant(0x037));
        }

        private Expression GetBottomBorderValue()
        {
            return Expression.Condition(_state.RSEL, Expression.Constant(0x0FB), Expression.Constant(0x0F7));
        }

        private Expression GetUpdateLeftBorder()
        {
            return Util.Void(
                Expression.IfThen(Expression.Equal(_state.RASTER, _bottomBorderValue), Expression.Assign(_state.BORDERV, Expression.Constant(true))),
                Expression.IfThen(Expression.AndAlso(_state.DEN, Expression.Equal(_state.RASTER, _topBorderValue)), Expression.Assign(_state.BORDERV, Expression.Constant(false))),
                Expression.AndAssign(_state.BORDERM, _state.BORDERV)
                );
        }

        private Expression GetUpdateRightBorder()
        {
            return Util.Void(
                Expression.Assign(_state.BORDERM, Expression.Constant(true)) // 1
                );
        }

        private Expression GetUpdateCycle63Border()
        {
            return Util.Void(
                Expression.IfThen(Expression.Equal(_state.RASTER, _bottomBorderValue), Expression.Assign(_state.BORDERV, Expression.Constant(true))),
                Expression.IfThen(Expression.AndAlso(_state.DEN, Expression.Equal(_state.RASTER, _topBorderValue)), Expression.Assign(_state.BORDERV, Expression.Constant(false)))
                );
        }

        private Expression GetBorderUnitOutput()
        {
            return Expression.Condition(Expression.OrElse(_state.BORDERM, _state.BORDERV), _state.EC, _muxOutput);
        }

        #endregion

        #region CYCLE DECODES

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

        private Expression Cycle16 // MAC 48
        {
            get
            {
                return _updateSpriteMcBase;
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
                    Expression.IfThen(Expression.Equal(_state.RC, Expression.Constant(0x7)),
                        Util.Void(Expression.Assign(_state.IDLE, Expression.Constant(true)),
                            Expression.Assign(_state.VCBASE, _state.VC))),
                    Expression.IfThen(Expression.Not(_state.IDLE), Expression.PreIncrementAssign(_state.RC)),
                    _updateSpriteDispEnable,
                    _disableSpriteShiftReg);
            }
        }

        private Expression ResetX(Expression rasterx, Expression rasterxc)
        {
            return Expression.Assign(rasterx, Expression.Assign(rasterxc, Expression.Constant(0)));
        }

        #endregion

        #region RASTER DECODES

        private Expression Raster0
        {
            get
            {
                return Expression.Assign(_state.VCBASE, Expression.Assign(_state.RASTER, Expression.Constant(0)));
            }
        }

        #endregion

        public Expression Clock(Func<Expression, Expression> readData, Func<Expression, Expression> readColor, Expression phi1, Expression phi2, Func<Expression, Expression> clockOutput)
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
                new Tuple<Expression, Expression>(_config.MacToCounterX(48), Cycle16),
                new Tuple<Expression, Expression>(_config.MacToCounterX(0), Cycle55),
                new Tuple<Expression, Expression>(_config.MacToCounterX(2), Cycle56),
                new Tuple<Expression, Expression>(_config.MacToCounterX(6), Cycle58),
                new Tuple<Expression, Expression>(_config.RasterXToCounterX(0x018), Expression.IfThen(_state.CSEL, _updateLeftBorder)),
                new Tuple<Expression, Expression>(_config.RasterXToCounterX(0x01F), Expression.IfThen(Expression.Not(_state.CSEL), _updateLeftBorder)),
                new Tuple<Expression, Expression>(_config.RasterXToCounterX(0x14F), Expression.IfThen(Expression.Not(_state.CSEL), _updateRightBorder)),
                new Tuple<Expression, Expression>(_config.RasterXToCounterX(0x158), Expression.IfThen(_state.CSEL, _updateRightBorder)),
                new Tuple<Expression, Expression>(_config.MacToCounterX(16), _updateCycle63Border)
            };

            decodes.AddRange(Enumerable.Range(0, 43).Select(c => new Tuple<Expression, Expression>(_config.MacToCounterX(c * 2 + 41), Util.Void(_updateBadLineEnable, _updateBadLine))));
            decodes.AddRange(GetAddressGeneratorDecodes(readData, readColor));
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
                _graphicsClock,
                _spriteClock[0],
                _spriteClock[1],
                _spriteClock[2],
                _spriteClock[3],
                _spriteClock[4],
                _spriteClock[5],
                _spriteClock[6],
                _spriteClock[7]
                );

            var block = new[]
            {
                Util.Decode(decodes, Expression.PreIncrementAssign(rasterxcounter)),
                shouldHoldX != null ? Expression.IfThen(Expression.Not(shouldHoldX), notHeldX) : notHeldX,
                clockOutput(_borderUnitOutput)
            };

            return Util.Void(block);
        }
    }
}
