// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using System.Text;
using System.Globalization;

namespace Crestron.Panopto.Common.ExtensionMethods
{
    public static class ExtensionMethods
    {
        internal static bool TryDispose(this CrestronQueue queue)
        {
            var disposed = false;
            if (queue.Exists() &&
                !queue.Disposed)
            {
                queue.Dispose();
                disposed = true;
            }
            return disposed;
        }

        public static void AppendToStringBuffer(this object appendObject, System.Text.StringBuilder builder, CCriticalSection lockObject)
        {
            try
            {
                lockObject.Enter();
                builder.Append(appendObject);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Crestron.Panopto.Common.ExtensionMethods.AppendToStringBuffer - {0}", e.ToString());
            }
            finally
            {
                lockObject.Leave();
            }
        }

        public static string ToString(this StringBuilder builder, CCriticalSection lockObject)
        {
            string result = string.Empty;
            try
            {
                lockObject.Enter();
                result = builder.ToString();
            }
            catch (Exception e)
            {
                ErrorLog.Error("Crestron.Panopto.Common.ExtensionMethods.ToString - {0}", e.ToString());
            }
            finally
            {
                lockObject.Leave();
            }
            return result;
        }

        public static void Clear(this StringBuilder builder, CCriticalSection lockObject)
        {
            try
            {
                lockObject.Enter();
                builder.Length = 0;

                // Setting the capacity is a problem on Mono systems.  Over time it
                // takes longer and longer.  It is not necessary anyway since it
                // will free the memory buffer and force it to be re-allocated.
                // builder.Capacity = 0;
            }
            catch (Exception e)
            {
                ErrorLog.Error("Crestron.Panopto.Common.ExtensionMethods.Clear - {0}", e.ToString());
            }
            finally
            {
                lockObject.Leave();
            }
        }


        public static string PadCharacter(this string inputString, Parameters parameter)
        {
            try
            {
                var padChar = Convert.ToChar(parameter.PadCharacter.Unescape());
                switch (parameter.PadDirection)
                {
                    case Parameters.PadDirections.Left:
                        return inputString.PadLeft(parameter.StaticDataWidth, padChar);
                    case Parameters.PadDirections.Right:
                        return inputString.PadRight(parameter.StaticDataWidth, padChar);
                    default:
                        return inputString;
                }
            }
            catch (ArgumentNullException ex)
            {
                return inputString;
            }
        }

        /// <summary>
        /// Check that a string is not null or whitespace
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static bool HasValue(this string inputString)
        {
            return !inputString.IsNullOrWhiteSpace();
        }

        /// <summary>
        /// Is Null or White/Empty space check for string
        /// </summary>
        /// <param name="inputstring"></param>
        /// <returns></returns>
        public static bool IsNullOrWhiteSpace(this string inputstring)
        {
            return ((inputstring.DoesNotExist())
                || (string.Empty == inputstring.Trim()));
        }

        /// <summary>
        /// Is Null check
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <returns></returns>
        public static bool DoesNotExist<T>(this T field)
        {
            return (null == field);
        }

        /// <summary>
        /// Is not null check
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <returns></returns>
        public static bool Exists<T>(this T field)
        {
            return (null != field);
        }

        /// <summary>
        /// Get safe command string for device
        /// Replace double backslashes with single
        /// </summary>
        /// <param name="commandString"></param>
        /// <param name="exception">The exception thrown by calling Regex.Unescape(commandString)</param>
        /// <returns></returns>
        internal static string GetSafeCommandString(this string commandString, out Exception exception)
        {
            exception = null;
            if (!commandString.IsNullOrWhiteSpace() && 
                commandString.Contains("\\u"))
            {
                try
                {
                    commandString = Regex.Unescape(commandString);
                }
                catch (Exception e)
                {
                    exception = e;
                }
            }
            return commandString;
        }

        /// <summary>
        /// Get safe command string for device
        /// Replace double backslashes with single
        /// </summary>
        /// <param name="commandString"></param>
        /// <returns></returns>
        public static string GetSafeCommandString(this string commandString)
        {
            Exception exception = null;
            return GetSafeCommandString(commandString, out exception);
        }

        /// <summary>
        /// Replace double backslashes with single
        /// </summary>
        /// <param name="commandString"></param>
        /// <returns></returns>
        public static string Unescape(this string inputString)
        {
            return inputString.HasValue() ?
                Regex.Unescape(inputString) :
                inputString;
        }

        /// <summary>
        /// Replace single backslashes with double
        /// </summary>
        /// <param name="commandString"></param>
        /// <returns></returns>
        public static string Escape(this string inputString)
        {
            return inputString.HasValue() ?
                Regex.Escape(inputString) :
                inputString;
        }

        /// <summary>
        /// Unescape each value in given dictionary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dictionary"></param>
        public static void UnescapeDictionaryValues<T>(this Dictionary<T, string> dictionary)
        {
            if (dictionary.Exists())
            {
                foreach (var item in dictionary.ToArray())
                {
                    dictionary[item.Key] = item.Value.Unescape();
                }
            }
        }

        /// <summary>
        /// If disposableObj exists then dispose it.
        /// And if it is a timer stop it before dispose.
        /// </summary>
        /// <param name="disposableObj"></param>
        /// <returns>true/false - whether dispose has been called.</returns>
        public static bool TryDispose(this IDisposable disposableObj)
        {
            var disposed = false;
            if (disposableObj.Exists())
            {
                if (disposableObj is CTimer &&
                    (!((CTimer)disposableObj).Disposed))
                {
                    ((CTimer)disposableObj).Stop();
                    ((CTimer)disposableObj).Dispose();
                    disposed = true;
                }
                else
                {
                    disposableObj.Dispose();
                    disposed = true;
                }
            }
            else
            {
                disposed = false;
            }

            return disposed;
        }


        /// <summary>
        /// Truncates a string to the specified max length
        /// </summary>
        internal static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
