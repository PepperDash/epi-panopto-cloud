// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System.Collections.Generic;
using Crestron.Panopto.Common.ExtensionMethods;
using System;

namespace Crestron.Panopto.Common
{
    public class CustomCommand
    {
        private string _command;

        public string Name { get; set; }
        public string Command
        {
            get { return _command; }
            set
            {
                _command = value.GetSafeCommandString(out CommandSetterError);
            }
        }

        /// <summary>
        /// An error message that is set when the setter for <see cref="Command"/> had an exception.
        /// This will be logged by the command sending logic if it is not null.
        /// </summary>
        internal Exception CommandSetterError;

        public IList<Parameters> Parameters { get; set; }
    }
}
