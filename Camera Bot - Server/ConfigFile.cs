using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Camera_Bot___Server
{
    /// <summary>
    /// Represents a config file for storage and usage.
    /// </summary>
    public class ConfigFile
    {
        /// <summary>
        /// The character that represent a commented line.
        /// </summary>
        public const char CommentCharacter = '#';


        #region Properties

        /// <summary>
        /// The regex that matches config lines.
        /// </summary>
        public static Regex ConfigLine_Regex { get; }


        /// <summary>
        /// The path of the file to read/write to.
        /// </summary>
        public string Path { get; }


        /// <summary>
        /// The options recognized by the config file. Can be changed, but must call <see cref="Camera_Bot___Server.ConfigFile.WriteFile"/> 
        /// to sync the data.
        /// </summary>
        public Dictionary<string, string> ConfigOptions { get; }

        #endregion Properties


        #region Constructors

        static ConfigFile()
        {
            ConfigLine_Regex = new Regex(@"^(.+)=(.+)$", RegexOptions.Compiled);
        }


        /// <summary>
        /// Constructs a config file with the given path.
        /// </summary>
        /// <param name="path">The path of the file to read/write to.</param>
        public ConfigFile(string path)
        {
            Path = path;

            ConfigOptions = new Dictionary<string, string>(10);
        }

        #endregion Constructors


        #region File IO

        /// <summary>
        /// Reads the file and updates the options.
        /// </summary>
        /// <remarks>Ignores empty lines and lines that start with the <see cref="Camera_Bot___Server.ConfigFile.CommentCharacter"/>. 
        /// Also ignores any malformed lines.</remarks>
        public void ReadFile()
        {
            try
            {
                string[] lines = File.ReadAllLines(Path);

                foreach (string line in lines)
                {
                    if (line != "" && !line.StartsWith(CommentCharacter.ToString()))
                    {
                        Match match = ConfigLine_Regex.Match(line);

                        if (match.Success)
                        {
                            if (ConfigOptions.ContainsKey(match.Groups[1].Value))
                            {
                                ConfigOptions[match.Groups[1].Value] = match.Groups[2].Value;
                            }
                            else
                            {
                                ConfigOptions.Add(match.Groups[1].Value, match.Groups[2].Value);
                            }
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                //Do nothing.
            }
        }


        /// <summary>
        /// Writes the stored data into the file. If the file already exists, its values are edited.
        /// </summary>
        public void WriteFile()
        {
            if (File.Exists(Path))
            {
                string[] lines = File.ReadAllLines(Path);

                StringBuilder builder = new StringBuilder(lines.Sum(x => x.Length));

                foreach (string line in lines)
                {
                    bool found = false;

                    foreach (string key in ConfigOptions.Keys)
                    {
                        if (line.StartsWith(key))
                        {
                            string new_line = line.Substring(0, line.LastIndexOf('=') + 1) + ConfigOptions[key] + "\r\n";

                            builder.Append(new_line);

                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        builder.Append(line + "\r\n");
                    }
                }


                using (StreamWriter writer = new StreamWriter(Path))
                {
                    writer.Write(builder.ToString());
                }
            }
            else
            {
                using (StreamWriter writer = new StreamWriter(Path))
                {
                    foreach (string key in ConfigOptions.Keys)
                    {
                        writer.WriteLine(key + "=" + ConfigOptions[key]);
                    }
                }
            }
        }

        #endregion File IO
    }
}
