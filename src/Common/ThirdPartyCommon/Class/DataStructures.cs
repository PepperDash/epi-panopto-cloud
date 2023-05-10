// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System.Collections.Generic;
using Crestron.Panopto.Common.Enums;
using Crestron.Panopto.Common.StandardCommands;

namespace Crestron.Panopto.Common
{
    public class Parameters
    {
        public string Id { get; set; }
        public short Max { get; set; }
        public short Min { get; set; }
        public Types Type;
        public short StaticDataWidth { get; set; }
        public string PadCharacter;
        public enum PadDirections { Left, Right }
        public PadDirections PadDirection;
        public string value { get; set; }
        
        public enum Types 
        { 
            String = 0,
            AsciiToHex = 1,
            DecimalToHex = 2,
            HexString = 3
            // Future conversion types can be added here
        }
    }

    public class Commands
    {
        public StandardCommandsEnum StandardCommand { get; set; }
        public string Command { get; set; }
        public IList<Parameters> Parameters { get; set; }

        /// <summary>
        /// Avr Zone Field to allow for Sending of zone related commands
        /// </summary>
        public bool AllowIsSendableOverride { get; set; }

        /// <summary>
        /// Avr Zone Field to allow for Queuing of zone related commands
        /// </summary>
        public bool AllowIsQueueableOverride { get; set; }
    }
}
