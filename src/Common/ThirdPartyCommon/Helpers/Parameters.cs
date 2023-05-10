// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Text;
using Crestron.Panopto.Common.ExtensionMethods;

namespace Crestron.Panopto.Common.Helpers
{
    public static class ParameterHelper
    {
        public static Parameters GetFirstValidParameter(Commands Command)
        {
            if (Command != null)
            {
                foreach (Parameters parameter in Command.Parameters)
                {
                    if (parameter.Id.Equals("id", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    return parameter;
                }
            }
            return null;
        }

        public static string ReplaceParameter(string Command, string Parameter, string NewValue)
        {
            if (!String.IsNullOrEmpty(Command))
            {
                if (Command.Contains(Parameter))
                {
                    return Command.Substring(0, Command.IndexOf(Parameter)) +
                        NewValue +
                        Command.Substring(Command.IndexOf(Parameter) + Parameter.Length,
                        Command.Length - Command.Substring(0, Command.IndexOf(Parameter)).Length - Parameter.Length);
                }
            }

            return String.Empty;
        }

        public static string FormatValue(ushort Value, Parameters Parameter)
        {
            /*TODO*/
            return string.Empty;
        }

        public static string FormatValue(string value, Parameters parameter)
        {
            string formattedValue = value;

            try
            {
                if (parameter.Exists())
                {
                    switch (parameter.Type)
                    {
                        case Parameters.Types.String:
                            // No need for normalization
                            break;

                        case Parameters.Types.AsciiToHex:
                            // Convert the value from ASCII to hex ("10" = 0x31 0x30)
                            formattedValue = Convert.ToString(value);
                            break;

                        case Parameters.Types.DecimalToHex:
                            // Convert the value from decimal to hex ("10" = 0x0A)
                            if (value == "92")
                            {
                                formattedValue = "\\u005C";
                            }
                            else
                            {
                                formattedValue = Encoding.GetString(new[] { Convert.ToByte(value) }, 0, 1);
                            }
                            break;

                        case Parameters.Types.HexString:
                            // Convert the value from ASCII to a hex string ("10" = 0x30 0x30 0x30 0x41)

                            formattedValue = Convert.ToInt16(value).ToString("X4");
                            break;
                    }
                    formattedValue = formattedValue.PadCharacter(parameter);
                }
            }
            catch (Exception)
            {
                formattedValue = "error";
            }
            
            return formattedValue;
        }

        private static string GetParameterString(Commands Command, Parameters Parameter)
        {
            if (Command.Parameters.Contains(Parameter))
            {
                string command = Command.Command;

                int start = command.IndexOf("!$[" + Parameter.Id);
                int end = command.IndexOf("]", start) + 1;

                return command.Substring(start, end - start);
            }

            return string.Empty;
        }


        public static Encoding Encoding = Encoding.GetEncoding("ISO-8859-1");

        public const string IDParameter = "!$[ID]";
        public const string DataFormattingFlag = "format=";
        public const string DataTypeFlag = "type=";
    }
}
