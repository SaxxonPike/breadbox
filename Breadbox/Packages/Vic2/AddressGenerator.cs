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

        public IEnumerable<Tuple<Expression, Expression>> GetDecodes(Func<Expression, Expression> readData, Func<Expression, Expression> readColorMemory)
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
            var noRead = readData(noAccess);

            // C accesses

            var colorAccess = Expression.Or(Expression.LeftShift(vm, Expression.Constant(10)), vc);
            var colorAssign = Util.Void(_state.Dn.AssignUsing(vmli, Expression.Assign(_state.CBUFFER, Expression.Or(readData(colorAccess), Expression.LeftShift(readColorMemory(colorAccess), Expression.Constant(8))))));
            var colorClear = Util.Void(Expression.Assign(_state.CBUFFER, Expression.Constant(0)));
            var colorAssignOnBadline = Expression.IfThenElse(_state.BADLINE, colorAssign, Util.Void(readData(noAccess)));

            // G accesses

            var idleAccess = Expression.Condition(ecm, Expression.Constant(0x39FF), Expression.Constant(0x3FFF));
            var idleRead = readData(idleAccess);
            var characterPointer = Expression.And(_state.Dn.SelectUsing(vmli), Expression.Constant(0xFF));
            var characterAccess = Util.Or(Expression.LeftShift(cb, Expression.Constant(11)),
                Expression.LeftShift(characterPointer, Expression.Constant(3)), rc);
            var bitmapAccess =
                Util.Or(Expression.LeftShift(Expression.And(cb, Expression.Constant(0x4)), Expression.Constant(13)),
                    Expression.LeftShift(vc, Expression.Constant(3)), rc);
            var nonIdleGraphicsAccess = Expression.And(idleAccess, Expression.Condition(bmm, bitmapAccess, characterAccess));
            var graphicsAccess = Expression.Condition(idle, idleAccess, nonIdleGraphicsAccess);
            var graphicsAssign = Util.Void(Expression.Assign(gd, readData(graphicsAccess)), Expression.PreIncrementAssign(vmli), Expression.PreIncrementAssign(vc));

            // R accesses

            var refreshAccess = Expression.Or(Expression.Constant(0x3F00), refresh);
            var refreshAssign = Util.Void(Expression.Assign(refresh, Expression.And(Expression.Decrement(refresh), Expression.Constant(0xFF))), readData(refreshAccess));

            // S and P accesses

            Func<int, Expression> spritePointerAccess = spriteNumber => Util.Or(Expression.Constant(spriteNumber | 0x1F8), Expression.LeftShift(vm, Expression.Constant(10)));
            Func<int, Expression> spriteDataAccess = spriteNumber => Util.Or(Expression.LeftShift(mp[spriteNumber], Expression.Constant(6)), mc[spriteNumber]);

            Func<int, Expression> spritePointerAssign = spriteNumber => Util.Void(Expression.Assign(mp[spriteNumber], spritePointerAccess(spriteNumber)));
            Func<int, Expression> spriteDataAssign = spriteNumber => Expression.IfThenElse(_state.MnE[spriteNumber],
                Util.Void(Expression.LeftShiftAssign(md[spriteNumber], Expression.Constant(8)), Expression.OrAssign(md[spriteNumber], readData(spriteDataAccess(spriteNumber)))),
                Util.Void(readData(noAccess)));

            // all together now

            var result = new List<Tuple<Expression, Expression>>();

            var invokeI = Util.Invoke(idleRead);
            var invokeX = Util.Invoke(noRead);
            var invokeR = Util.Invoke(refreshAssign);
            var invokeC = Util.Invoke(colorAssignOnBadline);
            var invokeG = Util.Invoke(graphicsAssign);
            var invokeS = Enumerable.Range(0, 8).Select(spriteDataAssign).Select(Util.Invoke).ToArray();
            var invokeP = Enumerable.Range(0, 8).Select(spritePointerAssign).Select(Util.Invoke).ToArray();

            result.AddRange(Enumerable.Range(0, 2).Select(i => new Tuple<Expression, Expression>(_config.MacToCounterX(2 + i * 2), invokeI)));
            result.AddRange(Enumerable.Range(0, 2).Select(i => new Tuple<Expression, Expression>(_config.MacToCounterX(3 + i * 2), invokeX)));
            result.AddRange(Enumerable.Range(0, 8).Select(i => new Tuple<Expression, Expression>(_config.MacToCounterX(6 + (i * 4)), invokeP[i])));
            result.AddRange(Enumerable.Range(0, 8).Select(i => new Tuple<Expression, Expression>(_config.MacToCounterX(7 + (i * 4)), invokeS[i])));
            result.AddRange(Enumerable.Range(0, 8).Select(i => new Tuple<Expression, Expression>(_config.MacToCounterX(8 + (i * 4)), invokeS[i])));
            result.AddRange(Enumerable.Range(0, 8).Select(i => new Tuple<Expression, Expression>(_config.MacToCounterX(9 + (i * 4)), invokeS[i])));
            result.AddRange(Enumerable.Range(0, 5).Select(i => new Tuple<Expression, Expression>(_config.MacToCounterX(38 + (i * 2)), invokeR)));
            result.AddRange(Enumerable.Range(0, 4).Select(i => new Tuple<Expression, Expression>(_config.MacToCounterX(39 + i * 2), invokeX)));
            result.AddRange(Enumerable.Range(0, 40).Select(i => new Tuple<Expression, Expression>(_config.MacToCounterX(47 + (i * 2)), invokeC)));
            result.AddRange(Enumerable.Range(0, 40).Select(i => new Tuple<Expression, Expression>(_config.MacToCounterX(48 + (i * 2)), invokeG)));
            result.Add(new Tuple<Expression, Expression>(_config.MacToCounterX(127), colorClear));
            result.AddRange(Enumerable.Range(0, (_config.ClocksPerRasterValue - 0x1F8) / 8).Select(i => new Tuple<Expression, Expression>(_config.MacToCounterX(127 + (i * 2)), invokeX)));
            result.AddRange(Enumerable.Range(0, (_config.ClocksPerRasterValue - 0x1F8) / 8).Select(i => new Tuple<Expression, Expression>(_config.MacToCounterX(128 + (i * 2)), invokeI)));

            return result;
        }
    }
}
