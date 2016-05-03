using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Extensions;

namespace Breadbox.Chips.Vic2
{
    public class Vic2Package
    {
        private readonly Vic2State _state = new Vic2State();
        private readonly ConstantExpression _width;
        private readonly ConstantExpression _lines;
        private readonly ConstantExpression _sprite0BaX;

        public Vic2Package(int width, int lines, int sprite0BaX)
        {
            _width = Expression.Constant(width);
            _lines = Expression.Constant(lines);
            _sprite0BaX = Expression.Constant(sprite0BaX);
        }

        public Expression Clock(Expression addressBus, Expression dataBus, Expression readBus)
        {
            var clock = Vic2ClockGenerator.Clock(_state.RasterX, _state.RasterY, _width, _lines, _state.BadLineEnable,
                _state.BadLine, _state.DEN, _state.YSCROLL, _state.BA, _state.AEC, _state.BaCounter, _state.FetchCounter,
                _sprite0BaX, _state.Address, _state.MobDma, _state.VM, _state.MobPointer, _state.Mc, _state.BMM, _state.ECM,
                _state.IdleState, _state.CB, _state.VC, _state.RC, _state.VideoMemory, _state.REF, _state.VMLI, _state.VCBASE);

            var borderClock = Vic2BorderUnit.Clock(_state.MainBorderFlipFlop, _state.VerticalBorderFlipFlop,
                _state.RasterX, _state.RasterY, _state.CSEL, _state.RSEL,
                Expression.Constant((int) _sprite0BaX.Value + 40), _state.DEN);

            return Expression.Block(
                clock,
                borderClock,
                Expression.Assign(_state.PixelOutput, Render)
                );
        }

        private Expression Render
        {
            get
            {
                var blanked = Vic2SyncGenerator.IsBlanked(_state.HSYNC, _state.VSYNC);
                var graphicsOutputColor = Vic2GraphicsDataSequencer.OutputColor(_state.GraphicsBuffer, _state.B0C,
                    _state.B1C, _state.B2C, _state.B3C, _state.BufferCData, _state.BMM, _state.ECM, _state.MCM);
                var graphicsOutputData = Vic2GraphicsDataSequencer.OutputData(_state.GraphicsBuffer, _state.BMM,
                    _state.MCM, _state.BufferCData);
                var spriteOutputDatas = new[]
                {
                    Vic2MobDataSequencer.OutputData(_state.MobBuffer0, _state.M0MC),
                    Vic2MobDataSequencer.OutputData(_state.MobBuffer1, _state.M1MC),
                    Vic2MobDataSequencer.OutputData(_state.MobBuffer2, _state.M2MC),
                    Vic2MobDataSequencer.OutputData(_state.MobBuffer3, _state.M3MC),
                    Vic2MobDataSequencer.OutputData(_state.MobBuffer4, _state.M4MC),
                    Vic2MobDataSequencer.OutputData(_state.MobBuffer5, _state.M5MC),
                    Vic2MobDataSequencer.OutputData(_state.MobBuffer6, _state.M6MC),
                    Vic2MobDataSequencer.OutputData(_state.MobBuffer7, _state.M7MC)
                };
                var spriteOutputColors = new[]
                {
                    Vic2MobDataSequencer.OutputColor(_state.MobBuffer0, _state.M0MC, _state.M0C, _state.MM0, _state.MM1),
                    Vic2MobDataSequencer.OutputColor(_state.MobBuffer1, _state.M1MC, _state.M1C, _state.MM0, _state.MM1),
                    Vic2MobDataSequencer.OutputColor(_state.MobBuffer2, _state.M2MC, _state.M2C, _state.MM0, _state.MM1),
                    Vic2MobDataSequencer.OutputColor(_state.MobBuffer3, _state.M3MC, _state.M3C, _state.MM0, _state.MM1),
                    Vic2MobDataSequencer.OutputColor(_state.MobBuffer4, _state.M4MC, _state.M4C, _state.MM0, _state.MM1),
                    Vic2MobDataSequencer.OutputColor(_state.MobBuffer5, _state.M5MC, _state.M5C, _state.MM0, _state.MM1),
                    Vic2MobDataSequencer.OutputColor(_state.MobBuffer6, _state.M6MC, _state.M6C, _state.MM0, _state.MM1),
                    Vic2MobDataSequencer.OutputColor(_state.MobBuffer7, _state.M7MC, _state.M7C, _state.MM0, _state.MM1),
                };

                var muxOutput = Vic2Mux.OutputColor(spriteOutputDatas, spriteOutputColors, _state.MobPriority, graphicsOutputData, graphicsOutputColor);
                var borderOutput = Vic2BorderUnit.OutputColor(_state.MainBorderFlipFlop, _state.VerticalBorderFlipFlop, muxOutput, _state.EC);

                return Expression.Condition(blanked, Expression.Constant(0), borderOutput);
            }
        }

        public Expression OutputVideo
        {
            get
            {
                return _state.PixelOutput;
            }
        }

        public Expression Address
        {
            get { return _state.Address; }
        }

        public Expression Irq
        {
            get { return _state.IRQ; }
        }

