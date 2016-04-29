using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Breadbox.Extensions;

namespace Breadbox.Chips.Vic6567
{
    public class BorderUnit
    {
        private bool _mainBorderFlipFlop;
        private bool _verticalBorderFlipFlop;

        private Expression MainBorderFlipFlop
        {
            get { return Util.Member(() => _mainBorderFlipFlop); }
        }

        private Expression VerticalBorderFlipFlop
        {
            get { return Util.Member(() => _verticalBorderFlipFlop); }
        }

        public Expression SetMainBorder
        {
            get { return Expression.Assign(MainBorderFlipFlop, Expression.Constant(true)); }
        }

        public Expression SetVerticalBorder
        {
            get { return Expression.Assign(VerticalBorderFlipFlop, Expression.Constant(true)); }
        }

        public Expression ClearMainBorder
        {
            get { return Expression.Assign(MainBorderFlipFlop, Expression.Constant(false)); }
        }

        public Expression ClearVerticalBorder
        {
            get { return Expression.Assign(VerticalBorderFlipFlop, Expression.Constant(false)); }
        }

        public Expression MuxBorderUnit(Expression inputColor, Expression borderColor)
        {
            return Expression.Condition(Expression.OrElse(MainBorderFlipFlop, VerticalBorderFlipFlop), borderColor, inputColor);
        }
    }
}
