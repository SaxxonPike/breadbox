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

        public Config(
            int clocksPerRaster,
            int rastersPerFrame,
            int hBlankSetX,
            int hBlankClearX,
            int vBlankSetY,
            int vBlankClearY,
            int fetchSprite0BaX,
            int rasterStartX
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
        }

        public int ClocksPerRasterValue { get { return _clocksPerRaster; } }
        public int RastersPerFrameValue { get { return _rastersPerFrame; } }
        public int HBlankSetXValue { get { return _hBlankSetX; } }

        public Expression ClocksPerRaster { get { return Expression.Constant(_clocksPerRaster);} }
        public Expression RastersPerFrame { get { return Expression.Constant(_rastersPerFrame); } }
        public Expression HBlankSetX { get { return Expression.Constant(_hBlankSetX); } }
        public Expression HBlankClearX { get { return Expression.Constant(_hBlankClearX); } }
        public Expression VBlankSetY { get { return Expression.Constant(_vBlankSetY); } }
        public Expression VBlankClearY { get { return Expression.Constant(_vBlankClearY); } }
        public Expression FetchSprite0BaX { get { return Expression.Constant(_fetchSprite0BaX); } }
        public Expression RasterStartX { get { return Expression.Constant(_rasterStartX); } }

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