        public Expression Ba
        {
            get { return _state.BA; }
        }

        public Expression Aec
        {
            get { return _state.AEC; }
        }

        private Expression PeekMobX(Expression mx)
        {
            return Expression.And(mx, Expression.Constant(0xFF));
        }

        private Expression PokeMobX(MemberExpression mx, Expression value)
        {
            return Expression.Block(
                Expression.AndAssign(mx, Expression.Constant(0x100)),
                Expression.OrAssign(mx, value),
                Expression.Empty());
        }

        private Expression PokeMobY(MemberExpression my, Expression value)
        {
            return Expression.Block(Expression.Assign(my, value), Expression.Empty());
        }

        private Expression PeekColor(Expression color)
        {
            return Expression.Or(color, Expression.Constant(0xF0));
        }

        private Expression PeekGroup(Expression b7, Expression b6, Expression b5, Expression b4, Expression b3,
            Expression b2, Expression b1, Expression b0)
        {
            var block = new List<Expression>();

            if (b0 != null) block.Add(Expression.Condition(b0, Expression.Constant(0x01), Expression.Constant(0x00)));
            if (b1 != null) block.Add(Expression.Condition(b1, Expression.Constant(0x02), Expression.Constant(0x00)));
            if (b2 != null) block.Add(Expression.Condition(b2, Expression.Constant(0x04), Expression.Constant(0x00)));
            if (b3 != null) block.Add(Expression.Condition(b3, Expression.Constant(0x08), Expression.Constant(0x00)));
            if (b4 != null) block.Add(Expression.Condition(b4, Expression.Constant(0x10), Expression.Constant(0x00)));
            if (b5 != null) block.Add(Expression.Condition(b5, Expression.Constant(0x20), Expression.Constant(0x00)));
            if (b6 != null) block.Add(Expression.Condition(b6, Expression.Constant(0x40), Expression.Constant(0x00)));
            if (b7 != null) block.Add(Expression.Condition(b7, Expression.Constant(0x80), Expression.Constant(0x00)));

            return block.Aggregate(Expression.Or);
        }

        private Expression PeekRasterXGroup(Expression b7, Expression b6, Expression b5, Expression b4, Expression b3,
            Expression b2, Expression b1, Expression b0)
        {
            var block = new List<Expression>();

            if (b0 != null) block.Add(Expression.Condition(Expression.NotEqual(Expression.Constant(0), Expression.And(b0, Expression.Constant(0x100))), Expression.Constant(0x01), Expression.Constant(0x00)));
            if (b1 != null) block.Add(Expression.Condition(Expression.NotEqual(Expression.Constant(0), Expression.And(b1, Expression.Constant(0x100))), Expression.Constant(0x02), Expression.Constant(0x00)));
            if (b2 != null) block.Add(Expression.Condition(Expression.NotEqual(Expression.Constant(0), Expression.And(b2, Expression.Constant(0x100))), Expression.Constant(0x04), Expression.Constant(0x00)));
            if (b3 != null) block.Add(Expression.Condition(Expression.NotEqual(Expression.Constant(0), Expression.And(b3, Expression.Constant(0x100))), Expression.Constant(0x08), Expression.Constant(0x00)));
            if (b4 != null) block.Add(Expression.Condition(Expression.NotEqual(Expression.Constant(0), Expression.And(b4, Expression.Constant(0x100))), Expression.Constant(0x10), Expression.Constant(0x00)));
            if (b5 != null) block.Add(Expression.Condition(Expression.NotEqual(Expression.Constant(0), Expression.And(b5, Expression.Constant(0x100))), Expression.Constant(0x20), Expression.Constant(0x00)));
            if (b6 != null) block.Add(Expression.Condition(Expression.NotEqual(Expression.Constant(0), Expression.And(b6, Expression.Constant(0x100))), Expression.Constant(0x40), Expression.Constant(0x00)));
            if (b7 != null) block.Add(Expression.Condition(Expression.NotEqual(Expression.Constant(0), Expression.And(b7, Expression.Constant(0x100))), Expression.Constant(0x80), Expression.Constant(0x00)));

            return block.Aggregate(Expression.Or);
        }

        private Expression PeekScrollGroup(Expression b7, Expression b6, Expression b5, Expression b4, Expression b3,
            Expression bScroll)
        {
            var block = new List<Expression> { bScroll };

            if (b3 != null) block.Add(Expression.Condition(b3, Expression.Constant(0x08), Expression.Constant(0x00)));
            if (b4 != null) block.Add(Expression.Condition(b4, Expression.Constant(0x10), Expression.Constant(0x00)));
            if (b5 != null) block.Add(Expression.Condition(b5, Expression.Constant(0x20), Expression.Constant(0x00)));
            if (b6 != null) block.Add(Expression.Condition(b6, Expression.Constant(0x40), Expression.Constant(0x00)));
            if (b7 != null) block.Add(Expression.Condition(b7, Expression.Constant(0x80), Expression.Constant(0x00)));

            return block.Aggregate(Expression.Or);
        }

