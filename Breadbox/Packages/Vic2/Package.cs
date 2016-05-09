﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Breadbox.Packages.Vic2
{
    public abstract class Package
    {
        private readonly Config _config;
        private readonly State _state = new State();
        private readonly AddressGenerator _addressGenerator;
        private readonly RasterCounter _rasterCounter;
        private readonly Mux _mux;
        private readonly VideoBuffer _videoBuffer;

        protected Package(Config config)
        {
            _config = config;
            _addressGenerator = new AddressGenerator(_state, _config);
            _rasterCounter = new RasterCounter(_state, _config);
            _mux = new Mux(_state, _config);
            _videoBuffer = new VideoBuffer(_state.HBLANK, _state.VBLANK, _config.VideoWidth, _config.VideoHeight);
        }

        public Expression Clock(Func<Expression, Expression> readMemory, Func<Expression, Expression> readColorMemory, Expression clockPhi1, Expression clockPhi2)
        {
            var clock2mhz = Util.Void(
                _addressGenerator.Clock(readMemory, readColorMemory)
                );

            return _rasterCounter.Clock(clock2mhz, clockPhi1, clockPhi2, _videoBuffer.Write(_mux.OutputColor));
        }

        public Expression Frame(Func<Expression, Expression> readMemory, Func<Expression, Expression> readColorMemory,
            Expression clockPhi1, Expression clockPhi2)
        {
            return Util.Repeat(_config.ClocksPerRasterValue*_config.RastersPerFrameValue,
                Clock(readMemory, readColorMemory, clockPhi1, clockPhi2));
        }
    }
}
