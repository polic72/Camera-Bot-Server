using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

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


        protected IPEndPoint endPoint;


        private bool continue_running = false;
        protected Thread acceptance_thread;

        protected Socket listener;


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
                ThreadStart threadStart = new ThreadStart(AcceptLoop);

                listener = new Socket(IPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(endPoint);
                listener.Listen(max_connections);

                acceptance_thread = new Thread(threadStart);
                acceptance_thread.Start();

                continue_running = true;
            }
        }


        private void AcceptLoop()
        {
            while (continue_running)
            {

            }
        }
    }
}