        private Expression PeekScrollRasterGroup(Expression bRaster, Expression b6, Expression b5, Expression b4,
            Expression b3, Expression bScroll)
        {
            var rst8 = Expression.RightShift(Expression.And(bRaster, Expression.Constant(0x100)), Expression.Constant(1));
            var block = new List<Expression> { bScroll, rst8 };

            if (b3 != null) block.Add(Expression.Condition(b3, Expression.Constant(0x08), Expression.Constant(0x00)));
            if (b4 != null) block.Add(Expression.Condition(b4, Expression.Constant(0x10), Expression.Constant(0x00)));
            if (b5 != null) block.Add(Expression.Condition(b5, Expression.Constant(0x20), Expression.Constant(0x00)));
            if (b6 != null) block.Add(Expression.Condition(b6, Expression.Constant(0x40), Expression.Constant(0x00)));

            return block.Aggregate(Expression.Or);
        }

        private Expression PokeColor(MemberExpression color, Expression value)
        {
            return Expression.Block(Expression.Assign(color, Expression.And(value, Expression.Constant(0x0F))), Expression.Empty());
        }

        private BlockExpression PokeGroup(MemberExpression b7, MemberExpression b6, MemberExpression b5, MemberExpression b4,
            MemberExpression b3, MemberExpression b2, MemberExpression b1, MemberExpression b0, Expression value)
        {
            var cachedValue = Expression.Variable(typeof(int));
            var block = new List<Expression> {Expression.Assign(cachedValue, value)};

            if (b0 != null) block.Add(Expression.Assign(b0, Expression.NotEqual(Expression.Constant(0), Expression.And(cachedValue, Expression.Constant(0x01)))));
            if (b1 != null) block.Add(Expression.Assign(b1, Expression.NotEqual(Expression.Constant(0), Expression.And(cachedValue, Expression.Constant(0x02)))));
            if (b2 != null) block.Add(Expression.Assign(b2, Expression.NotEqual(Expression.Constant(0), Expression.And(cachedValue, Expression.Constant(0x04)))));
            if (b3 != null) block.Add(Expression.Assign(b3, Expression.NotEqual(Expression.Constant(0), Expression.And(cachedValue, Expression.Constant(0x08)))));
            if (b4 != null) block.Add(Expression.Assign(b4, Expression.NotEqual(Expression.Constant(0), Expression.And(cachedValue, Expression.Constant(0x10)))));
            if (b5 != null) block.Add(Expression.Assign(b5, Expression.NotEqual(Expression.Constant(0), Expression.And(cachedValue, Expression.Constant(0x20)))));
            if (b6 != null) block.Add(Expression.Assign(b6, Expression.NotEqual(Expression.Constant(0), Expression.And(cachedValue, Expression.Constant(0x40)))));
            if (b7 != null) block.Add(Expression.Assign(b7, Expression.NotEqual(Expression.Constant(0), Expression.And(cachedValue, Expression.Constant(0x80)))));

            block.Add(Expression.Empty());

            return Expression.Block(new[] {cachedValue}, block);
        }

        private BlockExpression PokeRasterXGroup(MemberExpression b7, MemberExpression b6, MemberExpression b5,
            MemberExpression b4,
            MemberExpression b3, MemberExpression b2, MemberExpression b1, MemberExpression b0, Expression value)
        {
            var cachedValue = Expression.Variable(typeof(int));
            var block = new List<Expression> { Expression.Assign(cachedValue, value) };

            if (b0 != null) block.Add(Expression.Assign(b0, Expression.Or(Expression.And(b0, Expression.Constant(0xFF)), Expression.LeftShift(Expression.And(value, Expression.Constant(0x01)), Expression.Constant(8)))));
            if (b1 != null) block.Add(Expression.Assign(b1, Expression.Or(Expression.And(b1, Expression.Constant(0xFF)), Expression.LeftShift(Expression.And(value, Expression.Constant(0x02)), Expression.Constant(7)))));
            if (b2 != null) block.Add(Expression.Assign(b2, Expression.Or(Expression.And(b2, Expression.Constant(0xFF)), Expression.LeftShift(Expression.And(value, Expression.Constant(0x04)), Expression.Constant(6)))));
            if (b3 != null) block.Add(Expression.Assign(b3, Expression.Or(Expression.And(b3, Expression.Constant(0xFF)), Expression.LeftShift(Expression.And(value, Expression.Constant(0x08)), Expression.Constant(5)))));
            if (b4 != null) block.Add(Expression.Assign(b4, Expression.Or(Expression.And(b4, Expression.Constant(0xFF)), Expression.LeftShift(Expression.And(value, Expression.Constant(0x10)), Expression.Constant(4)))));
            if (b5 != null) block.Add(Expression.Assign(b5, Expression.Or(Expression.And(b5, Expression.Constant(0xFF)), Expression.LeftShift(Expression.And(value, Expression.Constant(0x20)), Expression.Constant(3)))));
            if (b6 != null) block.Add(Expression.Assign(b6, Expression.Or(Expression.And(b6, Expression.Constant(0xFF)), Expression.LeftShift(Expression.And(value, Expression.Constant(0x40)), Expression.Constant(2)))));
            if (b7 != null) block.Add(Expression.Assign(b7, Expression.Or(Expression.And(b7, Expression.Constant(0xFF)), Expression.LeftShift(Expression.And(value, Expression.Constant(0x80)), Expression.Constant(1)))));

            block.Add(Expression.Empty());

            return Expression.Block(new[] { cachedValue }, block);
        }

