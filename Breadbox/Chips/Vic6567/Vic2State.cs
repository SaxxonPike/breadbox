using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Extensions;
#pragma warning disable 649

namespace Breadbox.Chips.Vic6567
{
    public class Vic2State
    {
        private bool _mainBorderFlipFlop;
        private bool _verticalBorderFlipFlop;
        private int _graphicsBuffer;
        private int _graphicsMultiColorFlipFlop;
        private int _mc0;
        private int _mc1;
        private int _mc2;
        private int _mc3;
        private int _mc4;
        private int _mc5;
        private int _mc6;
        private int _mc7;
        private int _mcBase0;
        private int _mcBase1;
        private int _mcBase2;
        private int _mcBase3;
        private int _mcBase4;
        private int _mcBase5;
        private int _mcBase6;
        private int _mcBase7;
        private int _mobBuffer0;
        private int _mobBuffer1;
        private int _mobBuffer2;
        private int _mobBuffer3;
        private int _mobBuffer4;
        private int _mobBuffer5;
        private int _mobBuffer6;
        private int _mobBuffer7;
        private bool _mobMultiColorFlipFlop0;
        private bool _mobMultiColorFlipFlop1;
        private bool _mobMultiColorFlipFlop2;
        private bool _mobMultiColorFlipFlop3;
        private bool _mobMultiColorFlipFlop4;
        private bool _mobMultiColorFlipFlop5;
        private bool _mobMultiColorFlipFlop6;
        private bool _mobMultiColorFlipFlop7;
        private bool _mobDoubleWidthFlipFlop0;
        private bool _mobDoubleWidthFlipFlop1;
        private bool _mobDoubleWidthFlipFlop2;
        private bool _mobDoubleWidthFlipFlop3;
        private bool _mobDoubleWidthFlipFlop4;
        private bool _mobDoubleWidthFlipFlop5;
        private bool _mobDoubleWidthFlipFlop6;
        private bool _mobDoubleWidthFlipFlop7;
        private int _m0x;
        private int _m0y;
        private int _m1x;
        private int _m1y;
        private int _m2x;
        private int _m2y;
        private int _m3x;
        private int _m3y;
        private int _m4x;
        private int _m4y;
        private int _m5x;
        private int _m5y;
        private int _m6x;
        private int _m6y;
        private int _m7x;
        private int _m7y;
        private bool _ecm;
        private bool _bmm;
        private bool _den;
        private bool _rsel;
        private int _yscroll;
        private int _lpx;
        private int _lpy;
        private bool _m0e;
        private bool _m1e;
        private bool _m2e;
        private bool _m3e;
        private bool _m4e;
        private bool _m5e;
        private bool _m6e;
        private bool _m7e;
        private bool _res;
        private bool _mcm;
        private bool _csel;
        private int _xscroll;
        private bool _m0ye;
        private bool _m1ye;
        private bool _m2ye;
        private bool _m3ye;
        private bool _m4ye;
        private bool _m5ye;
        private bool _m6ye;
        private bool _m7ye;
        private int _vm;
        private int _cb;
        private bool _irq;
        private bool _ilp;
        private bool _immc;
        private bool _imbc;
        private bool _irst;
        private bool _elp;
        private bool _emmc;
        private bool _embc;
        private bool _erst;
        private bool _m0dp;
        private bool _m1dp;
        private bool _m2dp;
        private bool _m3dp;
        private bool _m4dp;
        private bool _m5dp;
        private bool _m6dp;
        private bool _m7dp;
        private bool _m0mc;
        private bool _m1mc;
        private bool _m2mc;
        private bool _m3mc;
        private bool _m4mc;
        private bool _m5mc;
        private bool _m6mc;
        private bool _m7mc;
        private bool _m0xe;
        private bool _m1xe;
        private bool _m2xe;
        private bool _m3xe;
        private bool _m4xe;
        private bool _m5xe;
        private bool _m6xe;
        private bool _m7xe;
        private bool _m0m;
        private bool _m1m;
        private bool _m2m;
        private bool _m3m;
        private bool _m4m;
        private bool _m5m;
        private bool _m6m;
        private bool _m7m;
        private bool _m0d;
        private bool _m1d;
        private bool _m2d;
        private bool _m3d;
        private bool _m4d;
        private bool _m5d;
        private bool _m6d;
        private bool _m7d;
        private int _ec;
        private int _b0c;
        private int _b1c;
        private int _b2c;
        private int _b3c;
        private int _mm0;
        private int _mm1;
        private int _m0c;
        private int _m1c;
        private int _m2c;
        private int _m3c;
        private int _m4c;
        private int _m5c;
        private int _m6c;
        private int _m7c;
        private int _ref;
        private int _rc;
        private int _hsync;
        private int _vsync;
        private int _vc;
        private int _vcBase;
        private int _vmli;
        private readonly int[] _videoMemory = new int[64];

