using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demotic.Server
{
    interface ITransport<T>
    {
        bool DataAvailable { get; }

        int Read(T[] buffer, int offset, int count);
        void Write(T[] buffer, int offset, int count);
    }
}
