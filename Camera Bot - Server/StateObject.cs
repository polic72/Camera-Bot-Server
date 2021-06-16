using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Camera_Bot___Server
{
    /// <summary>
    /// A small data class to ease connection between different threads of a <see cref="Camera_Bot___Server.AsyncServer"/>.
    /// </summary>
    public class StateObject
    {
        /// <summary>
        /// The size to use for the internal buffer.
        /// </summary>
        public const int BufferSize = 1024;


        #region Properties

        /// <summary>
        /// The internal buffer of the state object.
        /// </summary>
        public byte[] Buffer { get; }


        /// <summary>
        /// A stringbuilder to store translated buffers, if needed.
        /// </summary>
        public StringBuilder StoredText { get; }


        ///// <summary>
        ///// A list to add stored buffers to, if they matter.
        ///// </summary>
        //public List<byte[]> StoredBuffers = new List<byte[]>();

        
        /// <summary>
        /// The handler socket shared to send and receive data.
        /// </summary>
        public Socket Handler { get; }


        /// <summary>
        /// Whether or not the connection should keep accepting data.
        /// </summary>
        public bool KeepAlive { get; set; }


        /// <summary>
        /// The reset event for waiting for receives.
        /// </summary>
        public ManualResetEvent ReceiveResetEvent { get; }

        #endregion Properties


        /// <summary>
        /// Constructs a state object with the given handler socket.
        /// </summary>
        /// <param name="handler">The socket that represents the client.</param>
        public StateObject(Socket handler)
        {
            Handler = handler;


            Buffer = new byte[BufferSize];
            StoredText = new StringBuilder();

            KeepAlive = true;

            ReceiveResetEvent = new ManualResetEvent(false);
        }
    }
}