        public IndexExpression VideoMemory(Expression index)
        {
            return Expression.ArrayAccess(Util.Member(() => _videoMemory), index);
        }

        public MemberExpression VC
        {
            get { return Util.Member(() => _vc); }
        }

        public MemberExpression VCBASE
        {
            get { return Util.Member(() => _vcBase); }
        }

        public MemberExpression VMLI
        {
            get { return Util.Member(() => _vmli); }
        }

        public MemberExpression HSYNC
        {
            get { return Util.Member(() => _hsync); }
        }

        public MemberExpression VSYNC
        {
            get { return Util.Member(() => _vsync); }
        }

        public MemberExpression RC
        {
            get { return Util.Member(() => _rc); }
        }

        public MemberExpression M0X { get { return Util.Member(() => _m0x); } }
        public MemberExpression M0Y { get { return Util.Member(() => _m0y); } }
        public MemberExpression M1X { get { return Util.Member(() => _m1x); } }
        public MemberExpression M1Y { get { return Util.Member(() => _m1y); } }
        public MemberExpression M2X { get { return Util.Member(() => _m2x); } }
        public MemberExpression M2Y { get { return Util.Member(() => _m2y); } }
        public MemberExpression M3X { get { return Util.Member(() => _m3x); } }
        public MemberExpression M3Y { get { return Util.Member(() => _m3y); } }
        public MemberExpression M4X { get { return Util.Member(() => _m4x); } }
        public MemberExpression M4Y { get { return Util.Member(() => _m4y); } }
        public MemberExpression M5X { get { return Util.Member(() => _m5x); } }
        public MemberExpression M5Y { get { return Util.Member(() => _m5y); } }
        public MemberExpression M6X { get { return Util.Member(() => _m6x); } }
        public MemberExpression M6Y { get { return Util.Member(() => _m6y); } }
        public MemberExpression M7X { get { return Util.Member(() => _m7x); } }
        public MemberExpression M7Y { get { return Util.Member(() => _m7y); } }
        public MemberExpression ECM { get { return Util.Member(() => _ecm); } }
        public MemberExpression BMM { get { return Util.Member(() => _bmm); } }
        public MemberExpression DEN { get { return Util.Member(() => _den); } }
        public MemberExpression RSEL { get { return Util.Member(() => _rsel); } }
        public MemberExpression YSCROLL { get { return Util.Member(() => _yscroll); } }
        public MemberExpression LPX { get { return Util.Member(() => _lpx); } }
        public MemberExpression LPY { get { return Util.Member(() => _lpy); } }
        public MemberExpression M0E { get { return Util.Member(() => _m0e); } }
        public MemberExpression M1E { get { return Util.Member(() => _m1e); } }
        public MemberExpression M2E { get { return Util.Member(() => _m2e); } }
        public MemberExpression M3E { get { return Util.Member(() => _m3e); } }
        public MemberExpression M4E { get { return Util.Member(() => _m4e); } }
        public MemberExpression M5E { get { return Util.Member(() => _m5e); } }
        public MemberExpression M6E { get { return Util.Member(() => _m6e); } }
        public MemberExpression M7E { get { return Util.Member(() => _m7e); } }
        public MemberExpression RES { get { return Util.Member(() => _res); } }
        public MemberExpression MCM { get { return Util.Member(() => _mcm); } }
        public MemberExpression CSEL { get { return Util.Member(() => _csel); } }
        public MemberExpression XSCROLL { get { return Util.Member(() => _xscroll); } }
        public MemberExpression M0YE { get { return Util.Member(() => _m0ye); } }
        public MemberExpression M1YE { get { return Util.Member(() => _m1ye); } }
        public MemberExpression M2YE { get { return Util.Member(() => _m2ye); } }
        public MemberExpression M3YE { get { return Util.Member(() => _m3ye); } }
        public MemberExpression M4YE { get { return Util.Member(() => _m4ye); } }
        public MemberExpression M5YE { get { return Util.Member(() => _m5ye); } }
        public MemberExpression M6YE { get { return Util.Member(() => _m6ye); } }
        public MemberExpression M7YE { get { return Util.Member(() => _m7ye); } }
        public MemberExpression VM { get { return Util.Member(() => _vm); } }
        public MemberExpression CB { get { return Util.Member(() => _cb); } }
        public MemberExpression IRQ { get { return Util.Member(() => _irq); } }
        public MemberExpression ILP { get { return Util.Member(() => _ilp); } }
        public MemberExpression IMMC { get { return Util.Member(() => _immc); } }
        public MemberExpression IMBC { get { return Util.Member(() => _imbc); } }
        public MemberExpression IRST { get { return Util.Member(() => _irst); } }
        public MemberExpression ELP { get { return Util.Member(() => _elp); } }
        public MemberExpression EMMC { get { return Util.Member(() => _emmc); } }
        public MemberExpression EMBC { get { return Util.Member(() => _embc); } }
        public MemberExpression ERST { get { return Util.Member(() => _erst); } }
        public MemberExpression M0DP { get { return Util.Member(() => _m0dp); } }
        public MemberExpression M1DP { get { return Util.Member(() => _m1dp); } }
        public MemberExpression M2DP { get { return Util.Member(() => _m2dp); } }
        public MemberExpression M3DP { get { return Util.Member(() => _m3dp); } }
        public MemberExpression M4DP { get { return Util.Member(() => _m4dp); } }
        public MemberExpression M5DP { get { return Util.Member(() => _m5dp); } }
        public MemberExpression M6DP { get { return Util.Member(() => _m6dp); } }
        public MemberExpression M7DP { get { return Util.Member(() => _m7dp); } }
        public MemberExpression M0MC { get { return Util.Member(() => _m0mc); } }
        public MemberExpression M1MC { get { return Util.Member(() => _m1mc); } }
        public MemberExpression M2MC { get { return Util.Member(() => _m2mc); } }
        public MemberExpression M3MC { get { return Util.Member(() => _m3mc); } }
        public MemberExpression M4MC { get { return Util.Member(() => _m4mc); } }
        public MemberExpression M5MC { get { return Util.Member(() => _m5mc); } }
        public MemberExpression M6MC { get { return Util.Member(() => _m6mc); } }
        public MemberExpression M7MC { get { return Util.Member(() => _m7mc); } }
        public MemberExpression M0XE { get { return Util.Member(() => _m0xe); } }
        public MemberExpression M1XE { get { return Util.Member(() => _m1xe); } }
        public MemberExpression M2XE { get { return Util.Member(() => _m2xe); } }
        public MemberExpression M3XE { get { return Util.Member(() => _m3xe); } }
        public MemberExpression M4XE { get { return Util.Member(() => _m4xe); } }
        public MemberExpression M5XE { get { return Util.Member(() => _m5xe); } }
        public MemberExpression M6XE { get { return Util.Member(() => _m6xe); } }
        public MemberExpression M7XE { get { return Util.Member(() => _m7xe); } }
        public MemberExpression M0M { get { return Util.Member(() => _m0m); } }
        public MemberExpression M1M { get { return Util.Member(() => _m1m); } }
        public MemberExpression M2M { get { return Util.Member(() => _m2m); } }
        public MemberExpression M3M { get { return Util.Member(() => _m3m); } }
        public MemberExpression M4M { get { return Util.Member(() => _m4m); } }
        public MemberExpression M5M { get { return Util.Member(() => _m5m); } }
        public MemberExpression M6M { get { return Util.Member(() => _m6m); } }
        public MemberExpression M7M { get { return Util.Member(() => _m7m); } }
        public MemberExpression M0D { get { return Util.Member(() => _m0d); } }
        public MemberExpression M1D { get { return Util.Member(() => _m1d); } }
        public MemberExpression M2D { get { return Util.Member(() => _m2d); } }
        public MemberExpression M3D { get { return Util.Member(() => _m3d); } }
        public MemberExpression M4D { get { return Util.Member(() => _m4d); } }
        public MemberExpression M5D { get { return Util.Member(() => _m5d); } }
        public MemberExpression M6D { get { return Util.Member(() => _m6d); } }
        public MemberExpression M7D { get { return Util.Member(() => _m7d); } }
        public MemberExpression EC { get { return Util.Member(() => _ec); } }
        public MemberExpression B0C { get { return Util.Member(() => _b0c); } }
        public MemberExpression B1C { get { return Util.Member(() => _b1c); } }
        public MemberExpression B2C { get { return Util.Member(() => _b2c); } }
        public MemberExpression B3C { get { return Util.Member(() => _b3c); } }
        public MemberExpression MM0 { get { return Util.Member(() => _mm0); } }
        public MemberExpression MM1 { get { return Util.Member(() => _mm1); } }
        public MemberExpression M0C { get { return Util.Member(() => _m0c); } }
        public MemberExpression M1C { get { return Util.Member(() => _m1c); } }
        public MemberExpression M2C { get { return Util.Member(() => _m2c); } }
        public MemberExpression M3C { get { return Util.Member(() => _m3c); } }
        public MemberExpression M4C { get { return Util.Member(() => _m4c); } }
        public MemberExpression M5C { get { return Util.Member(() => _m5c); } }
        public MemberExpression M6C { get { return Util.Member(() => _m6c); } }
        public MemberExpression M7C { get { return Util.Member(() => _m7c); } }

