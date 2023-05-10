// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;

namespace Crestron.Panopto
{
    internal static class PanoptoLogger
    {
        public static bool DiagnosticLoggingEnabled = false;

        private const string _previous = "\\User\\PanoptoPreviousLog.txt";
        private const string _current = "\\User\\PanoptoLog.txt";

        private static CCriticalSection _lock = new CCriticalSection();

        public static void PrepareLogs()
        {
            try
            {
                if (File.Exists(_previous))
                {
                    File.Delete(_previous);
                }

                if (File.Exists(_current))
                {
                    File.Move(_current, _previous);
                    File.Create(_current);
                }
                else
                {
                    File.Create(_current);
                }
            }
            catch (Exception e)
            {
                PanoptoLogger.Error("Panopto.PanoptoLogger.PrepareLogs Error");
                PanoptoLogger.Error(e.ToString());
                PanoptoLogger.Error("message: {0}", e.Message);
            }
        }

        private static void PrintMessage(string message)
        {
            if (DiagnosticLoggingEnabled)
            {
                CrestronConsole.PrintLine(message);
                try
                {
                    _lock.Enter();
                    if (File.Exists(_current))
                    {
                        using (FileStream writer = File.Open(_current, FileMode.Append))
                        {
                            writer.Write(string.Format("{0}\x0D", message), Encoding.ASCII);
                        }
                    }
                }
                catch (Exception e)
                {
                    //I am doing nothing here on purpose
                }
                finally
                {
                    _lock.Leave();
                }
            }
        }

        public static void Notice(string message)
        {
            PrintMessage(message);
            if(DiagnosticLoggingEnabled)
            {
                ErrorLog.Notice(message);
            }
        }

        private static string ProcessMessage(string message, params object[] args)
        {
            int tick = CrestronEnvironment.TickCount;
            string processedMessage = String.Format(message, args);
            processedMessage = string.Format("{0}:{1}", tick, processedMessage);
            return processedMessage;
        }

        public static void Notice(string message, params object[] args)
        {
            if (DiagnosticLoggingEnabled)
            {
                string processedMessage = ProcessMessage(message, args);
                Notice(processedMessage);
            }
        }

        public static void Error(string message)
        {
            PrintMessage(message);
            ErrorLog.Error(message);
        }

        public static void Error(string message, params object[] args)
        {
            string processedMessage = ProcessMessage(message, args);
            Error(processedMessage);
        }
    }
}
