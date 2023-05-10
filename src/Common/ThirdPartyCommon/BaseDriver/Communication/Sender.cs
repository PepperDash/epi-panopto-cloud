// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Linq;
using Crestron.RAD.Common.ExtensionMethods;
using Crestron.RAD.Common.Helpers;

namespace Crestron.RAD.Common.BasicDriver
{

    public abstract partial class ABaseDriverProtocol
    {
        protected virtual bool PrepareStringThenSend(CommandSet commandSet)
        {
            commandSet.CommandPrepared = true;
            return Send(commandSet);
        }

        protected virtual bool Send(CommandSet commandSet)
        {
            if (commandSet.DoesNotExist())
            {
                Log("Send - CommandSet was null");
                return false;
            }
            var sendResult = false;
            SendCommandResult result = CommandHelper.Send(
                new SendCommandVariables
                {
                    CanSendCommands = SendCommands,
                    WarmingUp = WarmingUp,
                    CoolingDown = CoolingDown,
                    DriverLoaded = DriverLoaded,
                    HasPower = PowerIsOn,
                    Queue = QueueCommands,
                    PendingRequest = PendingRequest
                }, commandSet);
            if (result.Exists())
            {
                if (result.SendToTransport)
                {
                    SendToTransport(commandSet);
                }
                else if (result.SendToQueue)
                {
                    SendToQueue(commandSet);
                }
                sendResult = (result.SendToTransport || result.SendToQueue);
            }
            return sendResult;
        }

        /// <summary>
        /// Modifies the queue with the given command if the same type and priority exist or adds the command to the queue
        /// Polling commands will not overwrite the queue nor will they be added if the queue already contains them
        /// </summary>
        private void SendToQueue(CommandSet commandSet)
        {
            bool modifyQueue = true;
            if (commandSet.DoesNotExist())
            {
                Log("SendToQueue - CommandSet is null");
            }
            else
            {
                try
                {
                    CriticalSection.Enter();
                    if (CommandQueue.DoesNotExist() || CommandQueue.Values.DoesNotExist())
                    {
                        Log(string.Format("SendToQueue - CommandQueue/CommandQueue Values is null - Command={0}", commandSet.CommandName));
                    }
                    else
                    {
                        if (commandSet.IsPollingCommand
                            && CommandQueue.Values
                                .Any(t => t.CommandGroup == commandSet.CommandGroup && t.IsPollingCommand))
                        {
                            modifyQueue = false;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log(string.Format("SendToQueue Exception={0}", e.Message));
                }
                finally
                {
                    CriticalSection.Leave();
                }

                if (modifyQueue && ModifyQueuedCommand(commandSet) == false)
                {
                    AddCommandToQueue(commandSet);
                }
            }
        }

        /// <summary>
        /// Logs all queued commands if their priority is not the lowest
        /// </summary>
        /// <param name="commandSet">The command that was queued</param>
        /// <param name="modified">Was a command in the queue modified<param>
        private void LogQueuedCommand(CommandSet commandSet, bool modified)
        {
            if (commandSet.CommandPriority != CommandPriority.Lowest)
            {
                Log(string.Format(modified ? "Modifying queue : {0}" : "Sending to queue: {0}", commandSet.CommandName));
            }
        }

        /// <summary>
        /// Adds a command to the queue
        /// </summary>
        /// <param name="commandSet">Command to be queued</param>
        /// <returns>True if the command was added</returns>
        private bool AddCommandToQueue(CommandSet commandSet)
        {
            bool commandAdded = false;

            if (commandSet.DoesNotExist())
            {
                Log("AddCommandToQueue - CommandSet is null");
            }
            else
            {
                try
                {
                    CriticalSection.Enter();
                    if (CommandQueue.DoesNotExist() || CommandQueue.Values.DoesNotExist())
                    {
                        Log(string.Format("AddCommandToQueue - CommandQueue/CommandQueue Values is null - Command={0}", commandSet.CommandName));
                    }
                    else
                    {
                        CommandQueue.Add(commandSet, (int)commandSet.CommandPriority);
                        commandAdded = true;
                    }
                }
                catch (Exception e)
                {
                    Log("AddCommandToQueue Exception: " + e.Message);
                }
                finally
                {
                    CriticalSection.Leave();
                    LogQueuedCommand(commandSet, commandAdded);
                }
            }

            return commandAdded;
        }

        /// <summary>
        /// Attempts to modify the queue if a command already exists of the same group and priority
        /// </summary>
        /// <param name="commandSet">Command to be queued</param>
        /// <returns>True if a command was modified</returns>
        private bool ModifyQueuedCommand(CommandSet commandSet)
        {
            bool commandModified = false;

            if (commandSet.DoesNotExist())
            {
                Log("ModifyQueuedCommand - CommandSet is null");
            }
            else
            {
                try
                {
                    CriticalSection.Enter();
                    if (CommandQueue.DoesNotExist() || CommandQueue.Values.DoesNotExist())
                    {
                        Log(string.Format("ModifyQueuedCommand - CommandQueue/CommandQueue Values is null - Command={0}", commandSet.CommandName));
                    }
                    else
                    {
                        var commandQueueValues = CommandQueue.Values;
                        for (var i = 0; i < commandQueueValues.Length; i++)
                        {
                            if (commandQueueValues[i].CommandGroup == commandSet.CommandGroup && commandQueueValues[i].CommandPriority == commandSet.CommandPriority)
                            {
                                CommandQueue.Modify(i, commandSet);
                                commandModified = true;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log(String.Format("ModifyQueuedCommand failure. Reason={0}", e.Message));
                }
                finally
                {
                    CriticalSection.Leave();
                    LogQueuedCommand(commandSet, commandModified);
                }
            }

            return commandModified;
        }

        private void SendToTransport(CommandSet commandSet)
        {
            PendingRequest = commandSet;

            if (commandSet.CommandPriority != CommandPriority.Lowest)
            {
                Log("Sending to transport: " + commandSet.CommandName);
            }

            Transport.Send(commandSet.Command, commandSet.Parameters);
            LastCommandGroup = commandSet.CommandGroup;

            if (commandSet.CallBack != null)
            {
                commandSet.CallBack();
            }
            if (!WaitForResponse)
            {
                WaitTimer.Reset(TimeBetweenCommands);
            }
        }
    }
}