        private BlockExpression PokeScrollGroup(MemberExpression b7, MemberExpression b6, MemberExpression b5, MemberExpression b4,
            MemberExpression b3, MemberExpression bScroll, Expression value)
        {
            var cachedValue = Expression.Variable(typeof(int));
            var block = new List<Expression> { Expression.Assign(cachedValue, value), Expression.Assign(bScroll, Expression.And(cachedValue, Expression.Constant(0x7))) };

            if (b3 != null) block.Add(Expression.Assign(b3, Expression.NotEqual(Expression.Constant(0), Expression.And(cachedValue, Expression.Constant(0x08)))));
            if (b4 != null) block.Add(Expression.Assign(b4, Expression.NotEqual(Expression.Constant(0), Expression.And(cachedValue, Expression.Constant(0x10)))));
            if (b5 != null) block.Add(Expression.Assign(b5, Expression.NotEqual(Expression.Constant(0), Expression.And(cachedValue, Expression.Constant(0x20)))));
            if (b6 != null) block.Add(Expression.Assign(b6, Expression.NotEqual(Expression.Constant(0), Expression.And(cachedValue, Expression.Constant(0x40)))));
            if (b7 != null) block.Add(Expression.Assign(b7, Expression.NotEqual(Expression.Constant(0), Expression.And(cachedValue, Expression.Constant(0x80)))));

            block.Add(Expression.Empty());

            return Expression.Block(new[] { cachedValue }, block);
        }

        private BlockExpression PokeScrollRasterGroup(MemberExpression bRaster, MemberExpression b6, MemberExpression b5, MemberExpression b4,
            MemberExpression b3, MemberExpression bScroll, Expression value)
        {
            var cachedValue = Expression.Variable(typeof(int));
            var block = new List<Expression>
            {
                Expression.Assign(cachedValue, value),
                Expression.Assign(bScroll, Expression.And(cachedValue, Expression.Constant(0x7))),
                Expression.IfThenElse(Expression.NotEqual(Expression.And(cachedValue, Expression.Constant(0x80)), Expression.Constant(0x00)), Expression.OrAssign(bRaster, Expression.Constant(0x100)), Expression.AndAssign(bRaster, Expression.Constant(0x1FF))),
            };

            if (b3 != null) block.Add(Expression.Assign(b3, Expression.NotEqual(Expression.Constant(0), Expression.And(cachedValue, Expression.Constant(0x08)))));
            if (b4 != null) block.Add(Expression.Assign(b4, Expression.NotEqual(Expression.Constant(0), Expression.And(cachedValue, Expression.Constant(0x10)))));
            if (b5 != null) block.Add(Expression.Assign(b5, Expression.NotEqual(Expression.Constant(0), Expression.And(cachedValue, Expression.Constant(0x20)))));
            if (b6 != null) block.Add(Expression.Assign(b6, Expression.NotEqual(Expression.Constant(0), Expression.And(cachedValue, Expression.Constant(0x40)))));

            block.Add(Expression.Empty());

            return Expression.Block(new[] { cachedValue }, block);
        }

        private Expression PeekRegister00 { get { return PeekMobX(_state.M0X); } }
        private Expression PeekRegister02 { get { return PeekMobX(_state.M1X); } }
        private Expression PeekRegister04 { get { return PeekMobX(_state.M2X); } }
        private Expression PeekRegister06 { get { return PeekMobX(_state.M3X); } }
        private Expression PeekRegister08 { get { return PeekMobX(_state.M4X); } }
        private Expression PeekRegister0A { get { return PeekMobX(_state.M5X); } }
        private Expression PeekRegister0C { get { return PeekMobX(_state.M6X); } }
        private Expression PeekRegister0E { get { return PeekMobX(_state.M7X); } }

        private Expression PokeRegister00(Expression value) { return PokeMobX(_state.M0X, value); }
        private Expression PokeRegister02(Expression value) { return PokeMobX(_state.M1X, value); }
        private Expression PokeRegister04(Expression value) { return PokeMobX(_state.M2X, value); }
        private Expression PokeRegister06(Expression value) { return PokeMobX(_state.M3X, value); }
        private Expression PokeRegister08(Expression value) { return PokeMobX(_state.M4X, value); }
        private Expression PokeRegister0A(Expression value) { return PokeMobX(_state.M5X, value); }
        private Expression PokeRegister0C(Expression value) { return PokeMobX(_state.M6X, value); }
        private Expression PokeRegister0E(Expression value) { return PokeMobX(_state.M7X, value); }

        private Expression PeekRegister01 { get { return _state.M0Y; } }
        private Expression PeekRegister03 { get { return _state.M1Y; } }
        private Expression PeekRegister05 { get { return _state.M2Y; } }
        private Expression PeekRegister07 { get { return _state.M3Y; } }
        private Expression PeekRegister09 { get { return _state.M4Y; } }
        private Expression PeekRegister0B { get { return _state.M5Y; } }
        private Expression PeekRegister0D { get { return _state.M6Y; } }
        private Expression PeekRegister0F { get { return _state.M7Y; } }

