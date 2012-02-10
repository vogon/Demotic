using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demotic.Core.ObjectSystem
{
    public class DObjectNavigator
    {
        public DObjectNavigator(DObject startAt)
        {
            _root = startAt;
            _slot = null;
        }

        private DObjectNavigator() { }

        public DObject Get()
        {
            return GetChild(_root, _slot);
        }

        private DObject GetChild(DObject root, string slot)
        {
            if (string.IsNullOrEmpty(slot))
            {
                return root;
            }
            else if (root is DRecord)
            {
                return ((DRecord)root).Get(slot);
            }
            else
            {
                return null;
            }
        }

        public void Put(DObject value)
        {
            PutChild(_root, _slot, value);
        }

        private void PutChild(DObject root, string slot, DObject value)
        {
            if (string.IsNullOrEmpty(slot))
            {
                // todo: fix
                throw new InvalidOperationException();
            }
            if (root is DRecord)
            {
                ((DRecord)root).Set(slot, value);
            }
            else
            {
                // todo: fix
                throw new InvalidOperationException();
            }
        }

        public DObjectNavigator Select(string path)
        {
            string remainingPath = path.Trim();

            DObject location = _root;
            string slot = null;

            while (true)
            {
                string nextElement;
                bool endOfPath = NextPathElement(ref remainingPath, out nextElement);

                if (endOfPath)
                {
                    // done.  drop location and slot into a new navigator.
                    return new DObjectNavigator { _root = location, _slot = slot };
                }
                else
                {
                    if (slot == null)
                    {
                        // slot = null -> fill the slot with the next path component.
                        slot = nextElement;
                    }
                    else
                    {
                        // slot != null -> traverse down to slot, if possible (if not,
                        // the path is illegal), then fill slot again.
                        DObject nextRoot = GetChild(location, slot);

                        if (nextRoot == null)
                        {
                            return null;
                        }
                        else
                        {
                            location = nextRoot;
                            slot = nextElement;
                        }
                    }
                }
            }
        }

        private bool NextPathElement(ref string path, out string next)
        {
            // remove leading slashes...
            while (!string.IsNullOrEmpty(path) && path[0] == '/')
            {
                path = path.Substring(1);
            }

            if (string.IsNullOrEmpty(path))
            {
                path = null;
                next = null;
                return true;
            }

            // pull off everything up until the next slash
            int slashIndex = path.IndexOf('/');

            if (slashIndex != -1)
            {
                // still >0 slashes left.
                string thisElement = path.Substring(0, slashIndex);

                path = path.Substring(slashIndex);
                next = thisElement;

                return false;
            }
            else
            {
                // 0 slashes left; return trimmed path and signal end-of-path.
                next = path;
                path = null;

                return false;
            }
        }

        private DObject _root;
        private string _slot;
    }
}