        public MemberExpression GraphicsBuffer
        {
            get { return Util.Member(() => _graphicsBuffer); }
        }

        public MemberExpression GraphicsMultiColorFlipFlop
        {
            get { return Util.Member(() => _graphicsMultiColorFlipFlop); }
        }

        public MemberExpression MainBorderFlipFlop
        {
            get { return Util.Member(() => _mainBorderFlipFlop); }
        }

        public MemberExpression VerticalBorderFlipFlop
        {
            get { return Util.Member(() => _verticalBorderFlipFlop); }
        }

        public MemberExpression Mc0
        {
            get { return Util.Member(() => _mc0); }
        }

        public MemberExpression Mc1
        {
            get { return Util.Member(() => _mc1); }
        }

        public MemberExpression Mc2
        {
            get { return Util.Member(() => _mc2); }
        }

        public MemberExpression Mc3
        {
            get { return Util.Member(() => _mc3); }
        }

        public MemberExpression Mc4
        {
            get { return Util.Member(() => _mc4); }
        }

        public MemberExpression Mc5
        {
            get { return Util.Member(() => _mc5); }
        }

        public MemberExpression Mc6
        {
            get { return Util.Member(() => _mc6); }
        }

        public MemberExpression Mc7
        {
            get { return Util.Member(() => _mc7); }
        }

