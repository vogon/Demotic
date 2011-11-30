using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demotic.Core.ObjectSystem
{
    class DString : DObject
    {
        public DString(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }

        private string _value;
    }
}
