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
            //https://docs.microsoft.com/en-us/dotnet/framework/network-programming/synchronous-server-socket-example


            byte[] buffer = new byte[1024];


            //Establishing local endpoint.
            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress IP_address = hostEntry.AddressList[0];
            IPEndPoint local_endPoint = new IPEndPoint(IP_address, 11000);

            IPAddress address = new IPAddress(new byte[] { 10, 0, 0, 68 });

            Socket listener = new Socket(IP_address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(local_endPoint);
                listener.Listen(10);


                while (true)
                {
                    Socket client = listener.Accept();

                    int count = client.Receive(buffer);


                    string test = Encoding.ASCII.GetString(buffer, 0, count);


                    //client.Send()
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
