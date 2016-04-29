﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Breadbox.Interfaces
{
    public interface IWriteable
    {
        Expression GetWriteExpression(Expression value);
    }
}
