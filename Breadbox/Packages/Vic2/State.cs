using System.Collections.Generic;
using System.Linq.Expressions;

// ReSharper disable InconsistentNaming
#pragma warning disable 0649

namespace Breadbox.Packages.Vic2
{
    public class State
    {
        private readonly Config _config;
        private int _M0X;
        private int _M1X;
        private int _M2X;
        private int _M3X;
        private int _M4X;
        private int _M5X;
        private int _M6X;
        private int _M7X;
        private int _M0Y;
        private int _M1Y;
        private int _M2Y;
        private int _M3Y;
        private int _M4Y;
        private int _M5Y;
        private int _M6Y;
        private int _M7Y;
        private int _RASTER; // called RC in 6567 datasheet (but so is actual RC)
        private bool _ECM;
        private bool _BMM;
        private bool _DEN; // called BLNK in 6567 datasheet
        private bool _RSEL;
        private int _YSCROLL; // called Y in 6567 datasheet
        private int _LPX;
        private int _LPY;
        private bool _M0E;
        private bool _M1E;
        private bool _M2E;
        private bool _M3E;
        private bool _M4E;
        private bool _M5E;
        private bool _M6E;
        private bool _M7E;
        private bool _RES;
        private bool _MCM;
        private bool _CSEL;
        private int _XSCROLL; // called X in 6567 datasheet
        private bool _M0YE;
        private bool _M1YE;
        private bool _M2YE;
        private bool _M3YE;
        private bool _M4YE;
        private bool _M5YE;
        private bool _M6YE;
        private bool _M7YE;
        private int _VM;
        private int _CB;
        private bool _IRQ;
        private bool _ILP;
        private bool _IMMC;
        private bool _IMBC;
        private bool _IRST;
        private bool _ELP;
        private bool _EMMC;
        private bool _EMBC;
        private bool _ERST;
        private bool _M0DP;
        private bool _M1DP;
        private bool _M2DP;
        private bool _M3DP;
        private bool _M4DP;
        private bool _M5DP;
        private bool _M6DP;
        private bool _M7DP;
        private bool _M0MC;
        private bool _M1MC;
        private bool _M2MC;
        private bool _M3MC;
        private bool _M4MC;
        private bool _M5MC;
        private bool _M6MC;
        private bool _M7MC;
        private bool _M0XE;
        private bool _M1XE;
        private bool _M2XE;
        private bool _M3XE;
        private bool _M4XE;
        private bool _M5XE;
        private bool _M6XE;
        private bool _M7XE;
        private bool _M0M;
        private bool _M1M;
        private bool _M2M;
        private bool _M3M;
        private bool _M4M;
        private bool _M5M;
        private bool _M6M;
        private bool _M7M;
        private bool _M0D;
        private bool _M1D;
        private bool _M2D;
        private bool _M3D;
        private bool _M4D;
        private bool _M5D;
        private bool _M6D;
        private bool _M7D;
        private int _EC;
        private int _B0C;
        private int _B1C;
        private int _B2C;
        private int _B3C;
        private int _MM0;
        private int _MM1;
        private int _M0C;
        private int _M1C;
        private int _M2C;
        private int _M3C;
        private int _M4C;
        private int _M5C;
        private int _M6C;
        private int _M7C;
        private int _VC;
        private int _VCBASE;
        private int _MC0;
        private int _MC1;
        private int _MC2;
        private int _MC3;
        private int _MC4;
        private int _MC5;
        private int _MC6;
        private int _MC7;
        private int _MCBASE0;
        private int _MCBASE1;
        private int _MCBASE2;
        private int _MCBASE3;
        private int _MCBASE4;
        private int _MCBASE5;
        private int _MCBASE6;
        private int _MCBASE7;
        private int _RC;
        private int _D00;
        private int _D01;
        private int _D02;
        private int _D03;
        private int _D04;
        private int _D05;
        private int _D06;
        private int _D07;
        private int _D08;
        private int _D09;
        private int _D10;
        private int _D11;
        private int _D12;
        private int _D13;
        private int _D14;
        private int _D15;
        private int _D16;
        private int _D17;
        private int _D18;
        private int _D19;
        private int _D20;
        private int _D21;
        private int _D22;
        private int _D23;
        private int _D24;
        private int _D25;
        private int _D26;
        private int _D27;
        private int _D28;
        private int _D29;
        private int _D30;
        private int _D31;
        private int _D32;
        private int _D33;
        private int _D34;
        private int _D35;
        private int _D36;
        private int _D37;
        private int _D38;
        private int _D39;
        private bool _HBLANK;
        private bool _VBLANK;
        private int _VMLI;
        private int _MP0;
        private int _MP1;
        private int _MP2;
        private int _MP3;
        private int _MP4;
        private int _MP5;
        private int _MP6;
        private int _MP7;
        private bool _IDLE;
        private int _MD0;
        private int _MD1;
        private int _MD2;
        private int _MD3;
        private int _MD4;
        private int _MD5;
        private int _MD6;
        private int _MD7;
        private int _GD;
        private int _REF;
        private int _RASTERX;
        private int _RASTERXC;
        private bool _GDMC;
        private int _COLORBUFFER;
        private bool _MXMC0;
        private bool _MXMC1;
        private bool _MXMC2;
        private bool _MXMC3;
        private bool _MXMC4;
        private bool _MXMC5;
        private bool _MXMC6;
        private bool _MXMC7;
        private bool _MSRE0;
        private bool _MSRE1;
        private bool _MSRE2;
        private bool _MSRE3;
        private bool _MSRE4;
        private bool _MSRE5;
        private bool _MSRE6;
        private bool _MSRE7;
        private bool _ADDRESS;
        private bool _BADLINE;
        private bool _BADLINEENABLE;
        private bool _M0YET;
        private bool _M1YET;
        private bool _M2YET;
        private bool _M3YET;
        private bool _M4YET;
        private bool _M5YET;
        private bool _M6YET;
        private bool _M7YET;
        private bool _M0XET;
        private bool _M1XET;
        private bool _M2XET;
        private bool _M3XET;
        private bool _M4XET;
        private bool _M5XET;
        private bool _M6XET;
        private bool _M7XET;
        private bool _M0DMA;
        private bool _M1DMA;
        private bool _M2DMA;
        private bool _M3DMA;
        private bool _M4DMA;
        private bool _M5DMA;
        private bool _M6DMA;
        private bool _M7DMA;

