using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Extensions;

namespace Breadbox.Chips.Vic2
{
    public static class Vic2AddressGenerator
    {
        /*
            i  -  i  -  i  -  P0 s  s  s  P1 s  s  s  P2 s  s  s  P3 s  s  s  P4 s  s  s  P5 s  s  s
            00 01 02 03 04 05 06 07 08 09 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29

            P6 s  s  s  P7 s  s  s  r  -  r  -  r  -  r  -  r  c  g  c  g  c  g  c  g  c  g  c  g  c
            30 31 32 33 34 35 36 37 38 39 40 41 42 43 44 45 46 47 48 49 50 51 52 53 54 55 56 57 58 59

            ...

            c   g   c   g
            123 124 125 126
        */

        public static Expression Clock(
            MemberExpression fetchCounter,
            Expression rasterX,
            Expression sprite0BaX,
            MemberExpression ba,
            MemberExpression address,
            Expression badLine,
            IList<MemberExpression> mobDma,
            Expression vm,
            IList<MemberExpression> mobPointer,
            IList<MemberExpression> mobMc,
            Expression bmm,
            Expression ecm,
            Expression idle,
            Expression cb,
            MemberExpression vc,
            Expression rc,
            Func<Expression, Expression> videoMemory,
            MemberExpression refreshCounter,
            MemberExpression vmli,
            ConstantExpression width
            )
        {
            var halfCycleCount = (int) width.Value/4;
            return Expression.Block(
                Expression.IfThenElse(Expression.Equal(rasterX, sprite0BaX),
                    Expression.Assign(fetchCounter, Expression.Constant(0)), Expression.PreIncrementAssign(fetchCounter)),
                Expression.Assign(ba, OutputBa(badLine, fetchCounter, mobDma)),
                Expression.Assign(address, OutputAddress(fetchCounter, mobDma, vm, mobPointer, mobMc, bmm, ecm, idle, cb, vc, rc, videoMemory, badLine, refreshCounter, vmli, halfCycleCount)));
        }

        private static Expression OutputBa(
            Expression badLine,
            Expression fetchCounter,
            IList<MemberExpression> mobDma
            )
        {
            var sprite0 = Expression.AndAlso(mobDma[0], Util.WithinRange(fetchCounter, 0, 9));
            var sprite1 = Expression.AndAlso(mobDma[1], Util.WithinRange(fetchCounter, 4, 13));
            var sprite2 = Expression.AndAlso(mobDma[2], Util.WithinRange(fetchCounter, 8, 17));
            var sprite3 = Expression.AndAlso(mobDma[3], Util.WithinRange(fetchCounter, 12, 21));
            var sprite4 = Expression.AndAlso(mobDma[4], Util.WithinRange(fetchCounter, 16, 25));
            var sprite5 = Expression.AndAlso(mobDma[5], Util.WithinRange(fetchCounter, 20, 29));
            var sprite6 = Expression.AndAlso(mobDma[6], Util.WithinRange(fetchCounter, 24, 33));
            var sprite7 = Expression.AndAlso(mobDma[7], Util.WithinRange(fetchCounter, 28, 37));
            var graphics = Expression.AndAlso(badLine, Util.WithinRange(fetchCounter, 40, 125));
            var result =
                Expression.Not(
                    new[] { sprite0, sprite1, sprite2, sprite3, sprite4, sprite5, sprite6, sprite7, graphics }.Aggregate(
                        Expression.OrElse));
            return result;
        }

        private static SwitchCase PAccess(Expression vm, int spriteNumber)
        {
            return Expression.SwitchCase(Vic2SpriteAddressGenerator.P(vm, spriteNumber), Expression.Constant((spriteNumber * 4) + 6));
        }