        public MemberExpression McBase0
        {
            get { return Util.Member(() => _mcBase0); }
        }

        public MemberExpression McBase1
        {
            get { return Util.Member(() => _mcBase1); }
        }

        public MemberExpression McBase2
        {
            get { return Util.Member(() => _mcBase2); }
        }

        public MemberExpression McBase3
        {
            get { return Util.Member(() => _mcBase3); }
        }

        public MemberExpression McBase4
        {
            get { return Util.Member(() => _mcBase4); }
        }

        public MemberExpression McBase5
        {
            get { return Util.Member(() => _mcBase5); }
        }

        public MemberExpression McBase6
        {
            get { return Util.Member(() => _mcBase6); }
        }

        public MemberExpression McBase7
        {
            get { return Util.Member(() => _mcBase7); }
        }

        public MemberExpression MobBuffer0
        {
            get { return Util.Member(() => _mobBuffer0); }
        }

        public MemberExpression MobBuffer1
        {
            get { return Util.Member(() => _mobBuffer1); }
        }

        public MemberExpression MobBuffer2
        {
            get { return Util.Member(() => _mobBuffer2); }
        }

        public MemberExpression MobBuffer3
        {
            get { return Util.Member(() => _mobBuffer3); }
        }

        public MemberExpression MobBuffer4
        {
            get { return Util.Member(() => _mobBuffer4); }
        }