        private Expression PokeRegister01(Expression value) { return PokeMobY(_state.M0Y, value); }
        private Expression PokeRegister03(Expression value) { return PokeMobY(_state.M1Y, value); }
        private Expression PokeRegister05(Expression value) { return PokeMobY(_state.M2Y, value); }
        private Expression PokeRegister07(Expression value) { return PokeMobY(_state.M3Y, value); }
        private Expression PokeRegister09(Expression value) { return PokeMobY(_state.M4Y, value); }
        private Expression PokeRegister0B(Expression value) { return PokeMobY(_state.M5Y, value); }
        private Expression PokeRegister0D(Expression value) { return PokeMobY(_state.M6Y, value); }
        private Expression PokeRegister0F(Expression value) { return PokeMobY(_state.M7Y, value); }

        private Expression PeekRegister10
        {
            get
            {
                return PeekRasterXGroup(_state.M7X, _state.M6X, _state.M5X, _state.M4X, _state.M3X, _state.M2X, _state.M1X, _state.M0X);
            }
        }

        private Expression PokeRegister10(Expression value)
        {
            return PokeRasterXGroup(_state.M7X, _state.M6X, _state.M5X, _state.M4X, _state.M3X, _state.M2X, _state.M1X, _state.M0X, value);
        }

        private Expression PeekRegister11
        {
            get
            {
                return PeekScrollRasterGroup(_state.RasterY, _state.ECM, _state.BMM, _state.DEN, _state.RSEL, _state.YSCROLL);
            }
        }

        private Expression PokeRegister11(Expression value)
        {
            return PokeScrollRasterGroup(_state.RasterY, _state.ECM, _state.BMM, _state.DEN, _state.RSEL, _state.YSCROLL, value);
        }

        private Expression PeekRegister12
        {
            get { return Expression.And(_state.RasterY, Expression.Constant(0xFF)); }
        }

        private Expression PokeRegister12(Expression value)
        {
            return Expression.Block(Expression.Assign(_state.RasterY, Expression.Or(Expression.And(_state.RasterY, Expression.Constant(0x100)), value)), Expression.Empty());
        }

        private Expression PeekRegister13
        {
            get { return _state.LPX; }
        }

        private Expression PokeRegister13(Expression value)
        {
            return Expression.Block(Expression.Assign(_state.LPX, value), Expression.Empty());
        }

        private Expression PeekRegister14
        {
            get { return _state.LPY; }
        }

        private Expression PokeRegister14(Expression value)
        {
            return Expression.Block(Expression.Assign(_state.LPY, value), Expression.Empty());
        }

        private Expression PeekRegister15
        {
            get
            {
                return PeekGroup(_state.M7E, _state.M6E, _state.M5E, _state.M4E, _state.M3E, _state.M2E, _state.M1E, _state.M0E);
            }
        }

        private Expression PokeRegister15(Expression value)
        {
            return PokeGroup(_state.M7E, _state.M6E, _state.M5E, _state.M4E, _state.M3E, _state.M2E, _state.M1E, _state.M0E, value);
        }

        private Expression PeekRegister16
        {
            get
            {
                return Expression.Or(PeekScrollGroup(null, null, _state.RES, _state.MCM, _state.CSEL, _state.XSCROLL), Expression.Constant(0xC0));
            }
        }

        private Expression PokeRegister16(Expression value)
        {
            return PokeScrollGroup(null, null, _state.RES, _state.MCM, _state.CSEL, _state.XSCROLL, value);
        }

        private Expression PeekRegister17
        {
            get
            {
                return PeekGroup(_state.M7YE, _state.M6YE, _state.M5YE, _state.M4YE, _state.M3YE, _state.M2YE, _state.M1YE, _state.M0YE);
            }
        }

        private Expression PokeRegister17(Expression value)
        {
            return PokeGroup(_state.M7YE, _state.M6YE, _state.M5YE, _state.M4YE, _state.M3YE, _state.M2YE, _state.M1YE, _state.M0YE, value);
        }

        private Expression PeekRegister18
        {
            get
            {
                var disconnectedBits = Expression.Constant(0x01);
                var vm = Expression.LeftShift(_state.VM, Expression.Constant(4));
                var cb = Expression.LeftShift(_state.CB, Expression.Constant(1));

                return new Expression[] {disconnectedBits, vm, cb}.Aggregate(Expression.Or);
            }
        }

        private Expression PokeRegister18(Expression value)
        {
            var cachedValue = Expression.Parameter(typeof(int));
            return Expression.Block(new[] { cachedValue },
                Expression.Assign(cachedValue, value),
                Expression.Assign(_state.VM, Expression.RightShift(Expression.And(value, Expression.Constant(0xF0)), Expression.Constant(4))),
                Expression.Assign(_state.CB, Expression.RightShift(Expression.And(value, Expression.Constant(0x0E)), Expression.Constant(1))),
                Expression.Empty()
                );
        }

