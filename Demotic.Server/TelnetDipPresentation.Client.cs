﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Demotic.Core.ObjectSystem;
using Demotic.Dip;

namespace Demotic.Server
{
    partial class TelnetDipPresentation
    {
        private sealed class Client : IPresentationClient
        {
            internal Client(TcpClient tcpClient)
            {
                _tcpClient = tcpClient;
                _stream = tcpClient.GetStream();
                _canceler = new CancellationTokenSource();

                Task.Factory.StartNew(RequestServiceTask, _canceler.Token, TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);

                PutMessage("oh hey there");
            }

            private void RequestServiceTask()
            {
                StreamReader reader = new StreamReader(_stream, _DefaultEncoding);
                LinkedList<string> pendingLines = new LinkedList<string>();

                while (true)
                {
                    if (RequestPending != null)
                    {
                        string nextLine = reader.ReadLine();

                        if (nextLine == null)
                        {
                            // next line is null; stream has closed.
                            return;
                        }
                        else if (!String.IsNullOrWhiteSpace(nextLine))
                        {
                            DObject o = DObjectAdapter.Decode(Encoding.ASCII.GetBytes(nextLine));
                            var action = UserAction.Make(this, o);

                            if (action != null)
                            {
                                RequestPending(action);
                            }
                            else
                            {
                                PutMessage("hey idk what you're getting at");
                            }
                        }
                    }
                    else
                    {
                        // make sure not to fire until something has registered to be notified
                        // on an incoming request, or we could miss messages.
                        Thread.Sleep(10);
                    }
                }
            }

            public void PutMessage(string message)
            {
                WriteDipMessage((DString)message);
            }

            public void PutObject(DObject obj)
            {
                if (obj == null)
                {
                    PutMessage("object does not exist.");
                }
                else
                {
                    WriteDipMessage(obj);
                }
            }

            private void WriteDipMessage(DObject o)
            {
                StreamWriter writer = new StreamWriter(_stream, _DefaultEncoding);

                writer.Write(new string(Encoding.ASCII.GetChars(DObjectAdapter.Encode(o))) + "\r\n");
                writer.Flush();
            }

            private TcpClient _tcpClient;
            private NetworkStream _stream;

            private CancellationTokenSource _canceler;

            public event RequestPendingEvent RequestPending;
        }
    }
}