        public MemberExpression MobBuffer5
        {
            get { return Util.Member(() => _mobBuffer5); }
        }

        public MemberExpression MobBuffer6
        {
            get { return Util.Member(() => _mobBuffer6); }
        }

        public MemberExpression MobBuffer7
        {
            get { return Util.Member(() => _mobBuffer7); }
        }

        public MemberExpression MobMultiColorFlipFlop0
        {
            get { return Util.Member(() => _mobMultiColorFlipFlop0); }
        }

        public MemberExpression MobMultiColorFlipFlop1
        {
            get { return Util.Member(() => _mobMultiColorFlipFlop1); }
        }

        public MemberExpression MobMultiColorFlipFlop2
        {
            get { return Util.Member(() => _mobMultiColorFlipFlop2); }
        }

        public MemberExpression MobMultiColorFlipFlop3
        {
            get { return Util.Member(() => _mobMultiColorFlipFlop3); }
        }

        public MemberExpression MobMultiColorFlipFlop4
        {
            get { return Util.Member(() => _mobMultiColorFlipFlop4); }
        }

        public MemberExpression MobMultiColorFlipFlop5
        {
            get { return Util.Member(() => _mobMultiColorFlipFlop5); }
        }

        public MemberExpression MobMultiColorFlipFlop6
        {
            get { return Util.Member(() => _mobMultiColorFlipFlop6); }
        }

        public MemberExpression MobMultiColorFlipFlop7
        {
            get { return Util.Member(() => _mobMultiColorFlipFlop7); }
        }

        public MemberExpression MobDoubleWidthFlipFlop0
        {
            get { return Util.Member(() => _mobDoubleWidthFlipFlop0); }
        }

        public MemberExpression MobDoubleWidthFlipFlop1
        {
            get { return Util.Member(() => _mobDoubleWidthFlipFlop1); }
        }

        public MemberExpression MobDoubleWidthFlipFlop2
        {
            get { return Util.Member(() => _mobDoubleWidthFlipFlop2); }
        }

        public MemberExpression MobDoubleWidthFlipFlop3
        {
            get { return Util.Member(() => _mobDoubleWidthFlipFlop3); }
        }

        public MemberExpression MobDoubleWidthFlipFlop4
        {
            get { return Util.Member(() => _mobDoubleWidthFlipFlop4); }
        }

        public MemberExpression MobDoubleWidthFlipFlop5
        {
            get { return Util.Member(() => _mobDoubleWidthFlipFlop5); }
        }

        public MemberExpression MobDoubleWidthFlipFlop6
        {
            get { return Util.Member(() => _mobDoubleWidthFlipFlop6); }
        }

        public MemberExpression MobDoubleWidthFlipFlop7
        {
            get { return Util.Member(() => _mobDoubleWidthFlipFlop7); }
        }

        public MemberExpression REF
        {
            get { return Util.Member(() => _ref); }
        }

    }
}
