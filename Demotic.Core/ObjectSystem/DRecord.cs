using System;
using System.Collections.Generic;
using System.Dynamic;
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

        // NOTE: Roslyn CTP coerces "dynamic" fields down to "object", and doesn't support
        // the dynamic type, breaking dynamic binding.  dynamic binding is inaccessible from
        // scripts until a later beta or Dev11 ship.
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_values.ContainsKey(binder.Name))
            {
                result = _values[binder.Name];
                return true;
            }
            else
            {
                object realMember = null;
                bool realMemberExists = base.TryGetMember(binder, out realMember);

                if (realMemberExists)
                {
                    result = realMember;
                }
                else
                {
                    result = null;
                }

                return true;
            }
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

        public Dictionary<string, DObject> Values
        {
            get
            {
                return _values;
            }
        }

        private Dictionary<string, DObject> _values;
    }
}
