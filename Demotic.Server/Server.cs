using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace Demotic.Server
{
    internal sealed class Server
    {
        public Server()
        {
            _personalities = new List<IPresentation>();
            _clients = new List<IPresentationClient>();
            _worker = new Worker();

            // TODO: hardcoded for now; add configuration
            IPEndPoint telnet = new IPEndPoint(IPAddress.Any, 17717);
            _personalities.Add(new TelnetCmdLinePresentation(telnet));
        }

        private void AcceptLoop(IPresentation personality)
        {
            Console.WriteLine("in accept loop");

            while (true)
            {
                IPresentationClient newContext = personality.AwaitNextConnection();
                Console.WriteLine("accepted");
                _clients.Add(newContext);
                // TODO: we're going to need to maintain a list of "canonical" clients for each personality at some point
                newContext.RequestPending += ((req) => _worker.Dispatch(req, null));
            }
        }

        public void Start()
        {
            foreach (IPresentation p in _personalities)
            {
                p.Start();

                Task.Factory.StartNew(() => AcceptLoop(p), TaskCreationOptions.LongRunning);
            }

            _worker.Start();
        }

        public void Stop()
        {
            foreach (IPresentation p in _personalities)
            {
                p.Stop();
            }

            _worker.Stop();
        }

        private List<IPresentation> _personalities;
        private List<IPresentationClient> _clients;

        private Worker _worker;
    }

}
