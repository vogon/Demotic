using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demotic.Network
{
    public interface IMessageFormat
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="bytesConsumed"></param>
        /// <exception cref="System.FormatException">
        ///     if the encoding of payload is incorrect.
        /// </exception>
        /// <returns>
        ///     A Message object representing the message in payload, or null if
        ///     the encoding of the payload is otherwise valid but terminates prematurely.
        /// </returns>
        Message Decode(byte[] payload, out int bytesConsumed);
        byte[] Encode(Message msg);
    }
}
