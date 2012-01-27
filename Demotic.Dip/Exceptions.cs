using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demotic.Network
{
    public class BadBencodingException : Exception
    {
        public BadBencodingException(string message, params object[] args) :
            base(string.Format(message, args)) { }
    }

    public class NotBencodableException : Exception
    {
        public NotBencodableException(string message) : base(message) { }
    }
}
