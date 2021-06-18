using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using CBT_P;


namespace Camera_Bot___Server
{
    /// <summary>
    /// Represent an asynchronous socket server.
    /// </summary>
    public class AsyncServer
    {
        #region Constants

        /// <summary>
        /// The minimum number a port can be, inclusive.
        /// </summary>
        public const int MinPort = 1024;

        /// <summary>
        /// The maximum number a port can be, inclusive.
        /// </summary>
        public const int MaxPort = 49151;

        #endregion Constants


        #region Properties

        /// <summary>
        /// The IP address used to run the server.
        /// </summary>
        public IPAddress IPAddress { get; }


        /// <summary>
        /// The port the server is hosted on.
        /// </summary>
        public int Port { get; }


        /// <summary>
        /// Whether or not the server is currently accepting connections.
        /// </summary>
        public bool IsRunning => acceptance_thread?.IsAlive ?? false;

        #endregion Properties


        #region OnAccept Event

        ///// <summary>
        ///// What the server will do when it accepts a connection.
        ///// </summary>
        ///// <param name="stateObject">The state object for this connection.</param>
        //public delegate void OnAcceptHandler(object sender, );

        ///// <summary>
        ///// 
        ///// </summary>
        //public event OnAcceptHandler OnAccept;

        #endregion OnAccept Event


        protected IPEndPoint endPoint;


        private bool continue_running = false;

        protected Thread acceptance_thread;
        protected ManualResetEvent acceptance_waiter = new ManualResetEvent(false);
        protected LinkedList<StateObject> all_stateObjects;
        protected Socket listener;

        protected object Receive_Lock = new object();

        protected Thread command_thread;
        protected BlockingCollection<Command_Object> command_queue; //Already has a ConcurrentQueue under the hood.
        protected SerialCommunicator serial_communicator;


        /// <summary>
        /// Constructs an AsyncServer at the given IPAddress on the given port.
        /// </summary>
        /// <param name="IP_address">IPAddress for the server.</param>
        /// <param name="port">Port of the server. Must be in the range [<see cref="Camera_Bot___Server.AsyncServer.MinPort"/>, 
        /// <see cref="Camera_Bot___Server.AsyncServer.MaxPort"/>].</param>
        /// <exception cref="System.ArgumentOutOfRangeException">When the port is out of range.</exception>
        public AsyncServer(IPAddress IP_address, int port)
        {
            if (port < MinPort || port > MaxPort)
            {
                throw new ArgumentOutOfRangeException("port", "The port is out of range.");
            }


            IPAddress = IP_address;
            Port = port;

            endPoint = new IPEndPoint(IP_address, port);


            all_stateObjects = new LinkedList<StateObject>();


            //TODO remember to add a config setting for the Serial Port Name.

            serial_communicator = new SerialCommunicator("COM3");
        }


        /// <summary>
        /// Starts the server. Does nothing if the server is already started.
        /// </summary>
        /// <param name="max_connections">The maximum number of connections the server can allow at one time.</param>
        public void StartServer(int max_connections)
        {
            if (!IsRunning)
            {
                continue_running = true;


                //Acceptance thread:
                ThreadStart acceptance_threadStart = new ThreadStart(AcceptLoop);

                if (!listener?.IsBound ?? true)
                {
                    listener = new Socket(IPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    listener.Bind(endPoint);
                }
                listener.Listen(max_connections);

                acceptance_thread = new Thread(acceptance_threadStart);
                acceptance_thread.Start();


                //Command thread:
                ThreadStart command_threadStart = new ThreadStart(CommandLoop);

                command_queue = new BlockingCollection<Command_Object>();

                command_thread = new Thread(command_threadStart);
                command_thread.Start();
            }
        }


        /// <summary>
        /// The loop that will accept connections until the server is stopped.
        /// </summary>
        private void AcceptLoop()
        {
            while (continue_running)
            {
                acceptance_waiter.Reset();

                listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                acceptance_waiter.WaitOne();
            }
        }


        /// <summary>
        /// The method run whenever a connection is accepted.
        /// </summary>
        /// <param name="asyncResult">The result of accepting asynchronously.</param>
        private void AcceptCallback(IAsyncResult asyncResult)
        {
            acceptance_waiter.Set();

            Socket safe_listener = (Socket)asyncResult.AsyncState;
            Socket handler = safe_listener.EndAccept(asyncResult);


            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Connected");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" to [");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(((IPEndPoint)handler.RemoteEndPoint).Address.ToString());

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("].\r\n");


            //System.Diagnostics.Process process = System.Diagnostics.Process.GetCurrentProcess();
            //Console.WriteLine(process.Threads.Count.ToString() + "\r\n\r\n");


            StateObject stateObject = new StateObject(handler);
            all_stateObjects.AddLast(stateObject);

            while (stateObject.KeepAlive)
            {
                stateObject.ReceiveResetEvent.Reset();

                handler.BeginReceive(stateObject.Buffer, 0, stateObject.Buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), stateObject);

                stateObject.ReceiveResetEvent.WaitOne();
            }
        }


        /// <summary>
        /// The method run whenever the server receives data.
        /// </summary>
        /// <param name="asyncResult">The result of receiving asynchronously.</param>
        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            string whole;

            StateObject stateObject = (StateObject)asyncResult.AsyncState;


            int count = 0;
            try
            {
                count = stateObject.Handler.EndReceive(asyncResult);
            }
            catch (ObjectDisposedException)
            {
                //Server is shutting down, do nothing.
            }

            if (count > 0)
            {
                stateObject.StoredText.Append(Encoding.ASCII.GetString(stateObject.Buffer, 0, count));

                whole = stateObject.StoredText.ToString(0, stateObject.StoredText.Length);

                int pos = whole.IndexOf('|');
                if (pos > -1)
                {
                    string command = whole.Substring(0, pos);

                    stateObject.StoredText.Clear();
                    stateObject.StoredText.Append((whole != "") ? whole.Substring(pos + 1) : "");


                    Command_Object command_Object = new Command_Object(command, stateObject);
                    command_queue.Add(command_Object);

                    //stateObject.ReceiveResetEvent.Set();
                }
                else
                {
                    stateObject.ReceiveResetEvent.Set();
                }
            }
        }


