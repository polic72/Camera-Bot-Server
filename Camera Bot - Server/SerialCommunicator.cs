using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CBT_P;


namespace Camera_Bot___Server
{
    /// <summary>
    /// Represents a communicator with the Arduino serial. Uses CBT-P.
    /// </summary>
    public class SerialCommunicator
    {
        private SerialPort serialPort;


        /// <summary>
        /// Constructs a serial communicator with the given port name.
        /// </summary>
        /// <param name="portName">The name of the port to connect to.</param>
        /// <remarks>Baud rate is locked to 9600.</remarks>
        public SerialCommunicator(string portName)
        {
            serialPort = new SerialPort(portName, 9600);
        }


        /// <summary>
        /// Sends a command to the Arduino.
        /// </summary>
        /// <param name="command">Command for the Arduino to perform.</param>
        /// <returns>The response sent from Arduino.</returns>
        public Response SendCommand(Command command)
        {
            if (!serialPort.IsOpen)
            {
                serialPort.Open();
            }

            byte[] sending = Encoding.ASCII.GetBytes(command.ToString() + "|");
            serialPort.Write(sending, 0, sending.Length);

            return (Response)Enum.Parse(typeof(Response), serialPort.ReadTo("|"));
        }
    }
}