        private static SwitchCase SAccess(Expression p, MemberExpression mc, Expression dma, int spriteNumber)
        {
            var cases = Enumerable.Range(0, 3).Select(i => Expression.Constant((spriteNumber * 4) + 7 + i));
            var output = Expression.Condition(dma, Vic2SpriteAddressGenerator.S(p, Expression.PostIncrementAssign(mc)),
                    Expression.Constant(0x3FFF));
            return Expression.SwitchCase(output, cases);
        }

        private static SwitchCase CAccess(Expression vc, Expression vm, Expression badLine)
        {
            var fetch = Vic2GraphicsAddressGenerator.C(vc, vm);
            var cases = Enumerable.Range(0, 40).Select(i => Expression.Constant(47 + (i*2)));
            var output = Expression.Condition(badLine, fetch, Expression.Constant(0x3FFF));
            return Expression.SwitchCase(output, cases);
        }

        private static SwitchCase GAccess(Expression bmm, Expression ecm, Expression idle, Expression cb, MemberExpression vc, Expression rc, Func<Expression, Expression> videoMemory, MemberExpression vmli, int halfCycleCount)
        {
            var idleFetch = Vic2GraphicsAddressGenerator.IdleG(ecm);
            var bitmapFetch = Vic2GraphicsAddressGenerator.BitmapG(cb, vc, rc, ecm);
            var textFetch = Vic2GraphicsAddressGenerator.TextG(cb, videoMemory(vmli), rc, ecm);
            var cases = Enumerable.Range(0, 40).Select(i => Expression.Constant((48 + (i*2)) % halfCycleCount));
            var temp = Expression.Variable(typeof(int));
            var output = Expression.Condition(idle, idleFetch, Expression.Block(new[] {temp},
                Expression.Assign(temp, Expression.Condition(bmm, bitmapFetch, textFetch)),
                Vic2VideoMatrixCounter.Increment(vc, vmli),
                temp));
            return Expression.SwitchCase(output, cases);
        }

        private static SwitchCase RAccess(MemberExpression refreshCounter)
        {
            var cases = Enumerable.Range(0, 5).Select(i => Expression.Constant(38 + (i*2)));
            var temp = Expression.Variable(typeof(int));
            return Expression.SwitchCase(Vic2RefreshAddressGenerator.Generate(Vic2RefreshAddressGenerator.Decrement(refreshCounter)), cases);
        }

        private static Expression OutputAddress(
            Expression fetchCounter,
            IList<MemberExpression> mobDma,
            Expression vm,
            IList<MemberExpression> mobPointer,
            IList<MemberExpression> mobMc,
            Expression bmm,
            Expression ecm,
            Expression idle,
            Expression cb,
            MemberExpression vc,
            Expression rc,
            Func<Expression, Expression> videoMemory,
            Expression badLine,
            MemberExpression refreshCounter,
            MemberExpression vmli,
            int halfCycleCount)
        {
            var result = Expression.Switch(fetchCounter, Expression.Constant(0x3FFF),
                PAccess(vm, 0),
                SAccess(mobPointer[0], mobMc[0], mobDma[0], 0),
                PAccess(vm, 1),
                SAccess(mobPointer[1], mobMc[1], mobDma[1], 1),
                PAccess(vm, 2),
                SAccess(mobPointer[2], mobMc[2], mobDma[2], 2),
                PAccess(vm, 3),
                SAccess(mobPointer[3], mobMc[3], mobDma[3], 3),
                PAccess(vm, 4),
                SAccess(mobPointer[4], mobMc[4], mobDma[4], 4),
                PAccess(vm, 5),
                SAccess(mobPointer[5], mobMc[5], mobDma[5], 5),
                PAccess(vm, 6),
                SAccess(mobPointer[6], mobMc[6], mobDma[6], 6),
                PAccess(vm, 7),
                SAccess(mobPointer[7], mobMc[7], mobDma[7], 7),
                GAccess(bmm, ecm, idle, cb, vc, rc, videoMemory, vmli, halfCycleCount),
                CAccess(vc, vm, badLine),
                RAccess(refreshCounter)
                );
            return result;
        }

    }
}