        /// <summary>
        /// The loops that checks the queue for commands and performs them ASAP.
        /// </summary>
        private void CommandLoop()
        {
            while (continue_running)
            {
                Command_Object command_Object = command_queue.Take();

                if (command_Object == null)
                {
                    continue;
                }

                try
                {
                    Command command = (Command)Enum.Parse(typeof(Command), command_Object.Command);

                    switch (command)
                    {
                        #region Old
                        //#region Movement Start

                        //case Command.Up:
                        //    try
                        //    {
                        //        SendResponse(command_Object.StateObject.Handler, Response.Ok);


                                
                        //    }
                        //    catch (SocketException)
                        //    {
                        //        Disconnect(command_Object.StateObject);
                        //    }
                        //    break;

                        //#endregion Movement Start


                        //#region Movement End

                        //case Command.StopUp:
                        //    try
                        //    {
                        //        SendResponse(command_Object.StateObject.Handler, Response.Ok);


                        //        //
                        //    }
                        //    catch (SocketException)
                        //    {
                        //        Disconnect(command_Object.StateObject);
                        //    }
                        //    break;

                        //#endregion Movement End
                        #endregion Old


                        case Command.Disconnect:
                            try
                            {
                                SendResponse(command_Object.StateObject.Handler, Response.Goodbye);
                            }
                            catch (SocketException)
                            {
                                //Do nothing, they're already gone...
                            }

                            Disconnect(command_Object.StateObject);
                            break;


                        default:
                            try
                            {
                                SendResponse(command_Object.StateObject.Handler, Response.Ok);

                                Response response = serial_communicator.SendCommand(command);
                            }
                            catch (SocketException)
                            {
                                Disconnect(command_Object.StateObject);
                            }
                            break;
                    }
                }
                catch (ArgumentException)
                {
                    try
                    {
                        SendResponse(command_Object.StateObject.Handler, Response.Bad);
                    }
                    catch (SocketException)
                    {
                        Disconnect(command_Object.StateObject);
                    }
                }
                finally
                {
                    command_Object.StateObject.ReceiveResetEvent.Set();
                }


                Console.WriteLine(command_Object.Command + "\r\n");
            }
        }


        #region Helpers

        /// <summary>
        /// Sends a response to the given client.
        /// </summary>
        /// <param name="client_handler">The socket that communicates with a client.</param>
        /// <param name="response">The response to send to the client.</param>
        private void SendResponse(Socket client_handler, Response response)
        {
            client_handler.Send(Encoding.ASCII.GetBytes(response.ToString() + "|"));
        }


        /// <summary>
        /// Gracefully disconnects the server from the client state object.
        /// </summary>
        /// <param name="stateObject">The client's state object.</param>
        private void Disconnect(StateObject stateObject)
        {
            stateObject.Handler.Shutdown(SocketShutdown.Both);
            stateObject.Handler.Close();

            stateObject.KeepAlive = false;

            all_stateObjects.Remove(stateObject);
        }

        #endregion Helpers


        /// <summary>
        /// Gracefully stops the server.
        /// </summary>
        public void StopServer()
        {
            LinkedListNode<StateObject> node = all_stateObjects.First;

            while (node != null)
            {
                LinkedListNode<StateObject> temp = node.Next;

                Disconnect(node.Value);

                node.Value.ReceiveResetEvent.Set();

                node = temp;
            }


            continue_running = false;

            acceptance_waiter.Set();
            command_queue.Add(null);
        }


        /// <summary>
        /// Disposes of the data in the server. Stops the server if it is running, and it can never be started again.
        /// </summary>
        public void Close()
        {
            if (continue_running)
            {
                StopServer();
            }


            try
            {
                listener.Shutdown(SocketShutdown.Both);
                listener.Close();
            }
            catch (SocketException)
            {
                //Someone was trying to do something, too bad.
            }
        }
    }


    ///// <summary>
    ///// The event that will happen
    ///// </summary>
    //public class OnAcceptEventArgs
    //{

    //}
}
