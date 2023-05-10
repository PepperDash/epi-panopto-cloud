// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

namespace Crestron.Panopto.Common.Interfaces
{
    public interface IKeypad
    {
        /// <summary>
        /// Property indicating that the KeypadNumber command is supported.
        /// </summary>
        bool SupportsKeypadNumber { get; }

        /// <summary>
        /// Sends a keypard number to the device.
        /// </summary>
        /// <param name="number">Number to be sent to the device.</param>
        void KeypadNumber(uint number);

        /// <summary>
        /// Method to send a series of keypad characters to the device.
        /// </summary>
        /// <param name="keys"></param>
        void SendKeypadString(string keys);

        /// <summary>
        /// Property indicating that the Enter command is supported.
        /// API should define what this functionality is for.  Typically to enter a channel number.
        /// </summary>
        bool SupportsEnter { get; }

        /// <summary>
        /// Method to send the enter command to the Device.
        /// Typically, Enter is used to force a channel number change
        /// </summary>
        void Enter();

        /// <summary>
        /// Property indicating that the Pound command is supported.
        /// </summary>
        bool SupportsPound { get; }

        /// <summary>
        /// Method to send a "#" to the Device.
        /// </summary>
        void Pound();

        /// <summary>
        /// Property indicating that the Asterisk command is supported.
        /// </summary>
        bool SupportsAsterisk { get; }

        /// <summary>
        /// Method to send a "*" to the Device.
        /// </summary>
        void Asterisk();

        /// <summary>
        /// Property indicating that the Period command is supported.
        /// </summary>
        bool SupportsPeriod { get; }

        /// <summary>
        /// Method to send a "." to the Device.
        /// </summary>
        void Period();

        /// <summary>
        /// Property indicating that the Dash command is supported.
        /// </summary>
        bool SupportsDash { get; }

        /// <summary>
        /// Method to send a "-" to the Device.
        /// </summary>
        void Dash();

        /// <summary>
        /// Property indicating that the Keypad Back Space command is supported.
        /// </summary>
        bool SupportsKeypadBackSpace { get; }

        /// <summary>
        /// Method to send a Back Space to the Device.
        /// </summary>
        void KeypadBackSpace();

        /// <summary>
        /// Property to get the primary and secondary lables for the numeric keypyad buttons 0-9.
        /// </summary>
        KeypadLabels[] NumericKeypadLabels { get; }

        /// <summary>
        /// Property to get the primary and secondary lables for the dash keypyad button.
        /// </summary>
        KeypadLabels DashLabels { get; }

        /// <summary>
        /// Property to get the primary and secondary lables for the period keypyad button.
        /// </summary>
        KeypadLabels PeriodLabels { get; }

        /// <summary>
        /// Property to get the primary and secondary lables for the asterisk keypyad button.
        /// </summary>
        KeypadLabels AsteriskLabels { get; }

        /// <summary>
        /// Property to get the primary and secondary lables for the pound keypyad button.
        /// </summary>
        KeypadLabels PoundLabels { get; }
    }

    public class KeypadLabels
    {
        public string PrimaryLabel { get; set; }
        public string SecondaryLabel { get; set; }
    }
}
