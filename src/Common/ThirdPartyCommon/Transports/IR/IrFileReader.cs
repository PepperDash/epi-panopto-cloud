// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Crestron.Panopto.Common.BasicDriver;
using Crestron.Panopto.Common.Enums;
using Crestron.Panopto.Common.ExtensionMethods;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using Crestron.Panopto.Common.StandardCommands;
using Crestron.SimplSharp.CrestronIO;

namespace Crestron.Panopto.Common.Transports
{
    public class IrFileReader
    {
        private static Encoding _encoding = Encoding.GetEncoding("ISO-8859-1");
        private const uint Func_Overhead = 5;   // Used for multi-code operations, retrieved from CustomApplicationMessages.eIROutputConstants (internal so we can't reference it)
        private const int MinimumPulseLength = 100;

        public bool EnableLogging;
        public Action<string> Logger;

        public DateTime RevisionDate;
        public string Manufacturer;
        public string Model;
        public string DeviceType;

        internal Action<string, object[]> SendTx { get; set; }
        internal int Port { get; set; }

        private string _irFileName;
        private uint _multiModeCounter;
        private Dictionary<int, IrFunction> _irFunctions;
        private Dictionary<StandardCommandsEnum, IrFunction> _standardIrFunctions;
        private Dictionary<string, IrFunction> _labeledIrFunctions;

        private bool _delayingForOtherCommand;
        private Queue<DelayedIrCommand> _delayedCommands;
        private CCriticalSection _delayedCommandsLock;
        private readonly CTimer _delayTimer;
        private int _lastSentPressCommandTick;
        private TimedEventHelper _delayedReleaseCommand;
        private CCriticalSection _sendLock;

        #region Constructor

        public IrFileReader()
        {
            _delayingForOtherCommand = false;
            _irFunctions = new Dictionary<int, IrFunction>();
            _delayedCommands = new Queue<DelayedIrCommand>();
            _delayedCommandsLock = new CCriticalSection();
            _delayTimer = new CTimer(EndOfDelay, Timeout.Infinite);
            _delayedReleaseCommand = new TimedEventHelper() { ExecuteEventHandler = TimedFunctionRelease };
            _standardIrFunctions = new Dictionary<StandardCommandsEnum, IrFunction>();
            _labeledIrFunctions = new Dictionary<string, IrFunction>();
            _sendLock = new CCriticalSection();
        }

        #endregion Constructor

        #region File Loading

        public void LoadIrFile(string irFileName)
        {
            // Save the filename for logging purposes
            _irFileName = irFileName;

            if (File.Exists(irFileName))
            {
                var data = File.ReadToEnd(irFileName, Encoding.GetEncoding("ISO-8859-1"));

                var fileIsValid = ValidateIrFile(data);
                if (fileIsValid)
                {
                    ProcessIrFile(irFileName);
                    CreateCommandMappings();

                    if (EnableLogging)
                    {
                        LogMessage("File has been loaded");
                    }
                }
                else if (EnableLogging)
                {
                    LogMessage("Specified file is not a valid IR file");
                }

            }
            else if (EnableLogging)
            {
                LogMessage("File not found");
            }
        }

        private void ProcessIrFile(string irFileName)
        {
            byte commandSize = 0;
            byte[] dataBytes;
            string irData;
            const byte StandardButtonMapVersion = 1;
            const int MaximumStandardButtonId = 125; //arbitrary # < 128 [chosen to be easy for multiplication]

            using (BinaryReader binaryReader = new BinaryReader(File.Open(irFileName, FileMode.Open, FileAccess.Read)))
            {
                if (binaryReader.Exists())
                {
                    IDictionary<byte, StandardCommandsEnum> irFunctionToStandardCommandDictionary = new Dictionary<byte, StandardCommandsEnum>();
                    bool readingPayloads = false;

                    while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                    {
                        // Read a command here
                        commandSize = binaryReader.ReadByte();

                        if (commandSize == 0x00)
                            break;

                        // Read the ID
                        byte id = binaryReader.ReadByte();

                        // Not <EOS>, read on
                        if ((commandSize == 2) && (id == 255))
                        {
                            // The section ends with 0x02,0xFF.
                            // At this point all function names have been collected
                            // and <EOS> has been reached, so get out of here

                            // Start of reading the IR payloads
                            // The ID becomds the Button Number
                            // The IRData becomes the payload
                            readingPayloads = true;
                            continue;
                        }

                        dataBytes = binaryReader.ReadBytes(commandSize - 2);

                        if (readingPayloads)
                        {
                            IrFunction tmpIRFunction = null;
                            _irFunctions.TryGetValue(id, out tmpIRFunction);

                            if (tmpIRFunction.Exists())
                            {
                                tmpIRFunction.Payload = new byte[commandSize];
                                tmpIRFunction.Payload[0] = commandSize;
                                tmpIRFunction.Payload[1] = id;
                                Buffer.BlockCopy(dataBytes, 0, tmpIRFunction.Payload, 2, dataBytes.Length);
                            }

                            continue;
                        }

                        irData = System.Text.Encoding.ASCII.GetString(dataBytes, 0, dataBytes.Length);

                        // Check and fill the properties
                        if (id > 0xEF)
                        {
                            switch (id)
                            {
                                case 0xF1:
                                    // Filename
                                    break;
                                case 0xF2:
                                    RevisionDate = DateTime.ParseExact(irData, "MMddyy", null);
                                    break;
                                case 0xF3:
                                    Manufacturer = irData;
                                    break;
                                case 0xF4:
                                    Model = irData;
                                    break;
                                case 0xF5:
                                case 0xF6:
                                    break;
                                case 0xF7:
                                    DeviceType = irData;
                                    break;
                                case 0xF8:	// Standard command data

                                    if (dataBytes[0] == 2 && dataBytes[1] == StandardButtonMapVersion) //storage method 2, version 1
                                    {
                                        for (int i = 2; i < dataBytes.Length; i += 3)
                                        {
                                            if (i + 2 >= dataBytes.Length)
                                                break;

                                            byte functionId = dataBytes[i];				//IR function ID
                                            int multiplier = dataBytes[i + 1] - 1;
                                            int buttonOffset = dataBytes[i + 2];

                                            if (buttonOffset == MaximumStandardButtonId + 1)
                                                buttonOffset = 0;

                                            //standard command ID from Control 2 DB
                                            int standardButtonId = (multiplier * MaximumStandardButtonId) + buttonOffset;
                                            StandardCommandsEnum standardCommandEnum = FindMatchingStandardCommandById(standardButtonId, functionId);

                                            if (!standardCommandEnum.Equals(StandardCommandsEnum.NotAStandardCommand))
                                            {
                                                irFunctionToStandardCommandDictionary[functionId] = standardCommandEnum;
                                            }
                                        }
                                    }
                                    break;
                                case 0xF9:
                                case 0xFA:
                                case 0xFB:
                                case 0xFC:
                                case 0xFD:
                                case 0xFE:
                                case 0xFF:
                                    break;
                            }
                        }
                        else
                        {
                            StandardCommandsEnum standardCommandEnum = StandardCommandsEnum.NotAStandardCommand;
                            irFunctionToStandardCommandDictionary.TryGetValue(id, out standardCommandEnum);
                            if (standardCommandEnum.Equals(StandardCommandsEnum.NotAStandardCommand))
                            {
                                standardCommandEnum = FindMatchingStandardCommandByName(irData);
                            }
                            IrFunction irFunction = new IrFunction()
                            {
                                Id = id,
                                Label = irData.ToUpper(),
                                FrameworkStandardCommand = standardCommandEnum
                            };
                            _irFunctions.Add(id, irFunction);
                        }
                    }
                }
                else if (EnableLogging)
                {
                    LogMessage("Error processing IR file - reader failed to open file");
                }
            }
        }

