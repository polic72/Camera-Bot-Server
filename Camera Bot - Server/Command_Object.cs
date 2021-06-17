using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


namespace Camera_Bot___Server
{
    /// <summary>
    /// Represents a the necessary data a command needs to be executed.
    /// </summary>
    public class Command_Object
    {
        /// <summary>
        /// The command word to be processed.
        /// </summary>
        public string Command { get; }


        /// <summary>
        /// The state object that contains the relevant information for command execution.
        /// </summary>
        public StateObject StateObject { get; }


        /// <summary>
        /// Constructs a command object with the given state object.
        /// </summary>
        /// <param name="command">The command word to be processed.</param>
        /// <param name="stateObject">The state object with the relevant information for command execution.</param>
        public Command_Object(string command, StateObject stateObject)
        {
            Command = command;
            StateObject = stateObject;
        }
    }
}
