using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// https://raw.githubusercontent.com/Microsoft/AppModelSamples/master/Samples/TestCmdLine/TestCmdLine/CommandLineParser.cs
/// </summary>
namespace DiffWit.Utils
{
    public class ParsedCommands : List<KeyValuePair<string, string>> { }

    public class CommandLineParser
    {
        public static List<KeyValuePair<string, string>> ParsedArgs { get; private set; }

        public static void Parse(string argString = null)
        {
            string[] args = argString.Split(' ');
            if (ParsedArgs == null)
            {
                ParsedArgs = new List<KeyValuePair<string, string>>();
            }
            else
            {
                ParsedArgs.Clear();
            }

            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].StartsWith("-") || args[i].StartsWith("/"))
                    {
                        var data = ParseData(args, i);
                        if (data.Key != null)
                        {
                            for (int j = 0; j < ParsedArgs.Count; j++)
                            {
                                if (ParsedArgs[j].Key == data.Key)
                                {
                                    ParsedArgs.RemoveAt(j);
                                }
                            }
                            ParsedArgs.Add(data);
                        }
                    }
                }
            }
        }

        private static KeyValuePair<string, string> ParseData(string[] args, int index)
        {
            string key = null;
            string val = null;
            if (args[index].StartsWith("-") || args[index].StartsWith("/"))
            {
                if (args[index].Contains(":"))
                {
                    string argument = args[index];
                    int endIndex = argument.IndexOf(':');
                    key = argument.Substring(1, endIndex - 1);   // trim the '/' and the ':'.
                    int valueStart = endIndex + 1;
                    val = valueStart < argument.Length ? argument.Substring(
                        valueStart, argument.Length - valueStart) : null;
                }
                else
                {
                    key = args[index];
                    int argIndex = 1 + index;
                    if (argIndex < args.Length && !(args[argIndex].StartsWith("-") || args[argIndex].StartsWith("/")))
                    {
                        val = args[argIndex];
                    }
                    else
                    {
                        val = null;
                    }
                }
            }

            return key != null ? new KeyValuePair<string, string>(key, val) : default(KeyValuePair<string, string>);
        }

        public static bool IsFiniteOperation;

        public static ParsedCommands ParseUntrustedArgs(string cmdLineString)
        {
            ParsedCommands commands = new ParsedCommands();
            Parse(cmdLineString);

            foreach (KeyValuePair<string, string> kvp in ParsedArgs)
            {
                commands.Add(kvp);
            }
            return commands;
        }
    }
}