        private bool ValidateIrFile(string irFileData)
        {
            return irFileData.Length > 4 &&
                irFileData.StartsWith("\x04\xF0\x49\x52");
        }

        private void CreateCommandMappings()
        {
            _standardIrFunctions.Clear();
            _labeledIrFunctions.Clear();

            var irFunctions = _irFunctions.Values.ToList();

            for (int i = 0; i < irFunctions.Count; i++)
            {
                if (!irFunctions[i].FrameworkStandardCommand.Equals(StandardCommandsEnum.NotAStandardCommand))
                {
                    _standardIrFunctions[irFunctions[i].FrameworkStandardCommand] = irFunctions[i];
                }
                if (!string.IsNullOrEmpty(irFunctions[i].Label))
                {
                    _labeledIrFunctions[irFunctions[i].Label] = irFunctions[i];
                }
            }
        }

        #endregion File Loading

        #region Data Transmission

        public void TriggerFunctionPress(string commandName, bool allowQueueing, uint delayForFutureCommands)
        {
            try
            {
                _sendLock.Enter();
                IrFunction irFunction = null;

                // Try to resolve the name to a standard command first
                // then look at the label if that fails
                // This is to make it act like SimplSharpPro.IROutputPort
                var resolvedStandardCommand = StandardCommandsEnum.NotAStandardCommand;
                _buttonLabelToEnumMapping.TryGetValue(commandName, out resolvedStandardCommand);
                if (resolvedStandardCommand.Equals(StandardCommandsEnum.NotAStandardCommand))
                {
                    _labeledIrFunctions.TryGetValue(commandName.ToUpper(), out irFunction);
                }
                else
                {
                    _standardIrFunctions.TryGetValue(resolvedStandardCommand, out irFunction);
                }

                if (irFunction.Exists())
                {
                    if (_delayingForOtherCommand)
                    {
                        if (allowQueueing)
                        {
                            AddDelayedCommand(irFunction, delayForFutureCommands);
                        }
                    }
                    else
                    {
                        // Send the data to the IR port 
                        TransmitData(irFunction);
                        if (delayForFutureCommands > 0)
                        {
                            _delayingForOtherCommand = true;
                            _delayTimer.Reset(delayForFutureCommands);
                        }
                    }
                }
                else if (EnableLogging)
                {
                    LogMessage(string.Format("TriggerFunctionPress - Unable to find an IR function for command label {0}", commandName));
                }
            }
            catch (Exception e)
            {
                if (EnableLogging)
                {
                    LogMessage(string.Format("TriggerFunctionPress - Exception: {0}", e.Message));
                }
            }
            finally
            {
                _sendLock.Leave();
            }
        }

        public void TriggerFunctionPress(StandardCommandsEnum standardCommand, bool allowQueueing, uint delayForFutureCommands)
        {
            try
            {
                _sendLock.Enter();
                IrFunction irFunction = null;
                _standardIrFunctions.TryGetValue(standardCommand, out irFunction);
                if (irFunction.Exists())
                {
                    if (_delayingForOtherCommand)
                    {
                        if (allowQueueing)
                        {
                            AddDelayedCommand(irFunction, delayForFutureCommands);
                        }
                    }
                    else
                    {
                        _delayedReleaseCommand.Stop();
                        // Send the data to the IR port 
                        TransmitData(irFunction);

                        if (delayForFutureCommands > 0)
                        {
                            _delayingForOtherCommand = true;
                            _delayTimer.Reset(delayForFutureCommands);
                        }
                    }
                }
                else if (EnableLogging)
                {
                    LogMessage(string.Format("TriggerFunctionPress - Unable to find an IR function for standard command {0}", standardCommand));
                }
            }
            catch (Exception e)
            {
                if (EnableLogging)
                {
                    LogMessage(string.Format("TriggerFunctionPress - Exception: {0}", e.Message));
                }
            }
            finally
            {
                _sendLock.Leave();
            }
        }

        public void TriggerFunctionRelease()
        {
            try
            {
                _sendLock.Enter();

                // This method must space out the release and the last command sent by at least 100ms 
                // If it is less than 100ms the command might not work.
                // TransmitData keeps track of the last sent command
                var difference = Math.Abs(CrestronEnvironment.TickCount - _lastSentPressCommandTick);
                if (difference >= 100)
                {
                    _delayedReleaseCommand.Stop();
                    TransmitReleaseData();
                }
                else
                {
                    _delayedReleaseCommand.Start(MinimumPulseLength - difference);
                }
            }
            catch (Exception e)
            {
                if (EnableLogging)
                {
                    LogMessage(string.Format("TriggerFunctionRelease - Exception: {0}", e.Message));
                }
            }
            finally
            {
                _sendLock.Leave();
            }
        }

        private void TimedFunctionRelease(object notUsed)
        {
            TransmitReleaseData();
        }

        private void TransmitReleaseData()
        {
            if (SendTx.Exists() &&
                Port != 0)
            {
                var toIrPacketTx = new StringBuilder("\x16");
                toIrPacketTx.Append(Convert.ToChar(Port - 1));
                toIrPacketTx.Append("\x01");

                if (EnableLogging)
                {
                    LogMessage("Transmitting release code");
                }

                SendTx(toIrPacketTx.ToString(), null);
            }
        }

        private void TransmitData(IrFunction fucntion)
        {
            if (SendTx.Exists() &&
                Port != 0 &&
                fucntion.Exists() &&
                fucntion.Payload.Exists())
            {
                _lastSentPressCommandTick = CrestronEnvironment.TickCount;

                byte[] payload = fucntion.Payload;

                if (IsMultiCode(payload))
                {
                    var resetCounter = false;
                    payload = GetMultiCode(ref _multiModeCounter, fucntion.Payload, out resetCounter);
                    if (resetCounter)
                    {
                        _multiModeCounter = 0;
                    }
                }

                byte[] dataToTransmit = new byte[payload.Length + 3];
                dataToTransmit[0] = 0x16; // Header
                dataToTransmit[1] = Convert.ToByte(Port - 1); // Port
                dataToTransmit[2] = 0x00; // ?????? It's always zero

                Array.Copy(payload, 0, dataToTransmit, 3, payload.Length);

                if (EnableLogging)
                {
                    LogMessage(string.Format("Transmitting the command Name: {0} - StandardCommand: {1}",
                        fucntion.Label, fucntion.FrameworkStandardCommand));
                }

                SendTx(_encoding.GetString(dataToTransmit, 0, dataToTransmit.Length), null);
            }
            else if (EnableLogging)
            {
                if (SendTx.DoesNotExist())
                {
                    LogMessage("Unable to send command - SendTx has not been assigned");
                }
                else if (Port == 0)
                {
                    LogMessage("Unable to send command - an invalid IR port was specified");
                }
                else if (fucntion.DoesNotExist())
                {
                    LogMessage("Unable to send command - no valid function provided to send");
                }
                else if (fucntion.Payload.DoesNotExist())
                {
                    LogMessage(string.Format("Unable to send command {0} - no valid payload to send", fucntion.Label));
                }
            }
        }

