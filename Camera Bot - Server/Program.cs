using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace Camera_Bot___Server
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] buffer = new byte[1024];


            //Establishing local endpoint.
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress IP_address = hostEntry.AddressList[0];
            IPEndPoint local_endPoint = new IPEndPoint(IP_address, 11000);

            //https://docs.microsoft.com/en-us/dotnet/framework/network-programming/synchronous-server-socket-example
        }
    }
}
