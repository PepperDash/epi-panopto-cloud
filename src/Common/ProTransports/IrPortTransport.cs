// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using Crestron.RAD.Common.Transports;
using Crestron.SimplSharpPro;

namespace Crestron.RAD.ProTransports
{
    public class IrPortTransport : IIrPort
    {
        private readonly IROutputPort _iport;

        public IrPortTransport(IROutputPort irPort)
        {
            _iport = irPort;
        }

        public uint IRDriverIdByFileName(string irFileName)
        {
            return _iport.IRDriverIdByFileName(irFileName);
        }

        public string IRDriverFileNameByIRDriverId(uint irDriverId)
        {
            return _iport.IRDriverFileNameByIRDriverId(irDriverId);
        }

        public void UnloadIRDriver(uint irDriverIDtoUnload)
        {
            _iport.UnloadIRDriver(irDriverIDtoUnload);
        }

        public void UnloadIRDriver()
        {
            _iport.UnloadIRDriver();
        }

        public void UnloadAllIRDrivers()
        {
            _iport.UnloadAllIRDrivers();
        }

        public uint LoadIRDriver(string irFileName)
        {
            return _iport.LoadIRDriver(irFileName);
        }

        public void Press(uint irDriverId, string irCmdName)
        {
            _iport.Press(irDriverId, irCmdName);
        }

        public void Press(string irCmdName)
        {
            _iport.Press(irCmdName);
        }

        public void Release()
        {
            _iport.Release();
        }

        public void PressAndRelease(uint irDriverId, string irCmdName, ushort timeOutInMs)
        {
            _iport.PressAndRelease(irDriverId, irCmdName, timeOutInMs);
        }

        public void PressAndRelease(string irCmdName, ushort timeOutInMs)
        {
            _iport.PressAndRelease(irCmdName, timeOutInMs);
        }

        public string GetStandardCmdFromIRCmd(uint irDriverId, string irCommand)
        {
            return _iport.GetStandardCmdFromIRCmd(irDriverId, irCommand);
        }

        public string GetStandardCmdFromIRCmd(string irCommand)
        {
            return _iport.GetStandardCmdFromIRCmd(irCommand);
        }

        public string[] AvailableStandardIRCmds(uint irDriverId)
        {
            return _iport.AvailableStandardIRCmds(irDriverId);
        }

        public string[] AvailableStandardIRCmds()
        {
            return _iport.AvailableStandardIRCmds();
        }

        public string[] AvailableIRCmds(uint irDriverId)
        {
            return _iport.AvailableIRCmds(irDriverId);
        }

        public string[] AvailableIRCmds()
        {
            return _iport.AvailableIRCmds();
        }

        public bool IsIRCommandAvailable(uint irDriverId, string irCmdName)
        {
            return _iport.IsIRCommandAvailable(irDriverId, irCmdName);
        }

        public bool IsIRCommandAvailable(string irCmdName)
        {
            return _iport.IsIRCommandAvailable(irCmdName);
        }

        public int IRDriversLoadedCount { get { return _iport.IRDriversLoadedCount; } }
    }
}
