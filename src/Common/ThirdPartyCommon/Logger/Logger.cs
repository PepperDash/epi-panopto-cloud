// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using Crestron.Panopto.Common.Enums;
using Crestron.Panopto.Common.Interfaces;
using Crestron.SimplSharp.CrestronDataStore;

namespace Crestron.Panopto.Common.Logging
{
    /// <summary>
    /// Used for more verbose logging by the framework.
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// Specifies if logging is enabled on the system
        /// </summary>
        public bool LoggingEnabled;

        /// <summary>
        /// The current logging level.
        /// By default, Error will be selected.
        /// </summary>
        public LoggingLevel CurrentLevel;

        private Action<string> _driverLogger;

        public Logger(Action<string> driverLogger)
        {
            _driverLogger = driverLogger;
        }

        public void Debug(string message)
        {
            if (CurrentLevel == LoggingLevel.Debug &&
                _driverLogger != null)
            {
                _driverLogger(string.Format("[DEBUG] - {0}", message));
            }
        }

        public void Warning(string message)
        {
            if (CurrentLevel >= LoggingLevel.Warning &&
                _driverLogger != null)
            {
                _driverLogger(string.Format("[WARNING] - {0}", message));
            }
        }

        public void Error(string message)
        {
            if (CurrentLevel >= LoggingLevel.Error &&
                _driverLogger != null)
            {
                _driverLogger(string.Format("[ERROR] - {0}", message));
            }
        }
    }
}