        public State(Config config)
        {
            _config = config;
            Reset();
        }

        public void Reset()
        {
            _VBLANK = true;
            _HBLANK = true;
        }

        public Expression ADDRESS
        {
            get { return Util.Member(() => _ADDRESS); }
        }

        /// <summary>
        /// Badline condition.
        /// </summary>
        public Expression BADLINE
        {
            get { return Util.Member(() => _BADLINE); }
        }

        /// <summary>
        /// Enable badline condition.
        /// </summary>
        public Expression BADLINEENABLE
        {
            get { return Util.Member(() => _BADLINEENABLE); }
        }

        public IList<Expression> MnX
        {
            get
            {
                return new Expression[]
                {
                    Util.Member(() => _M0X),
                    Util.Member(() => _M1X),
                    Util.Member(() => _M2X),
                    Util.Member(() => _M3X),
                    Util.Member(() => _M4X),
                    Util.Member(() => _M5X),
                    Util.Member(() => _M6X),
                    Util.Member(() => _M7X)
                };
            }
        }

        public IList<Expression> MnY
        {
            get
            {
                return new Expression[]
                {
                    Util.Member(() => _M0Y),
                    Util.Member(() => _M1Y),
                    Util.Member(() => _M2Y),
                    Util.Member(() => _M3Y),
                    Util.Member(() => _M4Y),
                    Util.Member(() => _M5Y),
                    Util.Member(() => _M6Y),
                    Util.Member(() => _M7Y)
                };
            }
        }

        public Expression RASTER
        {
            get { return Util.Member(() => _RASTER); }
        }

        public Expression ECM
        {
            get { return Util.Member(() => _ECM); }
        }

        public Expression BMM
        {
            get { return Util.Member(() => _BMM); }
        }

        public Expression DEN
        {
            get { return Util.Member(() => _DEN); }
        }

        public Expression RSEL
        {
            get { return Util.Member(() => _RSEL); }
        }

        public Expression YSCROLL
        {
            get { return Util.Member(() => _YSCROLL); }
        }

        public Expression LPX
        {
            get { return Util.Member(() => _LPX); }
        }

        public Expression LPY
        {
            get { return Util.Member(() => _LPY); }
        }

