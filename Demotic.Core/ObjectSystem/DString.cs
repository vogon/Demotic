using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demotic.Core.ObjectSystem
{
    public class DString : DObject
    {
        public DString(string value)
        {
            _value = value;
        }

        public static implicit operator DString(string s)
        {
            return new DString(s);
        }

        public static implicit operator string(DString s)
        {
            return s.Value;
        }

        public static bool operator ==(DString a, DString b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(DString a, DString b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            if (obj is string)
            {
                return Value.Equals((string)obj);
            }
            else if (obj is DString)
            {
                return Value.Equals(((DString)obj).Value);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override T Accept<T>(IDObjectVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public string Value
        {
            get { return _value; }
        }

        private string _value;
    }
}
