using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Extensions;

namespace Breadbox.Chips.Vic6567
{
    public class Registers
    {
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

        public Expression M0X { get { return Util.Member(() => _m0x); } }
        public Expression M0Y { get { return Util.Member(() => _m0y); } }
        public Expression M1X { get { return Util.Member(() => _m1x); } }
        public Expression M1Y { get { return Util.Member(() => _m1y); } }
        public Expression M2X { get { return Util.Member(() => _m2x); } }
        public Expression M2Y { get { return Util.Member(() => _m2y); } }
        public Expression M3X { get { return Util.Member(() => _m3x); } }
        public Expression M3Y { get { return Util.Member(() => _m3y); } }
        public Expression M4X { get { return Util.Member(() => _m4x); } }
        public Expression M4Y { get { return Util.Member(() => _m4y); } }
        public Expression M5X { get { return Util.Member(() => _m5x); } }
        public Expression M5Y { get { return Util.Member(() => _m5y); } }
        public Expression M6X { get { return Util.Member(() => _m6x); } }
        public Expression M6Y { get { return Util.Member(() => _m6y); } }
        public Expression M7X { get { return Util.Member(() => _m7x); } }
        public Expression M7Y { get { return Util.Member(() => _m7y); } }
        public Expression ECM { get { return Util.Member(() => _ecm); } }
        public Expression BMM { get { return Util.Member(() => _bmm); } }
        public Expression DEN { get { return Util.Member(() => _den); } }
        public Expression RSEL { get { return Util.Member(() => _rsel); } }
        public Expression YSCROLL { get { return Util.Member(() => _yscroll); } }
        public Expression LPX { get { return Util.Member(() => _lpx); } }
        public Expression LPY { get { return Util.Member(() => _lpy); } }
        public Expression M0E { get { return Util.Member(() => _m0e); } }
        public Expression M1E { get { return Util.Member(() => _m1e); } }
        public Expression M2E { get { return Util.Member(() => _m2e); } }
        public Expression M3E { get { return Util.Member(() => _m3e); } }
        public Expression M4E { get { return Util.Member(() => _m4e); } }
        public Expression M5E { get { return Util.Member(() => _m5e); } }
        public Expression M6E { get { return Util.Member(() => _m6e); } }
        public Expression M7E { get { return Util.Member(() => _m7e); } }
        public Expression RES { get { return Util.Member(() => _res); } }
        public Expression MCM { get { return Util.Member(() => _mcm); } }
        public Expression CSEL { get { return Util.Member(() => _csel); } }
        public Expression XSCROLL { get { return Util.Member(() => _xscroll); } }
        public Expression M0YE { get { return Util.Member(() => _m0ye); } }
        public Expression M1YE { get { return Util.Member(() => _m1ye); } }
        public Expression M2YE { get { return Util.Member(() => _m2ye); } }
        public Expression M3YE { get { return Util.Member(() => _m3ye); } }
        public Expression M4YE { get { return Util.Member(() => _m4ye); } }
        public Expression M5YE { get { return Util.Member(() => _m5ye); } }
        public Expression M6YE { get { return Util.Member(() => _m6ye); } }
        public Expression M7YE { get { return Util.Member(() => _m7ye); } }
        public Expression VM { get { return Util.Member(() => _vm); } }
        public Expression CB { get { return Util.Member(() => _cb); } }
        public Expression IRQ { get { return Util.Member(() => _irq); } }
        public Expression ILP { get { return Util.Member(() => _ilp); } }
        public Expression IMMC { get { return Util.Member(() => _immc); } }
        public Expression IMBC { get { return Util.Member(() => _imbc); } }
        public Expression IRST { get { return Util.Member(() => _irst); } }
        public Expression ELP { get { return Util.Member(() => _elp); } }
        public Expression EMMC { get { return Util.Member(() => _emmc); } }
        public Expression EMBC { get { return Util.Member(() => _embc); } }
        public Expression ERST { get { return Util.Member(() => _erst); } }
        public Expression M0DP { get { return Util.Member(() => _m0dp); } }
        public Expression M1DP { get { return Util.Member(() => _m1dp); } }
        public Expression M2DP { get { return Util.Member(() => _m2dp); } }
        public Expression M3DP { get { return Util.Member(() => _m3dp); } }
        public Expression M4DP { get { return Util.Member(() => _m4dp); } }
        public Expression M5DP { get { return Util.Member(() => _m5dp); } }
        public Expression M6DP { get { return Util.Member(() => _m6dp); } }
        public Expression M7DP { get { return Util.Member(() => _m7dp); } }
        public Expression M0MC { get { return Util.Member(() => _m0mc); } }
        public Expression M1MC { get { return Util.Member(() => _m1mc); } }
        public Expression M2MC { get { return Util.Member(() => _m2mc); } }
        public Expression M3MC { get { return Util.Member(() => _m3mc); } }
        public Expression M4MC { get { return Util.Member(() => _m4mc); } }
        public Expression M5MC { get { return Util.Member(() => _m5mc); } }
        public Expression M6MC { get { return Util.Member(() => _m6mc); } }
        public Expression M7MC { get { return Util.Member(() => _m7mc); } }
        public Expression M0XE { get { return Util.Member(() => _m0xe); } }
        public Expression M1XE { get { return Util.Member(() => _m1xe); } }
        public Expression M2XE { get { return Util.Member(() => _m2xe); } }
        public Expression M3XE { get { return Util.Member(() => _m3xe); } }
        public Expression M4XE { get { return Util.Member(() => _m4xe); } }
        public Expression M5XE { get { return Util.Member(() => _m5xe); } }
        public Expression M6XE { get { return Util.Member(() => _m6xe); } }
        public Expression M7XE { get { return Util.Member(() => _m7xe); } }
        public Expression M0M { get { return Util.Member(() => _m0m); } }
        public Expression M1M { get { return Util.Member(() => _m1m); } }
        public Expression M2M { get { return Util.Member(() => _m2m); } }
        public Expression M3M { get { return Util.Member(() => _m3m); } }
        public Expression M4M { get { return Util.Member(() => _m4m); } }
        public Expression M5M { get { return Util.Member(() => _m5m); } }
        public Expression M6M { get { return Util.Member(() => _m6m); } }
        public Expression M7M { get { return Util.Member(() => _m7m); } }
        public Expression M0D { get { return Util.Member(() => _m0d); } }
        public Expression M1D { get { return Util.Member(() => _m1d); } }
        public Expression M2D { get { return Util.Member(() => _m2d); } }
        public Expression M3D { get { return Util.Member(() => _m3d); } }
        public Expression M4D { get { return Util.Member(() => _m4d); } }
        public Expression M5D { get { return Util.Member(() => _m5d); } }
        public Expression M6D { get { return Util.Member(() => _m6d); } }
        public Expression M7D { get { return Util.Member(() => _m7d); } }
        public Expression EC { get { return Util.Member(() => _ec); } }
        public Expression B0C { get { return Util.Member(() => _b0c); } }
        public Expression B1C { get { return Util.Member(() => _b1c); } }
        public Expression B2C { get { return Util.Member(() => _b2c); } }
        public Expression B3C { get { return Util.Member(() => _b3c); } }
        public Expression MM0 { get { return Util.Member(() => _mm0); } }
        public Expression MM1 { get { return Util.Member(() => _mm1); } }
        public Expression M0C { get { return Util.Member(() => _m0c); } }
        public Expression M1C { get { return Util.Member(() => _m1c); } }
        public Expression M2C { get { return Util.Member(() => _m2c); } }
        public Expression M3C { get { return Util.Member(() => _m3c); } }
        public Expression M4C { get { return Util.Member(() => _m4c); } }
        public Expression M5C { get { return Util.Member(() => _m5c); } }
        public Expression M6C { get { return Util.Member(() => _m6c); } }
        public Expression M7C { get { return Util.Member(() => _m7c); } }
    }
}
