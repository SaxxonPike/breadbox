using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Breadbox.Packages.Vic2
{
    public class Config
    {
        private readonly int _clocksPerRaster;
        private readonly int _rastersPerFrame;
        private readonly int _hBlankSetX;
        private readonly int _hBlankClearX;
        private readonly int _vBlankSetY;
        private readonly int _vBlankClearY;
        private readonly int _fetchSprite0BaX;
        private readonly int _rasterStartX;
        private readonly int _clockHzNum;
        private readonly int _clockHzDen;

        public Config(
            int clocksPerRaster,
            int rastersPerFrame,
            int hBlankSetX,
            int hBlankClearX,
            int vBlankSetY,
            int vBlankClearY,
            int fetchSprite0BaX,
            int rasterStartX,
            int baseClockHz,
            int dividerClockHz
            )
        {
            _clocksPerRaster = clocksPerRaster;
            _rastersPerFrame = rastersPerFrame;
            _hBlankSetX = hBlankSetX;
            _hBlankClearX = hBlankClearX;
            _vBlankSetY = vBlankSetY;
            _vBlankClearY = vBlankClearY;
            _fetchSprite0BaX = fetchSprite0BaX;
            _rasterStartX = rasterStartX;

            _clockHzNum = baseClockHz*8;
            _clockHzDen = dividerClockHz;
        }

        public int ClocksPerRasterValue { get { return _clocksPerRaster; } }
        public int RastersPerFrameValue { get { return _rastersPerFrame; } }
        public int HBlankSetXValue { get { return _hBlankSetX; } }
        public int HBlankClearXValue { get { return _hBlankClearX; } }
        public int RasterStartXValue { get { return _rasterStartX; } }
        public int VBlankSetYValue { get { return _vBlankSetY; } }
        public int VBlankClearYValue { get { return _vBlankClearY; } }
        public int ClockHzNumValue { get { return _clockHzNum; } }
        public int ClockHzDenValue { get { return _clockHzDen; } }

        public Expression ClocksPerRaster { get { return Expression.Constant(_clocksPerRaster);} }
        public Expression RastersPerFrame { get { return Expression.Constant(_rastersPerFrame); } }
        public Expression HBlankSetX { get { return Expression.Constant(_hBlankSetX); } }
        public Expression HBlankClearX { get { return Expression.Constant(_hBlankClearX); } }
        public Expression VBlankSetY { get { return Expression.Constant(_vBlankSetY); } }
        public Expression VBlankClearY { get { return Expression.Constant(_vBlankClearY); } }
        public Expression FetchSprite0BaX { get { return Expression.Constant(_fetchSprite0BaX); } }
        public Expression RasterStartX { get { return Expression.Constant(_rasterStartX); } }

        public Expression MacToCounterX(int mac)
        {
            var result = _fetchSprite0BaX + (mac * 4);
            while (result < 0)
            {
                result += _clocksPerRaster;
            }
            while (result >= _clocksPerRaster)
            {
                result -= _clocksPerRaster;
            }
            return Expression.Constant(result);
        }

        public Expression RasterXToCounterX(int rasterX)
        {
            while (rasterX < 0)
            {
                rasterX += _clocksPerRaster;
            }

            while (rasterX >= _clocksPerRaster)
            {
                rasterX -= _clocksPerRaster;
            }

            if (_clocksPerRaster <= 512 || rasterX <= _hBlankSetX)
            {
                return Expression.Constant(rasterX);
            }

            return Expression.Constant(rasterX + (_clocksPerRaster - 512));
        }

        public Expression CycleToCounterX(int cycleNumber, bool rising)
        {
            var result = _rasterStartX + ((cycleNumber - 1)*8) + (rising ? 0 : 4);
            while (result < 0)
            {
                result += _clocksPerRaster;
            }
            while (result >= _clocksPerRaster)
            {
                result -= _clocksPerRaster;
            }
            return Expression.Constant(result);
        }

        public int VideoWidth
        {
            get
            {
                var hBlankStart = _hBlankSetX;
                var hBlankEnd = _hBlankClearX;

                if (hBlankEnd < hBlankStart)
                {
                    hBlankEnd += _clocksPerRaster;
                }

                var blankedPixels = hBlankEnd - hBlankStart;
                return _clocksPerRaster - blankedPixels;
            }
        }

        public int VideoHeight
        {
            get
            {
                var vBlankStart = _vBlankSetY;
                var vBlankEnd = _vBlankClearY;

                if (vBlankEnd < vBlankStart)
                {
                    vBlankEnd += _rastersPerFrame;
                }

                var blankedRasters = vBlankEnd - vBlankStart;
                return _rastersPerFrame - blankedRasters;
            }
        }
    }
}
