using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demotic.Client
{
    internal abstract class DemoticViewModel
    {
        public DemoticViewModel(Client c)
        {
            Client = c;
        }

        protected Client Client { get; set; }
    }
}
