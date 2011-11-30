using System;
using System.Collections.Generic;
using System.Linq;
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
        public DNumber(int intValue)
        {
            _value = intValue;
        }

        public DNumber(double doubleValue)
        {
            _value = (decimal)doubleValue;
        }

        public int IntValue
        {
            get { return (int)_value; }
        }

        public double DoubleValue
        {
            get { return (double)_value; }
        }

        private decimal _value;
    }
}
