﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demotic.Network
{
    public enum MessageOpCode
    {
        Quote = 1,
        OK = 2,
        NG = 3,
        GetObject = 4,
        PutObject = 5,
        DoScript = 6
    }
}
