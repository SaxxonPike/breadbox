using System;
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
        private readonly GraphicsGenerator _graphicsGenerator;
        private readonly RasterCounter _rasterCounter;
        private readonly SpriteGenerator _spriteGenerator;
        private readonly Mux _mux;

        protected Package(Config config)
        {
            _config = config;
            _addressGenerator = new AddressGenerator(_state, _config);
            _graphicsGenerator = new GraphicsGenerator(_state, _config);
            _rasterCounter = new RasterCounter(_state, _config);
            _spriteGenerator = new SpriteGenerator(_state, _config);
            _mux = new Mux(_state, _config);
        }

        public Expression Clock(Func<Expression, Expression> readMemory, Func<Expression, Expression> readColorMemory, Expression clockPhi1, Expression clockPhi2)
        {
            var clock2mhz = Util.Void(
                _addressGenerator.Clock(readMemory, readColorMemory)
                );
            var clock8mhz = Util.Void(
                _graphicsGenerator.Clock,
                _spriteGenerator.Clock(0),
                _spriteGenerator.Clock(1),
                _spriteGenerator.Clock(2),
                _spriteGenerator.Clock(3),
                _spriteGenerator.Clock(4),
                _spriteGenerator.Clock(5),
                _spriteGenerator.Clock(6),
                _spriteGenerator.Clock(7)
                );

            return _rasterCounter.Clock(clock8mhz, clock2mhz, clockPhi1, clockPhi2);
        }

        public Expression OutputPixel
        {
            get { return _mux.OutputColor; }
        }
    }
}