        public IList<Expression> MnE
        {
            get
            {
                return new Expression[]
                {
                    Util.Member(() => _M0E),
                    Util.Member(() => _M1E),
                    Util.Member(() => _M2E),
                    Util.Member(() => _M3E),
                    Util.Member(() => _M4E),
                    Util.Member(() => _M5E),
                    Util.Member(() => _M6E),
                    Util.Member(() => _M7E)
                };
            }
        }

        public Expression RES
        {
            get { return Util.Member(() => _RES); }
        }

        public Expression MCM
        {
            get { return Util.Member(() => _MCM); }
        }

        public Expression CSEL
        {
            get { return Util.Member(() => _CSEL); }
        }

        public Expression XSCROLL
        {
            get { return Util.Member(() => _XSCROLL); }
        }

        public IList<Expression> MnYE
        {
            get
            {
                return new Expression[]
                {
                    Util.Member(() => _M0YE),
                    Util.Member(() => _M1YE),
                    Util.Member(() => _M2YE),
                    Util.Member(() => _M3YE),
                    Util.Member(() => _M4YE),
                    Util.Member(() => _M5YE),
                    Util.Member(() => _M6YE),
                    Util.Member(() => _M7YE)
                };
            }
        }

        public IList<Expression> MnYET
        {
            get
            {
                return new Expression[]
                {
                    Util.Member(() => _M0YET),
                    Util.Member(() => _M1YET),
                    Util.Member(() => _M2YET),
                    Util.Member(() => _M3YET),
                    Util.Member(() => _M4YET),
                    Util.Member(() => _M5YET),
                    Util.Member(() => _M6YET),
                    Util.Member(() => _M7YET)
                };
            }
        }

        public IList<Expression> MnXET
        {
            get
            {
                return new Expression[]
                {
                    Util.Member(() => _M0XET),
                    Util.Member(() => _M1XET),
                    Util.Member(() => _M2XET),
                    Util.Member(() => _M3XET),
                    Util.Member(() => _M4XET),
                    Util.Member(() => _M5XET),
                    Util.Member(() => _M6XET),
                    Util.Member(() => _M7XET)
                };
            }
        }

        public IList<Expression> MnDMA
        {
            get
            {
                return new Expression[]
                {
                    Util.Member(() => _M0DMA),
                    Util.Member(() => _M1DMA),
                    Util.Member(() => _M2DMA),
                    Util.Member(() => _M3DMA),
                    Util.Member(() => _M4DMA),
                    Util.Member(() => _M5DMA),
                    Util.Member(() => _M6DMA),
                    Util.Member(() => _M7DMA)
                };
            }
        }

        public Expression VM
        {
            get { return Util.Member(() => _VM); }
        }

        public Expression CB
        {
            get { return Util.Member(() => _CB); }
        }

        public Expression IRQ
        {
            get { return Util.Member(() => _IRQ); }
        }

        public Expression ILP
        {
            get { return Util.Member(() => _ILP); }
        }

        public Expression IMMC
        {
            get { return Util.Member(() => _IMMC); }
        }

        public Expression IMBC
        {
            get { return Util.Member(() => _IMBC); }
        }

        public Expression IRST
        {
            get { return Util.Member(() => _IRST); }
        }

        public Expression ELP
        {
            get { return Util.Member(() => _ELP); }
        }

        public Expression EMMC
        {
            get { return Util.Member(() => _EMMC); }
        }

        public Expression EMBC
        {
            get { return Util.Member(() => _EMBC); }
        }

        public Expression ERST
        {
            get { return Util.Member(() => _ERST); }
        }

        public IList<Expression> MnDP
        {
            get
            {
                return new Expression[]
                {
                    Util.Member(() => _M0DP),
                    Util.Member(() => _M1DP),
                    Util.Member(() => _M2DP),
                    Util.Member(() => _M3DP),
                    Util.Member(() => _M4DP),
                    Util.Member(() => _M5DP),
                    Util.Member(() => _M6DP),
                    Util.Member(() => _M7DP)
                };
            }
        }

        public IList<Expression> MnMC
        {
            get
            {
                return new Expression[]
                {
                    Util.Member(() => _M0MC),
                    Util.Member(() => _M1MC),
                    Util.Member(() => _M2MC),
                    Util.Member(() => _M3MC),
                    Util.Member(() => _M4MC),
                    Util.Member(() => _M5MC),
                    Util.Member(() => _M6MC),
                    Util.Member(() => _M7MC)
                };
            }
        }

