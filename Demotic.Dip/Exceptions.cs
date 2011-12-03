using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demotic.Dip
{
    class BadBencodingException : Exception
    {
        public BadBencodingException(string message) : base(message) { }
    }

    class NotBencodableException : Exception
    {
        public NotBencodableException(string message) : base(message) { }
    }
}
