using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Demotic.Core;
using Demotic.Core.ObjectSystem;
using Demotic.Network;

namespace Demotic.Server
{
    internal delegate void MessagePendingEvent(Message msg);

    internal interface IPresentation
    {
        void Start();

        MessageChannel AwaitNextConnection();

        void Stop();
    }
}
