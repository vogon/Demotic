using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demotic.Dip
{
    public class BadBencodingException : Exception
    {
        public BadBencodingException(string message) : base(message) { }
    }

    public class NotBencodableException : Exception
    {
        public NotBencodableException(string message) : base(message) { }
    }
}