        private Expression PeekRegister19
        {
            get
            {
                var disconnectedBits = Expression.Constant(0x70);
                return Expression.Or(PeekGroup(_state.IRQ, null, null, null, _state.ILP, _state.IMMC, _state.IMBC, _state.IRST), disconnectedBits);
            }
        }

        private Expression PokeRegister19(Expression value)
        {
            return PokeGroup(_state.IRQ, null, null, null, _state.ILP, _state.IMMC, _state.IMBC, _state.IRST, value);
        }

        private Expression PeekRegister1A
        {
            get
            {
                var disconnectedBits = Expression.Constant(0xF0);
                return Expression.Or(PeekGroup(null, null, null, null, _state.ELP, _state.EMMC, _state.EMBC, _state.ERST), disconnectedBits);
            }
        }

        private Expression PokeRegister1A(Expression value)
        {
            return PokeGroup(null, null, null, null, _state.ELP, _state.EMMC, _state.EMBC, _state.ERST, value);
        }

        private Expression PeekRegister1B
        {
            get
            {
                return PeekGroup(_state.M7DP, _state.M6DP, _state.M5DP, _state.M4DP, _state.M3DP, _state.M2DP, _state.M1DP, _state.M0DP);
            }
        }

        private Expression PokeRegister1B(Expression value)
        {
            return PokeGroup(_state.M7DP, _state.M6DP, _state.M5DP, _state.M4DP, _state.M3DP, _state.M2DP, _state.M1DP, _state.M0DP, value);
        }

        private Expression PeekRegister1C
        {
            get
            {
                return PeekGroup(_state.M7MC, _state.M6MC, _state.M5MC, _state.M4MC, _state.M3MC, _state.M2MC, _state.M1MC, _state.M0MC);
            }
        }

        private Expression PokeRegister1C(Expression value)
        {
            return PokeGroup(_state.M7MC, _state.M6MC, _state.M5MC, _state.M4MC, _state.M3MC, _state.M2MC, _state.M1MC, _state.M0MC, value);
        }

        private Expression PeekRegister1D
        {
            get
            {
                return PeekGroup(_state.M7XE, _state.M6XE, _state.M5XE, _state.M4XE, _state.M3XE, _state.M2XE, _state.M1XE, _state.M0XE);
            }
        }

        private Expression PokeRegister1D(Expression value)
        {
            return PokeGroup(_state.M7XE, _state.M6XE, _state.M5XE, _state.M4XE, _state.M3XE, _state.M2XE, _state.M1XE, _state.M0XE, value);
        }

        private Expression PeekRegister1E
        {
            get
            {
                return PeekGroup(_state.M7M, _state.M6M, _state.M5M, _state.M4M, _state.M3M, _state.M2M, _state.M1M, _state.M0M);
            }
        }

        private Expression PokeRegister1E(Expression value)
        {
            return PokeGroup(_state.M7M, _state.M6M, _state.M5M, _state.M4M, _state.M3M, _state.M2M, _state.M1M, _state.M0M, value);
        }

        private Expression PeekRegister1F
        {
            get
            {
                return PeekGroup(_state.M7D, _state.M6D, _state.M5D, _state.M4D, _state.M3D, _state.M2D, _state.M1D, _state.M0D);
            }
        }

        private Expression PokeRegister1F(Expression value)
        {
            return PokeGroup(_state.M7D, _state.M6D, _state.M5D, _state.M4D, _state.M3D, _state.M2D, _state.M1D, _state.M0D, value);
        }

        private Expression PeekRegister20
        {
            get { return PeekColor(_state.EC); }
        }

        private Expression PokeRegister20(Expression value)
        {
            return PokeColor(_state.EC, value);
        }

        private Expression PeekRegister21
        {
            get { return PeekColor(_state.B0C); }
        }

        private Expression PokeRegister21(Expression value)
        {
            return PokeColor(_state.B0C, value);
        }

        private Expression PeekRegister22
        {
            get { return PeekColor(_state.B1C); }
        }

        private Expression PokeRegister22(Expression value)
        {
            return PokeColor(_state.B1C, value);
        }

        private Expression PeekRegister23
        {
            get { return PeekColor(_state.B2C); }
        }

        private Expression PokeRegister23(Expression value)
        {
            return PokeColor(_state.B2C, value);
        }

        private Expression PeekRegister24
        {
            get { return PeekColor(_state.B3C); }
        }

        private Expression PokeRegister24(Expression value)
        {
            return PokeColor(_state.B3C, value);
        }

        private Expression PeekRegister25
        {
            get { return PeekColor(_state.MM0); }
        }

        private Expression PokeRegister25(Expression value)
        {
            return PokeColor(_state.MM0, value);
        }

        private Expression PeekRegister26
        {
            get { return PeekColor(_state.MM1); }
        }

        private Expression PokeRegister26(Expression value)
        {
            return PokeColor(_state.MM1, value);
        }

        private Expression PeekRegister27
        {
            get { return PeekColor(_state.M0C); }
        }

        private Expression PokeRegister27(Expression value)
        {
            return PokeColor(_state.M0C, value);
        }

