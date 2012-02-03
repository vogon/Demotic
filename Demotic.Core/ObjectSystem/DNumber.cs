using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Demotic.Core.ObjectSystem
{
    /// <summary>
    ///   An abstract number type for the purpose of Demotic rules.
    /// </summary>
    /// <remarks>
    ///   Ultimately there should be no mention of integers or doubles or any "real-world"
    ///   number type; we should pull in an arbitrary-precision numerics library that's
    ///   as close to accurately representing all of R as possible, and make it possible to use
    ///   it as you would any other number.
    /// </remarks>
    public class DNumber : DObject
    {
        public DNumber(decimal decValue)
        {
            _value = decValue;
        }

        // NOTE: the Roslyn CTP is missing operator overloading support.  (sad face.)
        // these operators are inaccessible from scripts until a later beta or perhaps
        // Dev11 ship.
        public static implicit operator DNumber(decimal dec)
        {
            return new DNumber(dec);
        }

        public static implicit operator DNumber(int i)
        {
            return new DNumber(i);
        }

        public static implicit operator DNumber(double d)
        {
            return new DNumber((decimal)d);
        }

        public static explicit operator int(DNumber dn)
        {
            return (int)dn.Value;
        }

        public static explicit operator double(DNumber dn)
        {
            return (double)dn.Value;
        }

        public static DNumber operator -(DNumber a)
        {
            return new DNumber(-a._value);
        }

        public static DNumber operator +(DNumber a)
        {
            return a;
        }

        public static bool operator >=(DNumber a, DNumber b)
        {
            return a._value >= b._value;
        }

        public static bool operator <=(DNumber a, DNumber b)
        {
            return a._value <= b._value;
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public decimal Value
        {
            get { return _value; }
        }

        private decimal _value;
    }
}