        #endregion Data Transmission

        #region Delayed Commands

        private void AddDelayedCommand(IrFunction function, uint delayForFutureCommands)
        {
            if (function.Exists())
            {
                var delayedCommand = new DelayedIrCommand(function, delayForFutureCommands);
                try
                {
                    _delayedCommandsLock.Enter();
                    _delayedCommands.Enqueue(delayedCommand);
                }
                finally
                {
                    _delayedCommandsLock.Leave();
                }

                if (EnableLogging)
                {
                    LogMessage(string.Format("Added command to queue - command: {0} - delay: {1}", function.Label, delayForFutureCommands));
                }
            }
        }

        private void EndOfDelay(object notUsed)
        {
            _delayingForOtherCommand = false;

            while (!_delayingForOtherCommand)
            {
                DelayedIrCommand nextCommand = null;
                try
                {
                    _delayedCommandsLock.Enter();
                    if (_delayedCommands.Count > 0)
                    {
                        nextCommand = _delayedCommands.Dequeue();
                    }
                }
                finally
                {
                    _delayedCommandsLock.Leave();
                }

                if (nextCommand.Exists())
                {
                    TriggerFunctionPress(nextCommand.Function.FrameworkStandardCommand, true, nextCommand.DelayForFutureCommands);
                }
                else
                {
                    break;
                }
            }

            TriggerFunctionRelease();
        }

        #endregion Delayed Commands

        #region Multicode Helpers

        private static bool IsMultiCode(byte[] irData)
        {
            //if byte 0 (count) == byte 3 + byte 4 + (2*byte 5)&0xF + 6this is not multi-code
            //byte 3 = length of header
            //byte 4 = length of repeat
            //byte 5 = number of timing words (top nibble is "special")
            //6 = header overhead
            if (irData.Exists() &&
                irData.Length > Func_Overhead)
            {
                if (irData[0] > (irData[3] + irData[4] + (2 * (irData[5] & 0xF)) + (uint)5 + 1))
                {
                    return true;
                }
            }

            return false;
        }

        private byte[] GetMultiCode(ref uint currMultiCodeCounter, byte[] irData, out bool resetCounter)
        {
            byte[] retVal = null;
            // Find the ir code at location m_iMultiCodeCounter
            uint count = irData[0];
            uint func = 1;
            uint funccount = (uint)(irData[func + 2] + irData[func + 3] + 2 * (irData[func + 4] & 0xF));
            uint nextfunc;
            resetCounter = false;
            for (uint i = 0; i < currMultiCodeCounter; i++)
            {
                nextfunc = func + Func_Overhead + funccount;
                if (nextfunc >= count)
                {
                    // Passed end so reset to the first code
                    func = 1;
                    funccount = (uint)(irData[func + 2] + irData[func + 3] + 2 * (irData[func + 4] & 0xF));
                    currMultiCodeCounter = 0;
                    resetCounter = true;
                    break;
                }
                else
                {
                    func = nextfunc;
                    funccount = (uint)(irData[func + 2] + irData[func + 3] + 2 * (irData[func + 4] & 0xF));
                }
            }

            retVal = new byte[funccount + Func_Overhead + 1];
            retVal[0] = (byte)(funccount + Func_Overhead + 1);
            Array.Copy(irData, (int)func, retVal, 1, (int)(funccount + Func_Overhead));
            currMultiCodeCounter++;

            return retVal;
        }

        #endregion Multicode Helpers

        #region Command Helpers

        public bool IsACommand(string commandName)
        {
            IrFunction irFunction = null;
            _labeledIrFunctions.TryGetValue(commandName.ToUpper(), out irFunction);

            if (EnableLogging)
            {
                LogMessage(string.Format("Labeled command {0} exists: {1}", commandName, irFunction.Exists()));
            }

            return irFunction.Exists();
        }

        public bool IsACommand(StandardCommandsEnum command)
        {
            IrFunction irFunction = null;
            _standardIrFunctions.TryGetValue(command, out irFunction);

            if (EnableLogging)
            {
                LogMessage(string.Format("Standard command {0} exists: {1}", command, irFunction.Exists()));
            }

            return irFunction.Exists();
        }

        private StandardCommandsEnum FindMatchingStandardCommandByName(string name)
        {
            var commandEnum = StandardCommandsEnum.NotAStandardCommand;

            // Match to the string version of the standard command ID
            _buttonLabelToEnumMapping.TryGetValue(name.ToUpper(), out commandEnum);

            if (commandEnum.Equals(StandardCommandsEnum.NotAStandardCommand))
            {
                // Match to our enum names
                _enumNameToEnumMapping.TryGetValue(name.ToUpper(), out commandEnum);
            }

            if (EnableLogging)
            {
                if (commandEnum.Equals(StandardCommandsEnum.NotAStandardCommand))
                {
                    LogMessage(string.Format("Unable to find a matching standard command for name {0}",
                        name));
                }
                else
                {
                    LogMessage(string.Format("Mapped name {0} to standard command {1}",
                        name, commandEnum));
                }
            }

            return commandEnum;
        }

        private StandardCommandsEnum FindMatchingStandardCommandById(int standardCommandid, int buttonId)
        {
            var commandEnum = StandardCommandsEnum.NotAStandardCommand;

            _standardCommandIdToEnumMapping.TryGetValue(standardCommandid, out commandEnum);

            if (EnableLogging)
            {
                if (commandEnum.Equals(StandardCommandsEnum.NotAStandardCommand))
                {
                    LogMessage(string.Format("Unable to find a matching standard command for ID {0} (Button #{1})",
                        standardCommandid, buttonId));
                }
                else
                {
                    LogMessage(string.Format("Mapped ID {0} (Button #{1}) to standard command {2}",
                        standardCommandid, buttonId, commandEnum));
                }
            }

            return commandEnum;
        }

        #endregion Command Helpers

        #region Logging

        private void LogMessage(string message)
        {
            if (Logger.Exists())
            {
                Logger(string.Format("{0} - IrFileReader ({1}) - {2}",
                   CrestronEnvironment.TickCount, Path.GetFileName(_irFileName), message));
            }
        }

        #endregion Logging

        #region Static Data

        private static Dictionary<string, StandardCommandsEnum> _internalEnumNameToEnumMapping;
        private static Dictionary<string, StandardCommandsEnum> _enumNameToEnumMapping
        {
            get
            {
                if (_internalEnumNameToEnumMapping.Exists())
                {
                    return _internalEnumNameToEnumMapping;
                }
                else
                {
                    PopulateEnumToEnumNameMapping();
                    return _internalEnumNameToEnumMapping;
                }
            }
        }

