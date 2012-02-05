using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using W3b.Sine;

namespace Demotic.Core.ObjectSystem
{
    /// <summary>
    ///   An abstract number type for the purpose of Demotic rules.
    /// </summary>
    public class DNumber : DObject
    {
        public DNumber(long value)
        {
            _value = BigFloatFactory.Instance.Create(value);
        }

        public DNumber(double value)
        {
            _value = BigFloatFactory.Instance.Create(value);
        }

        public DNumber(decimal value)
        {
            _value = BigFloatFactory.Instance.Create(value);
        }

        private DNumber(BigNum num)
        {
            _value = num;
        }

        // NOTE: the Roslyn CTP is missing operator overloading support.  (sad face.)
        // these operators are inaccessible from scripts until a later beta or perhaps
        // Dev11 ship.
        public static implicit operator DNumber(decimal dec)
        {
            return new DNumber(dec);
        }

        public static implicit operator DNumber(long i)
        {
            return new DNumber(i);
        }

        public static implicit operator DNumber(double d)
        {
            return new DNumber(d);
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

        public BigNum Value
        {
            get { return _value; }
        }

        //public decimal Value
        //{
        //    get { return _value; }
        //}

        private BigNum _value;
    }
}
