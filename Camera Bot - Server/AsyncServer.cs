using System;
using System.Collections.Concurrent;
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
        protected Socket listener;

        protected Thread command_thread;
        protected BlockingCollection<string> command_queue; //Already has a ConcurrentQueue under the hood.


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
        }


        /// <summary>
        /// Starts the server. Does nothing if the server is already started.
        /// </summary>
        /// <param name="max_connections">The maximum number of connections the server can allow at one time.</param>
        public void StartServer(int max_connections)
        {
            if (!IsRunning)
            {
                //Acceptance thread:
                ThreadStart acceptance_threadStart = new ThreadStart(AcceptLoop);

                listener = new Socket(IPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(endPoint);
                listener.Listen(max_connections);

                acceptance_thread = new Thread(acceptance_threadStart);
                acceptance_thread.Start();


                //Command thread:
                ThreadStart command_threadStart = new ThreadStart(CommandLoop);

                command_queue = new BlockingCollection<string>();

                command_thread = new Thread(command_threadStart);
                command_thread.Start();


                continue_running = true;
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


            StateObject stateObject = new StateObject(handler);

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


            int count = stateObject.Handler.EndReceive(asyncResult);

            if (count > 0)
            {
                stateObject.StoredText.Append(Encoding.ASCII.GetString(stateObject.Buffer));

                whole = stateObject.StoredText.ToString();

                int pos = whole.IndexOf('|');
                if (pos > -1)
                {
                    string command = whole.Substring(0, pos);

                    command_queue.Add(command);

                    stateObject.StoredText.Clear();
                    stateObject.StoredText.Append(whole.Substring(pos + 1));

                    stateObject.ReceiveResetEvent.Set();
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
                string command = command_queue.Take();

                Console.WriteLine("Command: " + command);
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