        private static void PopulateEnumToEnumNameMapping()
        {
            var result = new Dictionary<string, StandardCommandsEnum>();

            result.Add(StandardCommandsEnum.Vga1.ToString().ToUpper(), StandardCommandsEnum.Vga1);
            result.Add(StandardCommandsEnum.Vga2.ToString().ToUpper(), StandardCommandsEnum.Vga2);
            result.Add(StandardCommandsEnum.Vga3.ToString().ToUpper(), StandardCommandsEnum.Vga3);
            result.Add(StandardCommandsEnum.Vga4.ToString().ToUpper(), StandardCommandsEnum.Vga4);
            result.Add(StandardCommandsEnum.Vga5.ToString().ToUpper(), StandardCommandsEnum.Vga5);
            result.Add(StandardCommandsEnum.Vga6.ToString().ToUpper(), StandardCommandsEnum.Vga6);
            result.Add(StandardCommandsEnum.Vga7.ToString().ToUpper(), StandardCommandsEnum.Vga7);
            result.Add(StandardCommandsEnum.Vga8.ToString().ToUpper(), StandardCommandsEnum.Vga8);
            result.Add(StandardCommandsEnum.Vga9.ToString().ToUpper(), StandardCommandsEnum.Vga9);
            result.Add(StandardCommandsEnum.Vga10.ToString().ToUpper(), StandardCommandsEnum.Vga10);
            result.Add(StandardCommandsEnum.Hdmi1.ToString().ToUpper(), StandardCommandsEnum.Hdmi1);
            result.Add(StandardCommandsEnum.Hdmi2.ToString().ToUpper(), StandardCommandsEnum.Hdmi2);
            result.Add(StandardCommandsEnum.Hdmi3.ToString().ToUpper(), StandardCommandsEnum.Hdmi3);
            result.Add(StandardCommandsEnum.Hdmi4.ToString().ToUpper(), StandardCommandsEnum.Hdmi4);
            result.Add(StandardCommandsEnum.Hdmi5.ToString().ToUpper(), StandardCommandsEnum.Hdmi5);
            result.Add(StandardCommandsEnum.Hdmi6.ToString().ToUpper(), StandardCommandsEnum.Hdmi6);
            result.Add(StandardCommandsEnum.Hdmi7.ToString().ToUpper(), StandardCommandsEnum.Hdmi7);
            result.Add(StandardCommandsEnum.Hdmi8.ToString().ToUpper(), StandardCommandsEnum.Hdmi8);
            result.Add(StandardCommandsEnum.Hdmi9.ToString().ToUpper(), StandardCommandsEnum.Hdmi9);
            result.Add(StandardCommandsEnum.Hdmi10.ToString().ToUpper(), StandardCommandsEnum.Hdmi10);
            result.Add(StandardCommandsEnum.Dvi1.ToString().ToUpper(), StandardCommandsEnum.Dvi1);
            result.Add(StandardCommandsEnum.Dvi2.ToString().ToUpper(), StandardCommandsEnum.Dvi2);
            result.Add(StandardCommandsEnum.Dvi3.ToString().ToUpper(), StandardCommandsEnum.Dvi3);
            result.Add(StandardCommandsEnum.Dvi4.ToString().ToUpper(), StandardCommandsEnum.Dvi4);
            result.Add(StandardCommandsEnum.Dvi5.ToString().ToUpper(), StandardCommandsEnum.Dvi5);
            result.Add(StandardCommandsEnum.Dvi6.ToString().ToUpper(), StandardCommandsEnum.Dvi6);
            result.Add(StandardCommandsEnum.Dvi7.ToString().ToUpper(), StandardCommandsEnum.Dvi7);
            result.Add(StandardCommandsEnum.Dvi8.ToString().ToUpper(), StandardCommandsEnum.Dvi8);
            result.Add(StandardCommandsEnum.Dvi9.ToString().ToUpper(), StandardCommandsEnum.Dvi9);
            result.Add(StandardCommandsEnum.Dvi10.ToString().ToUpper(), StandardCommandsEnum.Dvi10);
            result.Add(StandardCommandsEnum.Component1.ToString().ToUpper(), StandardCommandsEnum.Component1);
            result.Add(StandardCommandsEnum.Component2.ToString().ToUpper(), StandardCommandsEnum.Component2);
            result.Add(StandardCommandsEnum.Component3.ToString().ToUpper(), StandardCommandsEnum.Component3);
            result.Add(StandardCommandsEnum.Component4.ToString().ToUpper(), StandardCommandsEnum.Component4);
            result.Add(StandardCommandsEnum.Component5.ToString().ToUpper(), StandardCommandsEnum.Component5);
            result.Add(StandardCommandsEnum.Component6.ToString().ToUpper(), StandardCommandsEnum.Component6);
            result.Add(StandardCommandsEnum.Component7.ToString().ToUpper(), StandardCommandsEnum.Component7);
            result.Add(StandardCommandsEnum.Component8.ToString().ToUpper(), StandardCommandsEnum.Component8);
            result.Add(StandardCommandsEnum.Component9.ToString().ToUpper(), StandardCommandsEnum.Component9);
            result.Add(StandardCommandsEnum.Component10.ToString().ToUpper(), StandardCommandsEnum.Component10);
            result.Add(StandardCommandsEnum.Composite1.ToString().ToUpper(), StandardCommandsEnum.Composite1);
            result.Add(StandardCommandsEnum.Composite2.ToString().ToUpper(), StandardCommandsEnum.Composite2);
            result.Add(StandardCommandsEnum.Composite3.ToString().ToUpper(), StandardCommandsEnum.Composite3);
            result.Add(StandardCommandsEnum.Composite4.ToString().ToUpper(), StandardCommandsEnum.Composite4);
            result.Add(StandardCommandsEnum.Composite5.ToString().ToUpper(), StandardCommandsEnum.Composite5);
            result.Add(StandardCommandsEnum.Composite6.ToString().ToUpper(), StandardCommandsEnum.Composite6);
            result.Add(StandardCommandsEnum.Composite7.ToString().ToUpper(), StandardCommandsEnum.Composite7);
            result.Add(StandardCommandsEnum.Composite8.ToString().ToUpper(), StandardCommandsEnum.Composite8);
            result.Add(StandardCommandsEnum.Composite9.ToString().ToUpper(), StandardCommandsEnum.Composite9);
            result.Add(StandardCommandsEnum.Composite10.ToString().ToUpper(), StandardCommandsEnum.Composite10);
            result.Add(StandardCommandsEnum.DisplayPort1.ToString().ToUpper(), StandardCommandsEnum.DisplayPort1);
            result.Add(StandardCommandsEnum.DisplayPort2.ToString().ToUpper(), StandardCommandsEnum.DisplayPort2);
            result.Add(StandardCommandsEnum.DisplayPort3.ToString().ToUpper(), StandardCommandsEnum.DisplayPort3);
            result.Add(StandardCommandsEnum.DisplayPort4.ToString().ToUpper(), StandardCommandsEnum.DisplayPort4);
            result.Add(StandardCommandsEnum.DisplayPort5.ToString().ToUpper(), StandardCommandsEnum.DisplayPort5);
            result.Add(StandardCommandsEnum.DisplayPort6.ToString().ToUpper(), StandardCommandsEnum.DisplayPort6);
            result.Add(StandardCommandsEnum.DisplayPort7.ToString().ToUpper(), StandardCommandsEnum.DisplayPort7);
            result.Add(StandardCommandsEnum.DisplayPort8.ToString().ToUpper(), StandardCommandsEnum.DisplayPort8);
            result.Add(StandardCommandsEnum.DisplayPort9.ToString().ToUpper(), StandardCommandsEnum.DisplayPort9);
            result.Add(StandardCommandsEnum.DisplayPort10.ToString().ToUpper(), StandardCommandsEnum.DisplayPort10);
            result.Add(StandardCommandsEnum.Usb1.ToString().ToUpper(), StandardCommandsEnum.Usb1);
            result.Add(StandardCommandsEnum.Usb2.ToString().ToUpper(), StandardCommandsEnum.Usb2);
            result.Add(StandardCommandsEnum.Usb3.ToString().ToUpper(), StandardCommandsEnum.Usb3);
            result.Add(StandardCommandsEnum.Usb4.ToString().ToUpper(), StandardCommandsEnum.Usb4);
            result.Add(StandardCommandsEnum.Usb5.ToString().ToUpper(), StandardCommandsEnum.Usb5);
            result.Add(StandardCommandsEnum.Antenna1.ToString().ToUpper(), StandardCommandsEnum.Antenna1);
            result.Add(StandardCommandsEnum.Antenna2.ToString().ToUpper(), StandardCommandsEnum.Antenna2);
            result.Add(StandardCommandsEnum.Network1.ToString().ToUpper(), StandardCommandsEnum.Network1);
            result.Add(StandardCommandsEnum.Network2.ToString().ToUpper(), StandardCommandsEnum.Network2);
            result.Add(StandardCommandsEnum.Network3.ToString().ToUpper(), StandardCommandsEnum.Network3);
            result.Add(StandardCommandsEnum.Network4.ToString().ToUpper(), StandardCommandsEnum.Network4);
            result.Add(StandardCommandsEnum.Network5.ToString().ToUpper(), StandardCommandsEnum.Network5);
            result.Add(StandardCommandsEnum.Network6.ToString().ToUpper(), StandardCommandsEnum.Network6);
            result.Add(StandardCommandsEnum.Network7.ToString().ToUpper(), StandardCommandsEnum.Network7);
            result.Add(StandardCommandsEnum.Network8.ToString().ToUpper(), StandardCommandsEnum.Network8);
            result.Add(StandardCommandsEnum.Network9.ToString().ToUpper(), StandardCommandsEnum.Network9);
            result.Add(StandardCommandsEnum.Network10.ToString().ToUpper(), StandardCommandsEnum.Network10);
            result.Add(StandardCommandsEnum.Input1.ToString().ToUpper(), StandardCommandsEnum.Input1);
            result.Add(StandardCommandsEnum.Input2.ToString().ToUpper(), StandardCommandsEnum.Input2);
            result.Add(StandardCommandsEnum.Input3.ToString().ToUpper(), StandardCommandsEnum.Input3);
            result.Add(StandardCommandsEnum.Input4.ToString().ToUpper(), StandardCommandsEnum.Input4);
            result.Add(StandardCommandsEnum.Input5.ToString().ToUpper(), StandardCommandsEnum.Input5);
            result.Add(StandardCommandsEnum.Input6.ToString().ToUpper(), StandardCommandsEnum.Input6);
            result.Add(StandardCommandsEnum.Input7.ToString().ToUpper(), StandardCommandsEnum.Input7);
            result.Add(StandardCommandsEnum.Input8.ToString().ToUpper(), StandardCommandsEnum.Input8);
            result.Add(StandardCommandsEnum.Input9.ToString().ToUpper(), StandardCommandsEnum.Input9);
            result.Add(StandardCommandsEnum.Input10.ToString().ToUpper(), StandardCommandsEnum.Input10);
            result.Add(StandardCommandsEnum.AspectRatio1.ToString().ToUpper(), StandardCommandsEnum.AspectRatio1);
            result.Add(StandardCommandsEnum.AspectRatio2.ToString().ToUpper(), StandardCommandsEnum.AspectRatio2);
            result.Add(StandardCommandsEnum.AspectRatio3.ToString().ToUpper(), StandardCommandsEnum.AspectRatio3);
            result.Add(StandardCommandsEnum.AspectRatio4.ToString().ToUpper(), StandardCommandsEnum.AspectRatio4);
            result.Add(StandardCommandsEnum.AspectRatio5.ToString().ToUpper(), StandardCommandsEnum.AspectRatio5);
            result.Add(StandardCommandsEnum.AspectRatio6.ToString().ToUpper(), StandardCommandsEnum.AspectRatio6);
            result.Add(StandardCommandsEnum.AspectRatio7.ToString().ToUpper(), StandardCommandsEnum.AspectRatio7);
            result.Add(StandardCommandsEnum.AspectRatio8.ToString().ToUpper(), StandardCommandsEnum.AspectRatio8);
            result.Add(StandardCommandsEnum.AllLampsOff.ToString().ToUpper(), StandardCommandsEnum.AllLampsOff);
            result.Add(StandardCommandsEnum.AllLampsOn.ToString().ToUpper(), StandardCommandsEnum.AllLampsOn);
            result.Add(StandardCommandsEnum.Antenna.ToString().ToUpper(), StandardCommandsEnum.Antenna);
            result.Add(StandardCommandsEnum.Asterisk.ToString().ToUpper(), StandardCommandsEnum.Asterisk);
            result.Add(StandardCommandsEnum.Mute.ToString().ToUpper(), StandardCommandsEnum.Mute);
            result.Add(StandardCommandsEnum.MuteOff.ToString().ToUpper(), StandardCommandsEnum.MuteOff);
            result.Add(StandardCommandsEnum.MuteOn.ToString().ToUpper(), StandardCommandsEnum.MuteOn);
            result.Add(StandardCommandsEnum.Auto.ToString().ToUpper(), StandardCommandsEnum.Auto);
            result.Add(StandardCommandsEnum.Aux1.ToString().ToUpper(), StandardCommandsEnum.Aux1);
            result.Add(StandardCommandsEnum.Aux2.ToString().ToUpper(), StandardCommandsEnum.Aux2);
            result.Add(StandardCommandsEnum.Channel.ToString().ToUpper(), StandardCommandsEnum.Channel);
            result.Add(StandardCommandsEnum.ChannelUp.ToString().ToUpper(), StandardCommandsEnum.ChannelUp);
            result.Add(StandardCommandsEnum.ChannelDown.ToString().ToUpper(), StandardCommandsEnum.ChannelDown);
            result.Add(StandardCommandsEnum.Tune.ToString().ToUpper(), StandardCommandsEnum.Tune);
            result.Add(StandardCommandsEnum.Eject.ToString().ToUpper(), StandardCommandsEnum.Eject);
            result.Add(StandardCommandsEnum.OnScreenDisplay.ToString().ToUpper(), StandardCommandsEnum.OnScreenDisplay);
            result.Add(StandardCommandsEnum.OnScreenDisplayOff.ToString().ToUpper(), StandardCommandsEnum.OnScreenDisplayOff);
            result.Add(StandardCommandsEnum.OnScreenDisplayOn.ToString().ToUpper(), StandardCommandsEnum.OnScreenDisplayOn);
            result.Add(StandardCommandsEnum.Power.ToString().ToUpper(), StandardCommandsEnum.Power);
            result.Add(StandardCommandsEnum.PowerOff.ToString().ToUpper(), StandardCommandsEnum.PowerOff);
            result.Add(StandardCommandsEnum.PowerOn.ToString().ToUpper(), StandardCommandsEnum.PowerOn);
            result.Add(StandardCommandsEnum.Vol.ToString().ToUpper(), StandardCommandsEnum.Vol);
            result.Add(StandardCommandsEnum.VolMinus.ToString().ToUpper(), StandardCommandsEnum.VolMinus);
            result.Add(StandardCommandsEnum.VolPlus.ToString().ToUpper(), StandardCommandsEnum.VolPlus);
            result.Add(StandardCommandsEnum._0.ToString().ToUpper(), StandardCommandsEnum._0);
            result.Add(StandardCommandsEnum._1.ToString().ToUpper(), StandardCommandsEnum._1);
            result.Add(StandardCommandsEnum._2.ToString().ToUpper(), StandardCommandsEnum._2);
            result.Add(StandardCommandsEnum._3.ToString().ToUpper(), StandardCommandsEnum._3);
            result.Add(StandardCommandsEnum._4.ToString().ToUpper(), StandardCommandsEnum._4);
            result.Add(StandardCommandsEnum._5.ToString().ToUpper(), StandardCommandsEnum._5);
            result.Add(StandardCommandsEnum._6.ToString().ToUpper(), StandardCommandsEnum._6);
            result.Add(StandardCommandsEnum._7.ToString().ToUpper(), StandardCommandsEnum._7);
            result.Add(StandardCommandsEnum._8.ToString().ToUpper(), StandardCommandsEnum._8);
            result.Add(StandardCommandsEnum._9.ToString().ToUpper(), StandardCommandsEnum._9);
            result.Add(StandardCommandsEnum.Octothorpe.ToString().ToUpper(), StandardCommandsEnum.Octothorpe);
            result.Add(StandardCommandsEnum.Audio.ToString().ToUpper(), StandardCommandsEnum.Audio);
            result.Add(StandardCommandsEnum.DownArrow.ToString().ToUpper(), StandardCommandsEnum.DownArrow);
            result.Add(StandardCommandsEnum.LeftArrow.ToString().ToUpper(), StandardCommandsEnum.LeftArrow);
            result.Add(StandardCommandsEnum.RightArrow.ToString().ToUpper(), StandardCommandsEnum.RightArrow);
            result.Add(StandardCommandsEnum.UpArrow.ToString().ToUpper(), StandardCommandsEnum.UpArrow);
            result.Add(StandardCommandsEnum.Enter.ToString().ToUpper(), StandardCommandsEnum.Enter);
            result.Add(StandardCommandsEnum.Home.ToString().ToUpper(), StandardCommandsEnum.Home);
            result.Add(StandardCommandsEnum.Clear.ToString().ToUpper(), StandardCommandsEnum.Clear);
            result.Add(StandardCommandsEnum.Display.ToString().ToUpper(), StandardCommandsEnum.Display);
            result.Add(StandardCommandsEnum.Exit.ToString().ToUpper(), StandardCommandsEnum.Exit);
            result.Add(StandardCommandsEnum.Blue.ToString().ToUpper(), StandardCommandsEnum.Blue);
            result.Add(StandardCommandsEnum.Green.ToString().ToUpper(), StandardCommandsEnum.Green);
            result.Add(StandardCommandsEnum.Red.ToString().ToUpper(), StandardCommandsEnum.Red);
            result.Add(StandardCommandsEnum.Yellow.ToString().ToUpper(), StandardCommandsEnum.Yellow);
            result.Add(StandardCommandsEnum.Options.ToString().ToUpper(), StandardCommandsEnum.Options);
            result.Add(StandardCommandsEnum.ForwardScan.ToString().ToUpper(), StandardCommandsEnum.ForwardScan);
            result.Add(StandardCommandsEnum.ReverseScan.ToString().ToUpper(), StandardCommandsEnum.ReverseScan);
            result.Add(StandardCommandsEnum.Pause.ToString().ToUpper(), StandardCommandsEnum.Pause);
            result.Add(StandardCommandsEnum.Play.ToString().ToUpper(), StandardCommandsEnum.Play);
            result.Add(StandardCommandsEnum.PlayPause.ToString().ToUpper(), StandardCommandsEnum.PlayPause);
            result.Add(StandardCommandsEnum.Repeat.ToString().ToUpper(), StandardCommandsEnum.Repeat);
            result.Add(StandardCommandsEnum.Return.ToString().ToUpper(), StandardCommandsEnum.Return);
            result.Add(StandardCommandsEnum.Select.ToString().ToUpper(), StandardCommandsEnum.Select);
            result.Add(StandardCommandsEnum.Stop.ToString().ToUpper(), StandardCommandsEnum.Stop);
            result.Add(StandardCommandsEnum.Subtitle.ToString().ToUpper(), StandardCommandsEnum.Subtitle);
            result.Add(StandardCommandsEnum.TopMenu.ToString().ToUpper(), StandardCommandsEnum.TopMenu);
            result.Add(StandardCommandsEnum.ForwardSkip.ToString().ToUpper(), StandardCommandsEnum.ForwardSkip);
            result.Add(StandardCommandsEnum.ReverseSkip.ToString().ToUpper(), StandardCommandsEnum.ReverseSkip);
            result.Add(StandardCommandsEnum.PopUpMenu.ToString().ToUpper(), StandardCommandsEnum.PopUpMenu);
            result.Add(StandardCommandsEnum.Menu.ToString().ToUpper(), StandardCommandsEnum.Menu);
            result.Add(StandardCommandsEnum.Info.ToString().ToUpper(), StandardCommandsEnum.Info);
            result.Add(StandardCommandsEnum.A.ToString().ToUpper(), StandardCommandsEnum.A);
            result.Add(StandardCommandsEnum.B.ToString().ToUpper(), StandardCommandsEnum.B);
            result.Add(StandardCommandsEnum.C.ToString().ToUpper(), StandardCommandsEnum.C);
            result.Add(StandardCommandsEnum.D.ToString().ToUpper(), StandardCommandsEnum.D);
            result.Add(StandardCommandsEnum.Back.ToString().ToUpper(), StandardCommandsEnum.Back);
            result.Add(StandardCommandsEnum.Dvr.ToString().ToUpper(), StandardCommandsEnum.Dvr);
            result.Add(StandardCommandsEnum.Favorite.ToString().ToUpper(), StandardCommandsEnum.Favorite);
            result.Add(StandardCommandsEnum.Guide.ToString().ToUpper(), StandardCommandsEnum.Guide);
            result.Add(StandardCommandsEnum.Last.ToString().ToUpper(), StandardCommandsEnum.Last);
            result.Add(StandardCommandsEnum.Live.ToString().ToUpper(), StandardCommandsEnum.Live);
            result.Add(StandardCommandsEnum.PageDown.ToString().ToUpper(), StandardCommandsEnum.PageDown);
            result.Add(StandardCommandsEnum.PageUp.ToString().ToUpper(), StandardCommandsEnum.PageUp);
            result.Add(StandardCommandsEnum.Record.ToString().ToUpper(), StandardCommandsEnum.Record);
            result.Add(StandardCommandsEnum.Replay.ToString().ToUpper(), StandardCommandsEnum.Replay);
            result.Add(StandardCommandsEnum.SpeedSlow.ToString().ToUpper(), StandardCommandsEnum.SpeedSlow);
            result.Add(StandardCommandsEnum.KeypadBackSpace.ToString().ToUpper(), StandardCommandsEnum.KeypadBackSpace);
            result.Add(StandardCommandsEnum.ThumbsUp.ToString().ToUpper(), StandardCommandsEnum.ThumbsUp);
            result.Add(StandardCommandsEnum.ThumbsDown.ToString().ToUpper(), StandardCommandsEnum.ThumbsDown);
            result.Add(StandardCommandsEnum.Dash.ToString().ToUpper(), StandardCommandsEnum.Dash);
            result.Add(StandardCommandsEnum.Period.ToString().ToUpper(), StandardCommandsEnum.Period);
            result.Add(StandardCommandsEnum.EnergyStar.ToString().ToUpper(), StandardCommandsEnum.EnergyStar);
            result.Add(StandardCommandsEnum.EnergyStarOn.ToString().ToUpper(), StandardCommandsEnum.EnergyStarOn);
            result.Add(StandardCommandsEnum.EnergyStarOff.ToString().ToUpper(), StandardCommandsEnum.EnergyStarOff);
            result.Add(StandardCommandsEnum.VideoMute.ToString().ToUpper(), StandardCommandsEnum.VideoMute);
            result.Add(StandardCommandsEnum.VideoMuteOn.ToString().ToUpper(), StandardCommandsEnum.VideoMuteOn);
            result.Add(StandardCommandsEnum.VideoMuteOff.ToString().ToUpper(), StandardCommandsEnum.VideoMuteOff);
            result.Add(StandardCommandsEnum.DVD.ToString().ToUpper(), StandardCommandsEnum.DVD);
            result.Add(StandardCommandsEnum.SAT.ToString().ToUpper(), StandardCommandsEnum.SAT);
            result.Add(StandardCommandsEnum.TV.ToString().ToUpper(), StandardCommandsEnum.TV);
            result.Add(StandardCommandsEnum.CD.ToString().ToUpper(), StandardCommandsEnum.CD);
            result.Add(StandardCommandsEnum.Tuner.ToString().ToUpper(), StandardCommandsEnum.Tuner);
            result.Add(StandardCommandsEnum.Phono.ToString().ToUpper(), StandardCommandsEnum.Phono);
            result.Add(StandardCommandsEnum.DSS.ToString().ToUpper(), StandardCommandsEnum.DSS);
            result.Add(StandardCommandsEnum.InternetRadio.ToString().ToUpper(), StandardCommandsEnum.InternetRadio);
            result.Add(StandardCommandsEnum.Sirius.ToString().ToUpper(), StandardCommandsEnum.Sirius);
            result.Add(StandardCommandsEnum.SiriusXm.ToString().ToUpper(), StandardCommandsEnum.SiriusXm);
            result.Add(StandardCommandsEnum.Pandora.ToString().ToUpper(), StandardCommandsEnum.Pandora);
            result.Add(StandardCommandsEnum.LastFm.ToString().ToUpper(), StandardCommandsEnum.LastFm);
            result.Add(StandardCommandsEnum.Rhapsody.ToString().ToUpper(), StandardCommandsEnum.Rhapsody);
            result.Add(StandardCommandsEnum.HdRadio.ToString().ToUpper(), StandardCommandsEnum.HdRadio);
            result.Add(StandardCommandsEnum.Spotify.ToString().ToUpper(), StandardCommandsEnum.Spotify);
            result.Add(StandardCommandsEnum.YouTube.ToString().ToUpper(), StandardCommandsEnum.YouTube);
            result.Add(StandardCommandsEnum.YouTubeTv.ToString().ToUpper(), StandardCommandsEnum.YouTubeTv);
            result.Add(StandardCommandsEnum.Netflix.ToString().ToUpper(), StandardCommandsEnum.Netflix);
            result.Add(StandardCommandsEnum.Hulu.ToString().ToUpper(), StandardCommandsEnum.Hulu);
            result.Add(StandardCommandsEnum.DirecTvNow.ToString().ToUpper(), StandardCommandsEnum.DirecTvNow);
            result.Add(StandardCommandsEnum.AmazonVideo.ToString().ToUpper(), StandardCommandsEnum.AmazonVideo);
            result.Add(StandardCommandsEnum.PlayStationVue.ToString().ToUpper(), StandardCommandsEnum.PlayStationVue);
            result.Add(StandardCommandsEnum.SlingTv.ToString().ToUpper(), StandardCommandsEnum.SlingTv);
            result.Add(StandardCommandsEnum.TunerFrequencyUp.ToString().ToUpper(), StandardCommandsEnum.TunerFrequencyUp);
            result.Add(StandardCommandsEnum.TunerFrequencyDown.ToString().ToUpper(), StandardCommandsEnum.TunerFrequencyDown);
            result.Add(StandardCommandsEnum.AirPlay.ToString().ToUpper(), StandardCommandsEnum.AirPlay);
            result.Add(StandardCommandsEnum.GoogleCast.ToString().ToUpper(), StandardCommandsEnum.GoogleCast);
            result.Add(StandardCommandsEnum.Dlna.ToString().ToUpper(), StandardCommandsEnum.Dlna);
            result.Add(StandardCommandsEnum.Tidal.ToString().ToUpper(), StandardCommandsEnum.Tidal);
            result.Add(StandardCommandsEnum.Deezer.ToString().ToUpper(), StandardCommandsEnum.Deezer);
            result.Add(StandardCommandsEnum.Crackle.ToString().ToUpper(), StandardCommandsEnum.Crackle);
            result.Add(StandardCommandsEnum.OnDemand.ToString().ToUpper(), StandardCommandsEnum.OnDemand);
            result.Add(StandardCommandsEnum.Tivo.ToString().ToUpper(), StandardCommandsEnum.Tivo);
            result.Add(StandardCommandsEnum.FSkip.ToString().ToUpper(), StandardCommandsEnum.FSkip);
            result.Add(StandardCommandsEnum.RSkip.ToString().ToUpper(), StandardCommandsEnum.RSkip);
            result.Add(StandardCommandsEnum.Optical1.ToString().ToUpper(), StandardCommandsEnum.Optical1);
            result.Add(StandardCommandsEnum.Optical2.ToString().ToUpper(), StandardCommandsEnum.Optical2);
            result.Add(StandardCommandsEnum.Optical3.ToString().ToUpper(), StandardCommandsEnum.Optical3);
            result.Add(StandardCommandsEnum.Optical4.ToString().ToUpper(), StandardCommandsEnum.Optical4);
            result.Add(StandardCommandsEnum.Optical5.ToString().ToUpper(), StandardCommandsEnum.Optical5);
            result.Add(StandardCommandsEnum.Optical6.ToString().ToUpper(), StandardCommandsEnum.Optical6);
            result.Add(StandardCommandsEnum.Optical7.ToString().ToUpper(), StandardCommandsEnum.Optical7);
            result.Add(StandardCommandsEnum.Optical8.ToString().ToUpper(), StandardCommandsEnum.Optical8);
            result.Add(StandardCommandsEnum.Optical9.ToString().ToUpper(), StandardCommandsEnum.Optical9);
            result.Add(StandardCommandsEnum.Optical10.ToString().ToUpper(), StandardCommandsEnum.Optical10);
            result.Add(StandardCommandsEnum.Coax1.ToString().ToUpper(), StandardCommandsEnum.Coax1);
            result.Add(StandardCommandsEnum.Coax2.ToString().ToUpper(), StandardCommandsEnum.Coax2);
            result.Add(StandardCommandsEnum.Coax3.ToString().ToUpper(), StandardCommandsEnum.Coax3);
            result.Add(StandardCommandsEnum.Coax4.ToString().ToUpper(), StandardCommandsEnum.Coax4);
            result.Add(StandardCommandsEnum.Coax5.ToString().ToUpper(), StandardCommandsEnum.Coax5);
            result.Add(StandardCommandsEnum.Coax6.ToString().ToUpper(), StandardCommandsEnum.Coax6);
            result.Add(StandardCommandsEnum.Coax7.ToString().ToUpper(), StandardCommandsEnum.Coax7);
            result.Add(StandardCommandsEnum.Coax8.ToString().ToUpper(), StandardCommandsEnum.Coax8);
            result.Add(StandardCommandsEnum.Coax9.ToString().ToUpper(), StandardCommandsEnum.Coax9);
            result.Add(StandardCommandsEnum.Coax10.ToString().ToUpper(), StandardCommandsEnum.Coax10);
            result.Add(StandardCommandsEnum.AnalogAudio1.ToString().ToUpper(), StandardCommandsEnum.AnalogAudio1);
            result.Add(StandardCommandsEnum.AnalogAudio2.ToString().ToUpper(), StandardCommandsEnum.AnalogAudio2);
            result.Add(StandardCommandsEnum.AnalogAudio3.ToString().ToUpper(), StandardCommandsEnum.AnalogAudio3);
            result.Add(StandardCommandsEnum.AnalogAudio4.ToString().ToUpper(), StandardCommandsEnum.AnalogAudio4);
            result.Add(StandardCommandsEnum.AnalogAudio5.ToString().ToUpper(), StandardCommandsEnum.AnalogAudio5);
            result.Add(StandardCommandsEnum.AnalogAudio6.ToString().ToUpper(), StandardCommandsEnum.AnalogAudio6);
            result.Add(StandardCommandsEnum.AnalogAudio7.ToString().ToUpper(), StandardCommandsEnum.AnalogAudio7);
            result.Add(StandardCommandsEnum.AnalogAudio8.ToString().ToUpper(), StandardCommandsEnum.AnalogAudio8);
            result.Add(StandardCommandsEnum.AnalogAudio9.ToString().ToUpper(), StandardCommandsEnum.AnalogAudio9);
            result.Add(StandardCommandsEnum.AnalogAudio10.ToString().ToUpper(), StandardCommandsEnum.AnalogAudio10);
            result.Add(StandardCommandsEnum.Bd1.ToString().ToUpper(), StandardCommandsEnum.Bd1);
            result.Add(StandardCommandsEnum.Catv1.ToString().ToUpper(), StandardCommandsEnum.Catv1);
            result.Add(StandardCommandsEnum.Game1.ToString().ToUpper(), StandardCommandsEnum.Game1);
            result.Add(StandardCommandsEnum.Pc1.ToString().ToUpper(), StandardCommandsEnum.Pc1);
            result.Add(StandardCommandsEnum.Bluetooth1.ToString().ToUpper(), StandardCommandsEnum.Bluetooth1);
            result.Add(StandardCommandsEnum.MediaPlayer1.ToString().ToUpper(), StandardCommandsEnum.MediaPlayer1);
            result.Add(StandardCommandsEnum.Ipod1.ToString().ToUpper(), StandardCommandsEnum.Ipod1);

            _internalEnumNameToEnumMapping = result;
        }

