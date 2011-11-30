using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Demotic.Core;
using Demotic.Core.ObjectSystem;

namespace Demotic.Server
{
    internal delegate void RequestPendingEvent(UserAction action);

    internal interface IPresentation
    {
        void Start();

        IPresentationClient AwaitNextConnection();

        void Stop();
    }

    internal interface IPresentationClient
    {
        event RequestPendingEvent RequestPending;

        void PutMessage(string message);

        void PutObject(DObject obj);
    }
}
