﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Demotic.Core.ObjectSystem;
using Demotic.Network;

namespace Demotic.Server
{
    class TelnetCmdLinePresentation : IPresentation
    {
        public TelnetCmdLinePresentation(IPEndPoint listenEndpoint)
        {
            _server = new TcpListener(listenEndpoint);
        }

        public void Start()
        {
            _server.Start();
        }

        public MessageChannel AwaitNextConnection()
        {
            throw new NotImplementedException();
            //return new TelnetCmdLinePresentation.Client(_server.AcceptTcpClient());
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        private TcpListener _server;
        private static Encoding _DefaultEncoding = Encoding.ASCII;

        //private sealed class Client : IPresentationClient
        //{
        //    internal Client(TcpClient tcpClient)
        //    {
        //        _tcpClient = tcpClient;
        //        _stream = tcpClient.GetStream();
        //        _canceler = new CancellationTokenSource();

        //        Task.Factory.StartNew(RequestServiceTask, _canceler.Token, TaskCreationOptions.LongRunning,
        //            TaskScheduler.Default);

        //        PutMessage("oh hey there");
        //    }

        //    private void RequestServiceTask()
        //    {
        //        StreamReader reader = new StreamReader(_stream, _DefaultEncoding);
        //        LinkedList<string> pendingLines = new LinkedList<string>();

        //        while (true)
        //        {
        //            if (RequestPending != null)
        //            {
        //                string nextLine = reader.ReadLine();

        //                if (nextLine == null)
        //                {
        //                    // next line is null; stream has closed.
        //                    return;
        //                }
        //                else if (!String.IsNullOrWhiteSpace(nextLine))
        //                {
        //                    try
        //                    {
        //                        UserAction a = CommandParser.ParseCommandLine(this, nextLine);

        //                        if (a != null)
        //                        {
        //                            RequestPending(a);
        //                        }
        //                        else
        //                        {
        //                            PutMessage("not implemented");
        //                        }
        //                    }
        //                    catch (CommandParser.ParseErrorException e)
        //                    {
        //                        PutMessage(string.Format("parse error: {0}", e.Message));
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                // make sure not to fire until something has registered to be notified
        //                // on an incoming request, or we could miss messages.
        //                Thread.Sleep(10);
        //            }
        //        }
        //    }

        //    public void PutMessage(string message)
        //    {
        //        WriteResponseString(message + "\r\n\r\n");
        //    }

        //    public void PutObject(DObject obj)
        //    {
        //        if (obj is DNumber)
        //        {
        //            WriteResponseString((obj as DNumber).DoubleValue.ToString() + "\r\n\r\n");
        //        }
        //        else if (obj == null)
        //        {
        //            WriteResponseString("object does not exist.\r\n\r\n");
        //        }
        //    }

        //    private void WriteResponseString(string s)
        //    {
        //        StreamWriter writer = new StreamWriter(_stream, _DefaultEncoding);

        //        writer.Write(s);
        //        writer.Flush();
        //    }

        //    private TcpClient _tcpClient;
        //    private NetworkStream _stream;

        //    private CancellationTokenSource _canceler;

        //    public event MessagePendingEvent RequestPending;
        //}
    }
}
