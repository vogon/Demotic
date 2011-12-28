using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demotic.Core.ObjectSystem
{
    public interface IDObjectVisitor<T>
    {
        public T Visit(DRecord rec);
        public T Visit(DString str);
        public T Visit(DNumber num);
    }
}