        private static Dictionary<string, StandardCommandsEnum> _internalButtonLabelToEnumMapping;
        private static Dictionary<string, StandardCommandsEnum> _buttonLabelToEnumMapping
        {
            get
            {
                if (_internalButtonLabelToEnumMapping.Exists())
                {
                    return _internalButtonLabelToEnumMapping;
                }
                else
                {
                    PopulateButtonLabelToEnumMapping();
                    return _internalButtonLabelToEnumMapping;
                }
            }
        }

        private static void PopulateButtonLabelToEnumMapping()
        {

            var result = new Dictionary<string, StandardCommandsEnum>();

            //Populating dictionary from the StandardEnumToCommandMapping dictionary
            if (Panopto.Common.StandardCommands.StandardCommands.StandardEnumToCommandMapping != null)
            {
                foreach (var item in Panopto.Common.StandardCommands.StandardCommands.StandardEnumToCommandMapping)
                {
                    if (item.Value != null)
                    {
                        if (!result.ContainsKey(item.Value.StandardCommandString))
                        {
                            result.Add(item.Value.StandardCommandString, item.Key);  //inverting Key and Value
                        }
                    }
                }

            }

            _internalButtonLabelToEnumMapping = result;
        }

        private static Dictionary<int, StandardCommandsEnum> _internalStandardCommandIdToEnumMapping;
        private static Dictionary<int, StandardCommandsEnum> _standardCommandIdToEnumMapping
        {
            get
            {
                if (_internalStandardCommandIdToEnumMapping.Exists())
                {
                    return _internalStandardCommandIdToEnumMapping;
                }
                else
                {
                    PopulateStandardCommandIdToEnumMapping();
                    return _internalStandardCommandIdToEnumMapping;
                }
            }
        }

        private static void PopulateStandardCommandIdToEnumMapping()
        {
            var result = new Dictionary<int, StandardCommandsEnum>();
            // The values are from a CSV file mapping Standard Command IDs to the standard command (Dropdown in Toolbox-DeviceLearner)

            //getting values from our IrStandardCommandIdToStandardEnumMapping Dictionary
            if (Panopto.Common.StandardCommands.StandardCommands.IrStandardCommandIdToStandardEnumMapping != null)
            {
                foreach (var item in Panopto.Common.StandardCommands.StandardCommands.IrStandardCommandIdToStandardEnumMapping)
                {
                    if (!result.ContainsKey(item.Value))
                    {
                        result.Add(item.Value, item.Key);   //inverting Key and Value
                    }
                }


                _internalStandardCommandIdToEnumMapping = result;
            }
        }
        #endregion Static Data
    }
}
