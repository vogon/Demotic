using System;
using System.Collections.Generic;
using System.Linq;
using Demotic.Core.ObjectSystem;
using Microsoft.CSharp.RuntimeBinder;

namespace Demotic.Dip
{
    public class MessageRouter<T>
    {
        public MessageRouter()
        {
            _chains = new List<VerifyChain>();
        }

        public void AddVerifier(T routingInfo, Func<dynamic, bool> verifier)
        {
            VerifyChain chain = _chains.FirstOrDefault(c => c.RoutingInfo.Equals(routingInfo));

            if (chain == null)
            {
                chain = new VerifyChain();
                _chains.Add(chain);
                chain.RoutingInfo = routingInfo;
            }

            chain.Verifiers.Add(verifier);
        }

        public T Lookup(DObject msg)
        {
            foreach (VerifyChain chain in _chains)
            {
                bool accept = true;

                foreach (var f in chain.Verifiers)
                {
                    try
                    {
                        if (!f(msg))
                        {
                            accept = false;
                            break;
                        }
                    }
                    catch (RuntimeBinderException)
                    {
                        accept = false;
                        break;
                    }
                }

                if (accept)
                {
                    return chain.RoutingInfo;
                }
            }

            return default(T);
        }

        private class VerifyChain
        {
            public VerifyChain()
            {
                RoutingInfo = default(T);
                Verifiers = new List<Func<dynamic, bool>>();
            }

            public T RoutingInfo { get; set; }
            public List<Func<dynamic, bool>> Verifiers { get; set; }
        }

        private List<VerifyChain> _chains;
    }
}