using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace Camera_Bot___Server
{
    class Program
    {
        public const string CONFIG_FILE = "server.config";


        public const string IPV4 = "IPv4";
        public const string PORT = "Port";
        public const string SERIAL_PORT_NAME = "SerialPortName";

        static void Main(string[] args)
        {
            ConfigFile configFile = new ConfigFile(CONFIG_FILE);

            configFile.ConfigOptions.Add(IPV4, "");
            configFile.ConfigOptions.Add(PORT, "11000");
            configFile.ConfigOptions.Add("SerialPortName", "");


            if (!File.Exists(CONFIG_FILE))
            {
                configFile.WriteFile();

                return;
            }


            configFile.ReadFile();

            //Check for correct config options:
            //if (configFile.ConfigOptions[IPV4])
            //{

            //}


            //Actually start server with them.
            //Optionally make a loop to type commands in the terminal.


            AsyncServer server = new AsyncServer(new IPAddress(new byte[] { 192, 168, 50, 108 }), 11000);    //25, 92, 246, 145

            server.StartServer(5);
        }
    }
}
