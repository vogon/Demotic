using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Dynamic;

namespace Demotic.Core.ObjectSystem
{
    public abstract class DObject : DynamicObject
    {
        public abstract T Accept<T>(IDObjectVisitor<T> visitor);
    }
}
