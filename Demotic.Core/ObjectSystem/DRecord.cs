using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demotic.Core.ObjectSystem
{
    public class DRecord : DObject
    {
        public DRecord()
        {
            _values = new Dictionary<string, DObject>();
        }

        public DObject this[string name]
        {
            get
            {
                DObject child;

                _values.TryGetValue(name, out child);

                return child;
            }
            set
            {
                _values[name] = value;
            }
        }

        public DObject Get(string name)
        {
            return this[name];
        }

        public void Set(string name, DObject value)
        {
            this[name] = value;
        }

        private Dictionary<string, DObject> _values;
    }
}
