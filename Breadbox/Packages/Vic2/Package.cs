using System;
using System.Linq.Expressions;

namespace Breadbox.Packages.Vic2
{
    public abstract class Package
    {
        private readonly Config _config;
        private readonly State _state;
        private readonly RasterCounter _rasterCounter;
        private readonly Mux _mux;
        private readonly VideoBuffer _videoBuffer;

        protected Package(Config config)
        {
            _config = config;
            _state = new State(_config);
            _rasterCounter = new RasterCounter(_state, _config);
            _mux = new Mux(_state, _config);
            _videoBuffer = new VideoBuffer(_state.HBLANK, _state.VBLANK, _config.VideoWidth, _config.VideoHeight);
        }

        public Expression Clock(Func<Expression, Expression> readMemory, Func<Expression, Expression> readColorMemory, Expression clockPhi1, Expression clockPhi2)
        {
            return _rasterCounter.Clock(readMemory, readColorMemory, clockPhi1, clockPhi2, _videoBuffer.Write(_mux.OutputColor));
        }

        public Expression Clock(int clocks, Func<Expression, Expression> readMemory,
            Func<Expression, Expression> readColorMemory, Expression clockPhi1, Expression clockPhi2)
        {
            return Util.Repeat(clocks, Clock(readMemory, readColorMemory, clockPhi1, clockPhi2));
        }

        public Expression Frame(Func<Expression, Expression> readMemory, Func<Expression, Expression> readColorMemory,
            Expression clockPhi1, Expression clockPhi2)
        {
            return Clock(CyclesPerFrame, readMemory, readColorMemory, clockPhi1, clockPhi2);
        }

        public int CyclesPerFrame
        {
            get { return _config.ClocksPerRasterValue * _config.RastersPerFrameValue; }
        }

        public int CyclesPerSecond
        {
            get { return _config.ClockHzNumValue/_config.ClockHzDenValue; }
        }
    }
}
