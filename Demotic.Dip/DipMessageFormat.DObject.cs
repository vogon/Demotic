using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Demotic.Core.ObjectSystem;

namespace Demotic.Network
{
    public partial class DipMessageFormat : IMessageFormat
    {
        private void SerializeDString(Bencoder e, DString s)
        {
            e.StartDString().ByteString(TextEncoding.GetBytes(s.Value)).FinishDString();
        }

        private void SerializeDNumber(Bencoder e, DNumber n)
        {
            e.StartDNumber().Number(n.Value).FinishDNumber();
        }

        private void SerializeDRecord(Bencoder e, DRecord rec)
        {
            e.StartDRecord();

            foreach (var kvp in rec.Values)
            {
                e.ByteString(TextEncoding.GetBytes(kvp.Key));

                if (kvp.Value is DNumber)
                {
                    SerializeDNumber(e, (DNumber)kvp.Value);
                }
                else if (kvp.Value is DString)
                {
                    SerializeDString(e, (DString)kvp.Value);
                }
                else /* kvp.Value is DRecord */
                {
                    Debug.Assert(kvp.Value is DRecord);
                    SerializeDRecord(e, (DRecord)kvp.Value);
                }
            }

            e.FinishDRecord();
        }
    }
}