        private Expression PeekRegister28
        {
            get { return PeekColor(_state.M1C); }
        }

        private Expression PokeRegister28(Expression value)
        {
            return PokeColor(_state.M1C, value);
        }

        private Expression PeekRegister29
        {
            get { return PeekColor(_state.M2C); }
        }

        private Expression PokeRegister29(Expression value)
        {
            return PokeColor(_state.M2C, value);
        }

        private Expression PeekRegister2A
        {
            get { return PeekColor(_state.M3C); }
        }

        private Expression PokeRegister2A(Expression value)
        {
            return PokeColor(_state.M3C, value);
        }

        private Expression PeekRegister2B
        {
            get { return PeekColor(_state.M4C); }
        }

        private Expression PokeRegister2B(Expression value)
        {
            return PokeColor(_state.M4C, value);
        }

        private Expression PeekRegister2C
        {
            get { return PeekColor(_state.M5C); }
        }

        private Expression PokeRegister2C(Expression value)
        {
            return PokeColor(_state.M5C, value);
        }

        private Expression PeekRegister2D
        {
            get { return PeekColor(_state.M6C); }
        }

        private Expression PokeRegister2D(Expression value)
        {
            return PokeColor(_state.M6C, value);
        }

        private Expression PeekRegister2E
        {
            get { return PeekColor(_state.M7C); }
        }

        private Expression PokeRegister2E(Expression value)
        {
            return PokeColor(_state.M7C, value);
        }

        public Expression PeekRegister(Expression address)
        {
            return Expression.Switch(Expression.And(address, Expression.Constant(0x3F)), Expression.Constant(0xFF),
                Expression.SwitchCase(PeekRegister00, Expression.Constant(0x00)),
                Expression.SwitchCase(PeekRegister01, Expression.Constant(0x01)),
                Expression.SwitchCase(PeekRegister02, Expression.Constant(0x02)),
                Expression.SwitchCase(PeekRegister03, Expression.Constant(0x03)),
                Expression.SwitchCase(PeekRegister04, Expression.Constant(0x04)),
                Expression.SwitchCase(PeekRegister05, Expression.Constant(0x05)),
                Expression.SwitchCase(PeekRegister06, Expression.Constant(0x06)),
                Expression.SwitchCase(PeekRegister07, Expression.Constant(0x07)),
                Expression.SwitchCase(PeekRegister08, Expression.Constant(0x08)),
                Expression.SwitchCase(PeekRegister09, Expression.Constant(0x09)),
                Expression.SwitchCase(PeekRegister0A, Expression.Constant(0x0A)),
                Expression.SwitchCase(PeekRegister0B, Expression.Constant(0x0B)),
                Expression.SwitchCase(PeekRegister0C, Expression.Constant(0x0C)),
                Expression.SwitchCase(PeekRegister0D, Expression.Constant(0x0D)),
                Expression.SwitchCase(PeekRegister0E, Expression.Constant(0x0E)),
                Expression.SwitchCase(PeekRegister0F, Expression.Constant(0x0F)),
                Expression.SwitchCase(PeekRegister10, Expression.Constant(0x10)),
                Expression.SwitchCase(PeekRegister11, Expression.Constant(0x11)),
                Expression.SwitchCase(PeekRegister12, Expression.Constant(0x12)),
                Expression.SwitchCase(PeekRegister13, Expression.Constant(0x13)),
                Expression.SwitchCase(PeekRegister14, Expression.Constant(0x14)),
                Expression.SwitchCase(PeekRegister15, Expression.Constant(0x15)),
                Expression.SwitchCase(PeekRegister16, Expression.Constant(0x16)),
                Expression.SwitchCase(PeekRegister17, Expression.Constant(0x17)),
                Expression.SwitchCase(PeekRegister18, Expression.Constant(0x18)),
                Expression.SwitchCase(PeekRegister19, Expression.Constant(0x19)),
                Expression.SwitchCase(PeekRegister1A, Expression.Constant(0x1A)),
                Expression.SwitchCase(PeekRegister1B, Expression.Constant(0x1B)),
                Expression.SwitchCase(PeekRegister1C, Expression.Constant(0x1C)),
                Expression.SwitchCase(PeekRegister1D, Expression.Constant(0x1D)),
                Expression.SwitchCase(PeekRegister1E, Expression.Constant(0x1E)),
                Expression.SwitchCase(PeekRegister1F, Expression.Constant(0x1F)),
                Expression.SwitchCase(PeekRegister20, Expression.Constant(0x20)),
                Expression.SwitchCase(PeekRegister21, Expression.Constant(0x21)),
                Expression.SwitchCase(PeekRegister22, Expression.Constant(0x22)),
                Expression.SwitchCase(PeekRegister23, Expression.Constant(0x23)),
                Expression.SwitchCase(PeekRegister24, Expression.Constant(0x24)),
                Expression.SwitchCase(PeekRegister25, Expression.Constant(0x25)),
                Expression.SwitchCase(PeekRegister26, Expression.Constant(0x26)),
                Expression.SwitchCase(PeekRegister27, Expression.Constant(0x27)),
                Expression.SwitchCase(PeekRegister28, Expression.Constant(0x28)),
                Expression.SwitchCase(PeekRegister29, Expression.Constant(0x29)),
                Expression.SwitchCase(PeekRegister2A, Expression.Constant(0x2A)),
                Expression.SwitchCase(PeekRegister2B, Expression.Constant(0x2B)),
                Expression.SwitchCase(PeekRegister2C, Expression.Constant(0x2C)),
                Expression.SwitchCase(PeekRegister2D, Expression.Constant(0x2D)),
                Expression.SwitchCase(PeekRegister2E, Expression.Constant(0x2E))
                );
        }