        public IList<Expression> MnXE
        {
            get
            {
                return new Expression[]
                {
                    Util.Member(() => _M0XE),
                    Util.Member(() => _M1XE),
                    Util.Member(() => _M2XE),
                    Util.Member(() => _M3XE),
                    Util.Member(() => _M4XE),
                    Util.Member(() => _M5XE),
                    Util.Member(() => _M6XE),
                    Util.Member(() => _M7XE)
                };
            }
        }

        public IList<Expression> MnM
        {
            get
            {
                return new Expression[]
                {
                    Util.Member(() => _M0M),
                    Util.Member(() => _M1M),
                    Util.Member(() => _M2M),
                    Util.Member(() => _M3M),
                    Util.Member(() => _M4M),
                    Util.Member(() => _M5M),
                    Util.Member(() => _M6M),
                    Util.Member(() => _M7M)
                };
            }
        }

        public IList<Expression> MnD
        {
            get
            {
                return new Expression[]
                {
                    Util.Member(() => _M0D),
                    Util.Member(() => _M1D),
                    Util.Member(() => _M2D),
                    Util.Member(() => _M3D),
                    Util.Member(() => _M4D),
                    Util.Member(() => _M5D),
                    Util.Member(() => _M6D),
                    Util.Member(() => _M7D)
                };
            }
        }

        public Expression EC
        {
            get { return Util.Member(() => _EC); }
        }

        public Expression B0C
        {
            get { return Util.Member(() => _B0C); }
        }

        public Expression B1C
        {
            get { return Util.Member(() => _B1C); }
        }

        public Expression B2C
        {
            get { return Util.Member(() => _B2C); }
        }

        public Expression B3C
        {
            get { return Util.Member(() => _B3C); }
        }

        public Expression MM0
        {
            get { return Util.Member(() => _MM0); }
        }

        public Expression MM1
        {
            get { return Util.Member(() => _MM1); }
        }

        public IList<Expression> MnC
        {
            get
            {
                return new Expression[]
                {
                    Util.Member(() => _M0C),
                    Util.Member(() => _M1C),
                    Util.Member(() => _M2C),
                    Util.Member(() => _M3C),
                    Util.Member(() => _M4C),
                    Util.Member(() => _M5C),
                    Util.Member(() => _M6C),
                    Util.Member(() => _M7C)
                };
            }
        }

        public IList<Expression> MCn
        {
            get
            {
                return new Expression[]
                {
                    Util.Member(() => _MC0),
                    Util.Member(() => _MC1),
                    Util.Member(() => _MC2),
                    Util.Member(() => _MC3),
                    Util.Member(() => _MC4),
                    Util.Member(() => _MC5),
                    Util.Member(() => _MC6),
                    Util.Member(() => _MC7)
                };
            }
        }

        public IList<Expression> MCBASEn
        {
            get
            {
                return new Expression[]
                {
                    Util.Member(() => _MCBASE0),
                    Util.Member(() => _MCBASE1),
                    Util.Member(() => _MCBASE2),
                    Util.Member(() => _MCBASE3),
                    Util.Member(() => _MCBASE4),
                    Util.Member(() => _MCBASE5),
                    Util.Member(() => _MCBASE6),
                    Util.Member(() => _MCBASE7)
                };
            }
        }

        public IList<Expression> Dn
        {
            get
            {
                return new Expression[]
                {
                    Util.Member(() => _D00),
                    Util.Member(() => _D01),
                    Util.Member(() => _D02),
                    Util.Member(() => _D03),
                    Util.Member(() => _D04),
                    Util.Member(() => _D05),
                    Util.Member(() => _D06),
                    Util.Member(() => _D07),
                    Util.Member(() => _D08),
                    Util.Member(() => _D09),
                    Util.Member(() => _D10),
                    Util.Member(() => _D11),
                    Util.Member(() => _D12),
                    Util.Member(() => _D13),
                    Util.Member(() => _D14),
                    Util.Member(() => _D15),
                    Util.Member(() => _D16),
                    Util.Member(() => _D17),
                    Util.Member(() => _D18),
                    Util.Member(() => _D19),
                    Util.Member(() => _D20),
                    Util.Member(() => _D21),
                    Util.Member(() => _D22),
                    Util.Member(() => _D23),
                    Util.Member(() => _D24),
                    Util.Member(() => _D25),
                    Util.Member(() => _D26),
                    Util.Member(() => _D27),
                    Util.Member(() => _D28),
                    Util.Member(() => _D29),
                    Util.Member(() => _D30),
                    Util.Member(() => _D31),
                    Util.Member(() => _D32),
                    Util.Member(() => _D33),
                    Util.Member(() => _D34),
                    Util.Member(() => _D35),
                    Util.Member(() => _D36),
                    Util.Member(() => _D37),
                    Util.Member(() => _D38),
                    Util.Member(() => _D39)
                };
            }
        }

