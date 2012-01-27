using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demotic.Core.ObjectSystem
{
    public interface IDObjectVisitor<T>
    {
        T Visit(DRecord rec);
        T Visit(DString str);
        T Visit(DNumber num);
    }
}