        public Expression PokeRegister(Expression address, Expression value)
        {
            return Expression.Switch(Expression.And(address, Expression.Constant(0x3F)),
                Expression.SwitchCase(PokeRegister00(value), Expression.Constant(0x00)),
                Expression.SwitchCase(PokeRegister01(value), Expression.Constant(0x01)),
                Expression.SwitchCase(PokeRegister02(value), Expression.Constant(0x02)),
                Expression.SwitchCase(PokeRegister03(value), Expression.Constant(0x03)),
                Expression.SwitchCase(PokeRegister04(value), Expression.Constant(0x04)),
                Expression.SwitchCase(PokeRegister05(value), Expression.Constant(0x05)),
                Expression.SwitchCase(PokeRegister06(value), Expression.Constant(0x06)),
                Expression.SwitchCase(PokeRegister07(value), Expression.Constant(0x07)),
                Expression.SwitchCase(PokeRegister08(value), Expression.Constant(0x08)),
                Expression.SwitchCase(PokeRegister09(value), Expression.Constant(0x09)),
                Expression.SwitchCase(PokeRegister0A(value), Expression.Constant(0x0A)),
                Expression.SwitchCase(PokeRegister0B(value), Expression.Constant(0x0B)),
                Expression.SwitchCase(PokeRegister0C(value), Expression.Constant(0x0C)),
                Expression.SwitchCase(PokeRegister0D(value), Expression.Constant(0x0D)),
                Expression.SwitchCase(PokeRegister0E(value), Expression.Constant(0x0E)),
                Expression.SwitchCase(PokeRegister0F(value), Expression.Constant(0x0F)),
                Expression.SwitchCase(PokeRegister10(value), Expression.Constant(0x10)),
                Expression.SwitchCase(PokeRegister11(value), Expression.Constant(0x11)),
                Expression.SwitchCase(PokeRegister12(value), Expression.Constant(0x12)),
                Expression.SwitchCase(PokeRegister13(value), Expression.Constant(0x13)),
                Expression.SwitchCase(PokeRegister14(value), Expression.Constant(0x14)),
                Expression.SwitchCase(PokeRegister15(value), Expression.Constant(0x15)),
                Expression.SwitchCase(PokeRegister16(value), Expression.Constant(0x16)),
                Expression.SwitchCase(PokeRegister17(value), Expression.Constant(0x17)),
                Expression.SwitchCase(PokeRegister18(value), Expression.Constant(0x18)),
                Expression.SwitchCase(PokeRegister19(value), Expression.Constant(0x19)),
                Expression.SwitchCase(PokeRegister1A(value), Expression.Constant(0x1A)),
                Expression.SwitchCase(PokeRegister1B(value), Expression.Constant(0x1B)),
                Expression.SwitchCase(PokeRegister1C(value), Expression.Constant(0x1C)),
                Expression.SwitchCase(PokeRegister1D(value), Expression.Constant(0x1D)),
                Expression.SwitchCase(PokeRegister1E(value), Expression.Constant(0x1E)),
                Expression.SwitchCase(PokeRegister1F(value), Expression.Constant(0x1F)),
                Expression.SwitchCase(PokeRegister20(value), Expression.Constant(0x20)),
                Expression.SwitchCase(PokeRegister21(value), Expression.Constant(0x21)),
                Expression.SwitchCase(PokeRegister22(value), Expression.Constant(0x22)),
                Expression.SwitchCase(PokeRegister23(value), Expression.Constant(0x23)),
                Expression.SwitchCase(PokeRegister24(value), Expression.Constant(0x24)),
                Expression.SwitchCase(PokeRegister25(value), Expression.Constant(0x25)),
                Expression.SwitchCase(PokeRegister26(value), Expression.Constant(0x26)),
                Expression.SwitchCase(PokeRegister27(value), Expression.Constant(0x27)),
                Expression.SwitchCase(PokeRegister28(value), Expression.Constant(0x28)),
                Expression.SwitchCase(PokeRegister29(value), Expression.Constant(0x29)),
                Expression.SwitchCase(PokeRegister2A(value), Expression.Constant(0x2A)),
                Expression.SwitchCase(PokeRegister2B(value), Expression.Constant(0x2B)),
                Expression.SwitchCase(PokeRegister2C(value), Expression.Constant(0x2C)),
                Expression.SwitchCase(PokeRegister2D(value), Expression.Constant(0x2D)),
                Expression.SwitchCase(PokeRegister2E(value), Expression.Constant(0x2E))
                );
        }
    }
}