        public Expression RC
        {
            get { return Util.Member(() => _RC); }
        }

        public Expression HBLANK
        {
            get { return Util.Member(() => _HBLANK); }
        }

        public Expression VBLANK
        {
            get { return Util.Member(() => _VBLANK); }
        }

        public Expression VC
        {
            get { return Util.Member(() => _VC); }
        }

        public Expression VCBASE
        {
            get { return Util.Member(() => _VCBASE); }
        }

        public Expression VMLI
        {
            get { return Util.Member(() => _VMLI); }
        }

        /// <summary>
        /// Mob data pointers.
        /// </summary>
        public IList<Expression> MPn
        {
            get
            {
                return new Expression[]
                {
                    Util.Member(() => _MP0),
                    Util.Member(() => _MP1),
                    Util.Member(() => _MP2),
                    Util.Member(() => _MP3),
                    Util.Member(() => _MP4),
                    Util.Member(() => _MP5),
                    Util.Member(() => _MP6),
                    Util.Member(() => _MP7)
                };
            }
        }

        /// <summary>
        /// Mob data buffers.
        /// </summary>
        public IList<Expression> MDn
        {
            get
            {
                return new Expression[]
                {
                    Util.Member(() => _MD0),
                    Util.Member(() => _MD1),
                    Util.Member(() => _MD2),
                    Util.Member(() => _MD3),
                    Util.Member(() => _MD4),
                    Util.Member(() => _MD5),
                    Util.Member(() => _MD6),
                    Util.Member(() => _MD7)
                };
            }
        }

        /// <summary>
        /// Idle state.
        /// </summary>
        public Expression IDLE
        {
            get { return Util.Member(() => _IDLE); }
        }

        /// <summary>
        /// Graphics shift register data.
        /// </summary>
        public Expression GD
        {
            get { return Util.Member(() => _GD); }
        }

        /// <summary>
        /// Refresh counter.
        /// </summary>
        public Expression REF
        {
            get { return Util.Member(() => _REF); }
        }

        /// <summary>
        /// The "visible" raster X counter (pauses in new NTSC for 8 clocks)
        /// </summary>
        public Expression RASTERX
        {
            get { return Util.Member(() => _RASTERX); }
        }

        /// <summary>
        /// Actual raster X counter.
        /// </summary>
        public Expression RASTERXC
        {
            get { return Util.Member(() => _RASTERXC); }
        }

        /// <summary>
        /// Graphics data output multicolor flipflop.
        /// </summary>
        public Expression GDMC
        {
            get { return Util.Member(() => _GDMC); }
        }

        /// <summary>
        /// Last accessed C data.
        /// </summary>
        public Expression CBUFFER
        {
            get { return Util.Member(() => _COLORBUFFER); }
        }

        /// <summary>
        /// Mob x-expansion flipflop.
        /// </summary>
        public IList<Expression> MXMCn
        {
            get
            {
                return new Expression[]
                {
                    Util.Member(() => _MXMC0),
                    Util.Member(() => _MXMC1),
                    Util.Member(() => _MXMC2),
                    Util.Member(() => _MXMC3),
                    Util.Member(() => _MXMC4),
                    Util.Member(() => _MXMC5),
                    Util.Member(() => _MXMC6),
                    Util.Member(() => _MXMC7)
                };
            }
        }

        /// <summary>
        /// Mob shift register enable.
        /// </summary>
        public IList<Expression> MSREn
        {
            get
            {
                return new Expression[]
                {
                    Util.Member(() => _MSRE0),
                    Util.Member(() => _MSRE1),
                    Util.Member(() => _MSRE2),
                    Util.Member(() => _MSRE3),
                    Util.Member(() => _MSRE4),
                    Util.Member(() => _MSRE5),
                    Util.Member(() => _MSRE6),
                    Util.Member(() => _MSRE7)
                };
            }
        }
    }
}
