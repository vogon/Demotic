using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Demotic.Core.ObjectSystem;

namespace Demotic.Network
{
    public struct Message
    {
        public int SequenceNumber { get; set; }
        public int AckNumber { get; set; }

        public MessageOpCode OpCode { get; set; }

        public Dictionary<string, object> Attributes { get; set; }

        public override string ToString()
        {
            return string.Format("{{ op = {0}; seq = {1}; to = {2} }}",
                OpCode, SequenceNumber, AckNumber);
        }
    }
}
