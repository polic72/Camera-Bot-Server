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
            AsyncServer server = new AsyncServer(new IPAddress(new byte[] { 10, 0, 0, 68 }), 11000);

            server.StartServer(5);




            //string start = "Up|";

            //StringBuilder stringBuilder = new StringBuilder(start);
            //string whole = stringBuilder.ToString();

            //int pos = whole.IndexOf('|');

            //if (pos > -1)
            //{
            //    string command = whole.Substring(0, pos);

            //    whole = whole.Substring(pos + 1);
            //}




            ////https://docs.microsoft.com/en-us/dotnet/framework/network-programming/synchronous-server-socket-example


            //byte[] buffer = new byte[1024];

            //int counter = 0;


            ////Establishing local endpoint.
            //IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            //IPAddress IP_address = hostEntry.AddressList[0];

            ////IPAddress IP_address = new IPAddress(new byte[] { 10, 0, 0, 68 });
            //IPEndPoint local_endPoint = new IPEndPoint(IP_address, 11000);

            //Socket listener = new Socket(IP_address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            //try
            //{
            //    listener.Bind(local_endPoint);
            //    listener.Listen(10);


            //    while (true)
            //    {
            //        Socket client = listener.Accept();

            //        while (client.Connected)
            //        {
            //            int count = -1;
            //            try
            //            {
            //                count = client.Receive(buffer);
            //            }
            //            catch (SocketException)
            //            {
            //                break;
            //            }


            //            string command = Encoding.ASCII.GetString(buffer, 0, count);

            //            switch (command)
            //            {
            //                case "Up":
            //                    client.Send(Encoding.ASCII.GetBytes("Ok"));
            //                    Console.WriteLine(counter++);
            //                    break;


            //                case "Down":
            //                    //
            //                    break;


            //                case "Left":
            //                    //
            //                    break;


            //                case "Right":
            //                    //
            //                    break;


            //                case "Break":
            //                    client.Send(Encoding.ASCII.GetBytes("Goodbye"));

            //                    client.Shutdown(SocketShutdown.Both);
            //                    client.Close();


            //                    //listener = new Socket(IP_address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            //                    //listener.Bind(local_endPoint);
            //                    //listener.Listen(10);
            //                    break;


            //                default:
            //                    client.Send(Encoding.ASCII.GetBytes("Bad"));
            //                    continue;
            //            }
            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.ToString());
            //}
        }
    }
}
