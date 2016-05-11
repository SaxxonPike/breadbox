using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Breadbox.Packages.Vic2
{
    public class AddressGenerator
    {
        // 000 - idle
        // 001 - no access
        // 002 - idle
        // 003 - no access
        // 004 - idle
        // 005 - no access
        // 006 - pointer 0
        // 007 - sprite 0:0
        // 008 - sprite 0:1
        // 009 - sprite 0:2
        // 010 - pointer 1
        // 011 - sprite 1:0
        // 012 - sprite 1:1
        // 013 - sprite 1:2
        // 014 - pointer 2
        // 015 - sprite 2:0
        // 016 - sprite 2:1
        // 017 - sprite 2:2
        // 018 - pointer 3
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
        // 040 - refresh
        // 041 - no access
        // 042 - refresh
        // 043 - no access
        // 044 - refresh
        // 045 - no access
        // 046 - refresh
        // 047 - c:00
        // 048 - g:00
        // ...

        private readonly State _state;
        private readonly Config _config;

        public AddressGenerator(State state, Config config)
        {
            _state = state;
            _config = config;
        }

        public Expression UpdateBadlineEnable
        {
            get
            {
                return Util.Simplify(Expression.Switch(_state.RASTER,
                    Expression.SwitchCase(Util.Void(Expression.Assign(_state.BADLINEENABLE, Expression.OrElse(_state.BADLINE, _state.DEN))), Expression.Constant(0x030)),
                    Expression.SwitchCase(Util.Void(Expression.Assign(_state.BADLINE, Expression.Assign(_state.BADLINEENABLE, Expression.Constant(false)))), Expression.Constant(0x0F8))));
            }
        }

        public Expression UpdateBadline
        {
            get
            {
                var rastery = _state.RASTER;
                return Expression.IfThen(Util.And(_state.BADLINEENABLE,
                    Expression.GreaterThanOrEqual(rastery, Expression.Constant(0x30)),
                    Expression.LessThanOrEqual(rastery, Expression.Constant(0xF7)),
                    Expression.Equal(Expression.And(Expression.Constant(0x7), rastery), _state.YSCROLL)),
                    Expression.Assign(_state.BADLINE, Expression.Constant(true)));
            }
        }

        public Expression Clock(Func<Expression, Expression> readData, Func<Expression, Expression> readColorMemory)
        {
            var mac = _state.MAC;
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

            var macCycles = _config.ClocksPerRasterValue/4;

            // MAC

            var incrementMac = Expression.PreIncrementAssign(mac);
            var noAccess = Expression.Constant(0x3FFF);

            // C accesses

            var colorAccess = Expression.Or(Expression.LeftShift(vm, Expression.Constant(10)), vc);
            var colorAccessTests = Enumerable.Range(0, 40).Select(i => Expression.Constant(((i * 2) + 47) % macCycles));
            var colorAssign = Util.Void(_state.Dn.AssignUsing(vmli, Expression.Assign(_state.CBUFFER, Expression.Or(readData(colorAccess), Expression.LeftShift(readColorMemory(colorAccess), Expression.Constant(8))))));
            var colorClear = Util.Void(Expression.Assign(_state.CBUFFER, Expression.Constant(0)));
            var colorClearTest = Expression.Constant(((41*2) + 47)%macCycles);
            var colorAssignOnBadline = Expression.IfThenElse(_state.BADLINE, colorAssign, Util.Void(readData(noAccess)));

            // G accesses

            var idleAccess = Expression.Condition(ecm, Expression.Constant(0x39FF), Expression.Constant(0x3FFF));
            var characterPointer = Expression.And(_state.Dn.SelectUsing(vmli), Expression.Constant(0xFF));
            var characterAccess = Util.Or(Expression.LeftShift(cb, Expression.Constant(11)),
                Expression.LeftShift(characterPointer, Expression.Constant(3)), rc);
            var bitmapAccess =
                Util.Or(Expression.LeftShift(Expression.And(cb, Expression.Constant(0x4)), Expression.Constant(13)),
                    Expression.LeftShift(vc, Expression.Constant(3)), rc);
            var nonIdleGraphicsAccess = Expression.And(idleAccess, Expression.Condition(bmm, bitmapAccess, characterAccess));
            var graphicsAccess = Expression.Condition(idle, idleAccess, nonIdleGraphicsAccess);
            var graphicsAccessTests = Enumerable.Range(0, 40).Select(i => Expression.Constant(((i*2) + 48) % macCycles));
            var graphicsAssign = Util.Void(Expression.Assign(gd, readData(graphicsAccess)), Expression.PreIncrementAssign(vmli), Expression.PreIncrementAssign(vc));

            // R accesses

            var refreshAccess = Expression.Or(Expression.Constant(0x3F00), refresh);
            var refreshAssign = Util.Void(Expression.Assign(refresh, Expression.And(Expression.Decrement(refresh), Expression.Constant(0xFF))), readData(refreshAccess));
            var refreshTests = Enumerable.Range(0, 5).Select(i => Expression.Constant(38 + (i*2)));

            // S and P accesses

            Func<int, Expression> spritePointerAccess = spriteNumber => Util.Or(Expression.Constant(spriteNumber | 0x1F8), Expression.LeftShift(vm, Expression.Constant(10)));
            Func<int, Expression> spriteDataAccess = spriteNumber => Util.Or(Expression.LeftShift(mp[spriteNumber], Expression.Constant(6)), mc[spriteNumber]);
            Func<int, Expression> spritePointerAccessTest = spriteNumber => Expression.Constant((spriteNumber * 4) + 6);
            Func<int, Expression[]> spriteDataAccessTests = spriteNumber => Enumerable.Range((spriteNumber * 4) + 7, 3).Select(i => (Expression)Expression.Constant(i)).ToArray();

            Func<int, Expression> spritePointerAssign = spriteNumber => Util.Void(Expression.Assign(mp[spriteNumber], spritePointerAccess(spriteNumber)));
            Func<int, Expression> spriteDataAssign = spriteNumber => Expression.IfThenElse(_state.MnE[spriteNumber],
                Util.Void(Expression.LeftShiftAssign(md[spriteNumber], Expression.Constant(8)), Expression.OrAssign(md[spriteNumber], readData(spriteDataAccess(spriteNumber)))),
                Util.Void(readData(noAccess)));

            // all together now

            return Util.Void(
                incrementMac,
                Util.Simplify(
                Expression.Switch(mac,
                    Util.Void(readData(Expression.Condition(Expression.Equal(Expression.And(mac, Expression.Constant(1)), Expression.Constant(0)), idleAccess, noAccess))),
                    Expression.SwitchCase(spritePointerAssign(0), spritePointerAccessTest(0)),
                    Expression.SwitchCase(spritePointerAssign(1), spritePointerAccessTest(1)),
                    Expression.SwitchCase(spritePointerAssign(2), spritePointerAccessTest(2)),
                    Expression.SwitchCase(spritePointerAssign(3), spritePointerAccessTest(3)),
                    Expression.SwitchCase(spritePointerAssign(4), spritePointerAccessTest(4)),
                    Expression.SwitchCase(spritePointerAssign(5), spritePointerAccessTest(5)),
                    Expression.SwitchCase(spritePointerAssign(6), spritePointerAccessTest(6)),
                    Expression.SwitchCase(spritePointerAssign(7), spritePointerAccessTest(7)),
                    Expression.SwitchCase(spriteDataAssign(0), spriteDataAccessTests(0)),
                    Expression.SwitchCase(spriteDataAssign(1), spriteDataAccessTests(1)),
                    Expression.SwitchCase(spriteDataAssign(2), spriteDataAccessTests(2)),
                    Expression.SwitchCase(spriteDataAssign(3), spriteDataAccessTests(3)),
                    Expression.SwitchCase(spriteDataAssign(4), spriteDataAccessTests(4)),
                    Expression.SwitchCase(spriteDataAssign(5), spriteDataAccessTests(5)),
                    Expression.SwitchCase(spriteDataAssign(6), spriteDataAccessTests(6)),
                    Expression.SwitchCase(spriteDataAssign(7), spriteDataAccessTests(7)),
                    Expression.SwitchCase(graphicsAssign, graphicsAccessTests),
                    Expression.SwitchCase(colorAssignOnBadline, colorAccessTests),
                    Expression.SwitchCase(colorClear, colorClearTest),
                    Expression.SwitchCase(refreshAssign, refreshTests)
                )));
        }
    }
}
