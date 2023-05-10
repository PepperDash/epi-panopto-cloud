// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

namespace Crestron.Panopto.Common.Transports
{
    public interface IIrPort
    {
        uint IRDriverIdByFileName(string irFileName);
        string IRDriverFileNameByIRDriverId(uint irDriverId);
        void UnloadIRDriver(uint irDriverIDtoUnload);
        void UnloadIRDriver();
        void UnloadAllIRDrivers();
        uint LoadIRDriver(string irFileName);
        void Press(uint irDriverId, string irCmdName);
        void Press(string irCmdName);
        void Release();
        void PressAndRelease(uint irDriverId, string irCmdName, ushort timeOutInMs);
        void PressAndRelease(string irCmdName, ushort timeOutInMs);
        string GetStandardCmdFromIRCmd(uint irDriverId, string irCommand);
        string GetStandardCmdFromIRCmd(string irCommand);
        string[] AvailableStandardIRCmds(uint irDriverId);
        string[] AvailableStandardIRCmds();
        string[] AvailableIRCmds(uint irDriverId);
        string[] AvailableIRCmds();
        bool IsIRCommandAvailable(uint irDriverId, string irCmdName);
        bool IsIRCommandAvailable(string irCmdName);
        int IRDriversLoadedCount { get; }
    }
}
