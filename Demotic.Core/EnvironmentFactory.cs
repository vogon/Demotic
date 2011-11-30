using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Demotic.Core.ObjectSystem;

namespace Demotic.Core
{
    public static class EnvironmentFactory
    {
        static EnvironmentFactory()
        {
            CommonRoot = new DRecord();
            CommonRoot["test"] = new DNumber(3);
        }

        public static DRecord CommonRoot { get; private set; }
    }
}
