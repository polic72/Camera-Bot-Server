using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Camera_Bot___Server
{
    public class StateObject
    {
        /// <summary>
        /// The size to use for the internal buffer.
        /// </summary>
        public const int BufferSize = 1024;


        /// <summary>
        /// The internal buffer of the state object.
        /// </summary>
        public byte[] Buffer => new byte[1024];


        /// <summary>
        /// A stringbuilder to store translated buffers, if needed.
        /// </summary>
        public StringBuilder StoredText = new StringBuilder();


        ///// <summary>
        ///// A list to add stored buffers to, if they matter.
        ///// </summary>
        //public List<byte[]> StoredBuffers = new List<byte[]>();

        
        /// <summary>
        /// The socket shared to send and receive data.
        /// </summary>
        public Socket Socket = null;
    }
}
