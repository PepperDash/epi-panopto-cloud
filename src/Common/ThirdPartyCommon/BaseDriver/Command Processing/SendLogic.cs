// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Linq;
using Crestron.Panopto.Common.ExtensionMethods;
using Crestron.Panopto.Common.Helpers;
using Crestron.Panopto.Common.Enums;
using Crestron.SimplSharp;
using System.Collections.Generic;
using Crestron.Panopto.Common.Events;
using Crestron.Panopto.Common.Logging;
using Crestron.Panopto.Common.Transports;

namespace Crestron.Panopto.Common.BasicDriver
{
    public abstract partial class ABaseDriverProtocol
    {
        internal event Action<StandardCommandsEnum, CommonCommandGroupType> FakeFeedbackForCommand;

        /// <summary>
        /// The command ID that will be incremented everytime it is assigned to a command.
        /// Anytime this value is referenced to create a <see cref="Command"/> it must be incremented. If it
        /// is higher that ulong.MaxValue, it must be assigned ulong.MinValue.
        /// </summary>
        private ulong _commandId = ulong.MinValue;

        /// <summary>
        /// Keeps track of when the last command that was considered a pending request was sent. 
        /// This is used when WaitForResponse is true and the driver needs to not send anyting until a time out
        /// or until the response to the last sent command is received.
        /// </summary>
        private int _transmissionTickCountForPendingRequest = 0;

        /// <summary>
        /// This needs to be updated to CrestronEnvironment.TickCount whenever any command is sent.
        /// This means polling and non-polling commands are responsible for updating to this.
        /// This is used to enforce command pacing based on TimeBetweenCommands
        /// </summary>
        private int _lastSentCommandTick = 0;

        /// <summary>
        /// This keeps track of when the last non-polling command was
        /// sent to the transport without ever going into the queue.
        /// This is tracked since we want to hold off polling for a little bit after
        /// a command skipped the queue incase of ramping.
        /// We don't want polling commands to go out during a ramp, so this is how we keep track of it 
        /// </summary>
        private int _lastSentCommandThatSkippedQueueTick = 0;

        /// <summary>
        /// If we sent a command to the device without going to the queue first,
        /// then we need to wait until we start polling again incase there are more commands
        /// that will be sent that way. This is done to avoid polling during ramps
        /// </summary>
        private static int _ticksToWaitToResumePolling = 250;

        /// <summary>
        /// This keeps tracks of when the last poll cycle happened. 
        /// This means that when the first poll command is sent from the sequence, this value is set.
        /// It will not be set for all subsequent poll commands in that same poll cycle.
        /// This will make sure that we only poll in bursts at the polling interval.
        /// </summary>
        private int _lastPollCycleTick = 0;

        /// <summary>
        /// Keeps track of when a poll command was marked for removal
        /// to prevent iterating through that collection every clock cycle
        /// </summary>
        private bool _pollCommandMarkedForRemoval = false;

        /// <summary>
        /// Keeps track of when the last command of the poll sequence was sent
        /// </summary>
        private bool _finishedPollSequence = false;

        /// <summary>
        /// Lock used for the non-polling queue for modifications
        /// </summary>
        private CCriticalSection _nonPollingCommandLock = new CCriticalSection();

        /// <summary>
        /// Non-polling queue that uses a modified MinHeap
        /// </summary>
        private MinHeap<Command> _nonPollingCommandQueue = new MinHeap<Command>(50);

        /// <summary>
        /// Lock used for the polling queue for modifications
        /// </summary>
        private CCriticalSection _pollingCommandLock = new CCriticalSection();

        /// <summary>
        /// Polling queue
        /// </summary>
        private List<Command> _pollingCommandQueue = new List<Command>();

        /// <summary>
        /// This is used to make sure we are not attempting to process the queue
        /// while this driver is already processing it and is taking longer than normal
        /// </summary>
        private CCriticalSection _processQueueLock = new CCriticalSection();

        #region Driver methods

        /// <summary>
        /// Sends the specified CommandSet object to the internal command processor.
        /// 
        /// If this is a standard polling command, it will be sent to the polling queue as non-persistent.
        /// Otherwise it will be sent to the non-polling queue or will be sent to the device immediatly if conditions allow that.
        /// </summary>
        /// <returns>False</returns>
        protected virtual bool Send(CommandSet commandSet)
        {
            if (commandSet.Exists())
            {
                if (_commandId == ulong.MaxValue)
                {
                    _commandId = ulong.MinValue;
                }

                var translatedCommand = new Command(_commandId, commandSet, false, false);

                _commandId++;

                // Split based on if this is a polling command or not
                if (commandSet.IsPollingCommand ||
                    commandSet.IsNonStandardPollingCommand)
                {
                    // Polling commands will be added directly to the queue
                    AddCommandToPollingQueue(translatedCommand);
                }
                else
                {
                    // Non-polling commands can be sent immediatly or to the queue
                    // depending on the status of the device and driver
                    SendOrAddNonPollingCommand(translatedCommand);
                }
            }
            else
            {
                Log("Specified CommandSet object was null");
            }

            return false;
        }

        /// <summary>
        /// Marks a command as ready to send (no further processing is neeed). 
        /// This will then invoke Send with the commandSet.
        /// 
        /// This should be overridden by drivers to modify the CommandSet
        /// before it is ready to be sent to the device. 
        /// 
        /// If a driver sends the data directly to the device in this method, that driver
        /// needs to return true in their overridden method.
        /// </summary>
        /// <param name="commandSet">The command to get ready to set</param>
        /// <returns>True if this method sent the command to the device by the driver.</returns>
        protected virtual bool PrepareStringThenSend(CommandSet commandSet)
        {
            commandSet.CommandPrepared = true;
            if (commandSet.PrepareOnly)
            {
                return false;
            }
            else
            {
                return Send(commandSet);
            }
        }

        /// <summary>
        /// Work-around for backwards compatability. Allows for preparing a CommandSet
        /// without having it invoke Send.
        /// </summary>
        private bool PrepareString(Command commandQueueObject)
        {
            commandQueueObject.CommandSet.PrepareOnly = true;
            return (PrepareStringThenSend(commandQueueObject.CommandSet));
        }

        /// <summary>
        /// Sets the polling sequence the driver should use while the device is powered on.
        /// Polling commands will be prepared when they are about to be sent and unprepared afterwards.
        /// Only standard polling commands are allowed in the sequence.
        /// </summary>
        /// <param name="sequence">The standard polling commands that should be added to the polling queue</param>
        protected void SetPollingSequence(StandardCommandsEnum[] sequence)
        {
            if (sequence != null)
            {
                for (int i = 0; i < sequence.Length; i++)
                {
                    var comandGroup = FindCommandGroupFromStandardCommand(sequence[i]);
                    var commandSet = BuildCommand(
                        sequence[i],
                        comandGroup,
                        CommandPriority.Lowest,
                        string.Format("{0}_{1}",
                            comandGroup, sequence[i].ToString()));

                    if (commandSet.Exists())
                    {
                        if (commandSet.IsPollingCommand)
                        {
                            commandSet.CommandPrepared = false;

                            if (_commandId.Equals(ulong.MaxValue))
                            {
                                _commandId = ulong.MinValue;
                            }

                            // While the driver may support unsolicited feedback and we don't want persistent
                            // polling commands, they must stay persistent until they receive a response if that type of
                            // feedback is supported.

                            var queuedObject = new Command(_commandId, commandSet, true, true);
                            _commandId++;
                            queuedObject.CommandSet.CommandPriority = CommandPriority.Lowest;

                            // These are always treated as poll commands, so only add them to the polling queue
                            AddCommandToPollingQueue(queuedObject);
                        }
                        else if (EnableLogging)
                        {
                            Log(string.Format("Not adding {0} to polling sequence because it is not a valid polling command",
                                commandSet.CommandName));
                        }
                    }
                    else if (EnableLogging)
                    {
                        Log(string.Format("Unable to create polling command with StandardCommandEnum value {0}",
                            sequence[i]));
                    }
                }
            }
        }

        /// <summary>
        /// Do not call this in a driver.
        /// Only called by the device-type classes.
        /// Given a CommandSet, this will convert it to <see cref="Command"/> and make it
        /// a non-standard polling command even if the CommandSet is already a polling command.
        /// 
        /// This allows the framework to still add non-standard polling commands to the polling queue while preventing
        /// drivers from doing the same thing. Drivers need to override Poll() to perform non-standard polling.
        /// </summary>
        protected void AddPollingCommand(CommandSet commandSet)
        {
            if (commandSet.Exists())
            {
                if (_commandId.Equals(ulong.MaxValue))
                {
                    _commandId = ulong.MinValue;
                }

                var queuedObject = new Command(_commandId, commandSet, true, true);

                _commandId++;

                queuedObject.CommandSet.CommandPriority = CommandPriority.Lowest;
                queuedObject.CommandSet.IsNonStandardPollingCommand = true;
                AddCommandToPollingQueue(queuedObject);
            }
        }

        /// <summary>
        /// Used to send a command to the device. 
        /// 
        /// This will set to PendingRequest to the command if the command was not already sent
        /// This will always invoke the callback of the CommandSet.
        /// This will always set LastCommandGroup to the commandgroup of the command
        /// </summary>
        /// <param name="commandSet">The command to send</param>
        /// <param name="commandAlreadySent">Specifies if the command should be sent to the transport. 
        /// Drivers may send the command on their own within PrepareStringThenSend</param>
        internal void SendToTransport(CommandSet commandSet, bool commandAlreadySent)
        {
            if (commandAlreadySent == false)
            {
                PendingRequest = commandSet;
                if (EnableLogging)
                {
                    Log(string.Format(
                        "Sending to transport: {0} of group {1}",
                        commandSet.CommandName,
                        commandSet.CommandGroup));

                    // Check if there was an exception thrown on the Command setter
                    if (commandSet.CommandSetterError != null)
                    {
                        Log(string.Format(
                            "Exception occoured when command string was set on CommandSet name {0}: {1}",
                            commandSet.CommandName,
                            commandSet.CommandSetterError.Message));
                    }
                }

                Transport.Send(commandSet.Command, commandSet.Parameters);

                // Keeping track of when we last set PendingRequest
                _transmissionTickCountForPendingRequest = CrestronEnvironment.TickCount;
            }

            LastCommandGroup = commandSet.CommandGroup;

            // Perform the action needed when the command is sent. This is typically
            // used for Warmup/Cooldown in the framework
            if (commandSet.CallBack != null)
            {
                commandSet.CallBack();
            }

            // Fake feedback for the command if applicable
            if (commandSet.FakeFeedbackCallback != null)
            {
                commandSet.FakeFeedbackCallback(commandSet.StandardCommand, commandSet.CommandGroup);
            }
        }

        #endregion Driver methods

        #region Clock operations

        /// <summary>
        /// Method that is invoked by the static clock every 25ms.
        /// This method is in a non-shared CCriticalSection that it will try to enter, and if it is unable to enter, then it will exit. 
        /// This will remove any commands that have been marked for removal in the queue
        /// or that cannot be in the queue under current conditions. 
        /// Finally it will attempt to send a command to the device. Non-polling commands have the highest priority, but if the 
        /// first one cannot be sent, then a polling command will be sent instead if conditions allow for that.
        /// </summary>
        private void ProcessCommandQueue()
        {
            try
            {
                // If the driver is using TcpTransport, then we have to wait for the callback to the last TCPClient.SendDataAsync
                // before attempting to call it again.  See CCDRV-2673
                if (Transport is TcpTransport &&
                    ((TcpTransport)Transport).WaitingForAsyncSendCallback)
                {
                    // Last call to TcpTransport.SendData has not completed, wait for next call to this method 

                    // We could add a count-timeout here to prevent us from being stuck in this state, but if
                    // that callback is never invoked we will risk breaking transmission logic completely until a
                    // a reconnect/reboot/restart.
                    if (EnableLogging)
                    {
                        Log("Skipping command queue procesing because transport is busy");
                    }
                    return;
                }

                // Now check if we can send a command based on the value of TimeBetweenCommands
                // This should apply to all commands, no matter if they are polling or not.
                if (Math.Abs(CrestronEnvironment.TickCount - _lastSentCommandTick) < TimeBetweenCommands)
                {
                    // It is too soon to send anything, so exit now and process states when 
                    // it could be time to send something
                    return;
                }

                // If this driver needs to wait for a response before sending another command,
                // then this will stop processing if that response has not been received.
                if (WaitForResponse &&
                    PendingRequest.Exists())
                {
                    // Check to see if the last sent command has timed-out
                    if (Math.Abs(CrestronEnvironment.TickCount - _transmissionTickCountForPendingRequest) > TimeOut)
                    { // Timed out
                        PendingRequest = null;
                    }
                    else
                    { // Still waiting, don't send anything.
                        return;
                    }
                }

                // Get all commands that are eligible for sending
                var queuedNonPollingCommands = _nonPollingCommandQueue.Values.ToList();
                var queuedPollingCommands = _pollingCommandQueue.ToList();
                // Need to remove any commands marked for removal or commands that cannot exist in the queue
                // based on the device's and driver's states
                for (int i = 0; i < queuedNonPollingCommands.Count; i++)
                {
                    if (queuedNonPollingCommands[i].MarkedForRemoval)
                    {
                        RemoveCommandFromNonPollingQueue(queuedNonPollingCommands[i]);
                    }
                    else if (queuedNonPollingCommands[i].CommandSet.AllowRemoveCommandOverride ?
                        CanRemoveNonPollingCommandOverride(queuedNonPollingCommands[i].CommandSet,
                            queuedNonPollingCommands.FirstOrDefault(
                            x => x.CommandSet.StandardCommand.Equals(StandardCommandsEnum.PowerOn)) != null) :
                        CanRemoveNonPollingCommand(queuedNonPollingCommands[i], queuedNonPollingCommands))
                    {
                        RemoveCommandFromNonPollingQueue(queuedNonPollingCommands[i]);
                    }
                }

                // Removed any poll commands marked for removal
                if (_pollCommandMarkedForRemoval)
                {
                    for (int i = 0; i < queuedPollingCommands.Count; i++)
                    {
                        if (queuedPollingCommands[i].MarkedForRemoval)
                        {
                            RemoveCommandFromPollingQueue(queuedPollingCommands[i]);
                        }
                    }
                    _pollCommandMarkedForRemoval = false;
                }

                // Refresh snap-shots since there could have been removals
                queuedNonPollingCommands = _nonPollingCommandQueue.Values.ToList();
                queuedPollingCommands = _pollingCommandQueue.ToList();

                // We should be able to send a command at this moment in time
                // See if we can send a non-polling command, and if we can't, send a polling command
                if (queuedNonPollingCommands == null ||
                    queuedNonPollingCommands.Count == 0 ||
                    ProcessNonPollingCommandQueue(queuedNonPollingCommands) == false &&
                    queuedPollingCommands != null)
                {
                    ProcessPollingCommandQueue(queuedPollingCommands);
                }
            }
            catch (Exception e)
            {
                if (EnableLogging)
                {
                    Logger.Error(string.Format("ProcessCommandQueue -> {0}", e));
                }
            }
        }

        /// <summary>
        /// This will only look at the first possible command and if it is able to send it,
        /// it will return true and send it. If it is unable to do so, it will return false.
        /// 
        /// If the command supports fake-feedback, it will perform that opeartion before sending it,
        /// but after confirming it can send it.
        /// </summary>
        /// <param name="commands">List of non-polling commands</param>
        /// <returns>True if the command was sent to the transport</returns>
        private bool ProcessNonPollingCommandQueue(List<Command> commands)
        {
            // Pick the first command in the queue and see if we can send it right now
            // The count has to be greater than zero to get a non-null list, so no need for a null check
            Command commandToSend = commands[0];

            var canSendCommand = commandToSend.CommandSet.AllowIsSendableOverride
                    ? CanSendCommand(commandToSend.CommandSet)
                    : commandToSend.IsSendable(WarmingUp, CoolingDown, PowerIsOn, SupportsPowerFeedback);

            // Based on state of device, we might not be able to send the command
            if (canSendCommand)
            {
                // Note the time sent to prevent sending commands too fast to the device
                // based on the setting TimeBetweenCommands
                _lastSentCommandTick = CrestronEnvironment.TickCount;

                if (FakeFeedbackForCommand != null &&
                    commandToSend.IsFakeFeedbackCommand)
                {
                    // Fake feedback once the command is sent to the transport, not before by setting a callback on CommandSet
                    commandToSend.CommandSet.FakeFeedbackCallback = FakeFeedbackForCommand;
                }

                // Send it to the device
                SendToTransport(commandToSend.CommandSet, false);

                commandToSend.TransmissionTickCount = CrestronEnvironment.TickCount;

                // Mark it so that it be removed then next time the queue is maintained
                // Maintaining the queue happens before this method is invoke, so that will
                // prevent us from sending it multiple times
                // Mainly doing this to avoid entering a critical section again while in a method
                // that is invoked every 25ms
                commandToSend.MarkedForRemoval = true;
            }

            return canSendCommand;
        }

        /// <summary>
        /// This will iterate through all the polling commands until it finds one it can send.
        /// This will also check to see if any polling commands have timed out.
        /// The polling cycle begins at PollingInterval, but only after the last poll command in a sequence is sent.
        /// </summary>
        /// <param name="commands">List of polling commands</param>
        private void ProcessPollingCommandQueue(List<Command> commands)
        {
            var commandReceivedNoResponse = false;
            if (PollingEnabled == false)
            {
                // Polling is disabled so exit now.
                return;
            }
            if (_finishedPollSequence &&
                Math.Abs(CrestronEnvironment.TickCount - _lastPollCycleTick) < PollingInterval)
            {
                // Polling should only happen after the polling interval has passed after the lasts poll command was sent
                return;
            }
            else if (Math.Abs(CrestronEnvironment.TickCount - _lastSentCommandThatSkippedQueueTick) < _ticksToWaitToResumePolling)
            {
                // If a non-polling command was recently sent that also skipped the queue
                // then we must wait a little longer before resuming polling
                // to prevent polls in the middle of a ramp
                // Leave this method now, we can't send anything since something else was sent 
                // and they might keep sending more things
                return;
            }
            else
            {
                // We will need to pick out the first polling command in the sequence that can be sent.
                for (int i = 0; i < commands.Count; i++)
                {
                    if (i == 0)
                    {
                        _finishedPollSequence = false;
                    }
                    else if (i == commands.Count - 1)
                    {
                        _finishedPollSequence = true;
                        _lastPollCycleTick = CrestronEnvironment.TickCount;
                    }
                    // If the polling command was recently sent, then skip it.
                    if (Math.Abs(CrestronEnvironment.TickCount - commands[i].TransmissionTickCount) < PollingInterval)
                    {
                        // Skip it, it is too soon to send it
                        continue;
                    }
                    if (commands[i].CommandSet.AllowIsSendableOverride ? !CanSendCommand(commands[i].CommandSet) :
                        !commands[i].IsSendable(WarmingUp, CoolingDown, PowerIsOn, SupportsPowerFeedback))
                    {
                        // Skip it, it is not sendable
                        continue;
                    }
                    // This must be the one we can send right now
                    var pollCommand = commands[i];
                    // Preserve the original command string since BuildCommand won't always build the command
                    // if it was not in JSON the expected way.
                    string originalCommand = pollCommand.CommandSet.Command;
                    // Keep track of the driver sending the command in PrepareStringThenSend before we do
                    var alreadySentToTransport = PrepareString(pollCommand);
                    // Note transmission times
                    _lastSentCommandTick = CrestronEnvironment.TickCount;
                    // Send to the device (this method will not send it if it was already sent, but it has other stuff it needs to do)
                    SendToTransport(pollCommand.CommandSet, alreadySentToTransport);
                    if (pollCommand.Persistent)
                    {
                        // This operation no longer fails because the driver clock is now waiting for every listener
                        // to finish their operations before calling this method again.
                        pollCommand.CommandSet.Command = originalCommand;
                        pollCommand.CommandSet.CommandPrepared = false;
                        // Mark as waiting for feedback for timeout purposes, if applicable
                        if (pollCommand.SupportsFeedback)
                        {
                            if (pollCommand.WaitingForFeedback)
                            {
                                // Check if this command was already waiting for feedback and we have passed the timeout 
                                if (Math.Abs(CrestronEnvironment.TickCount - pollCommand.TransmissionTickCount) > TimeOut)
                                {
                                    MessageTimedOut(EnableLogging ? string.Format("Last attempt to send {0} did not get a valid response from the device",
                                        pollCommand.CommandSet.CommandName) : string.Empty);
                                }
                                commandReceivedNoResponse = true;
                            }
                            else
                            {
                                pollCommand.WaitingForFeedback = true;
                                if (pollCommand.TransmissionTickCount != 0 &&
                                    SupportsUnsolicitedFeedback == true &&
                                    _finishedPollSequence == true &&
                                    commandReceivedNoResponse == false)
                                {
                                    // All poll commands have received a response
                                    PollingEnabled = false;
                                    if (EnableLogging)
                                    {
                                        Log("Unsolicited feedback is true and the polling sequence has completed successfully at least once. Polling is now stopping.");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Remove from queue
                        pollCommand.MarkedForRemoval = true;
                        _pollCommandMarkedForRemoval = true;
                    }
                    pollCommand.TransmissionTickCount = CrestronEnvironment.TickCount;
                    break;
                }
            }
        }

        #endregion Clock operations

        #region Thread-Safe collection modifiers

        /// <summary>
        /// Thread-safe method to remove a non-polling comand from the queue
        /// </summary>
        private void RemoveCommandFromNonPollingQueue(Command command)
        {
            var removedEntry = false;
            var queuedCount = 0;
            try
            {
                _nonPollingCommandLock.Enter();

                if (_nonPollingCommandQueue.Count > 0)
                {
                    queuedCount = _nonPollingCommandQueue.Count;

                    var values = _nonPollingCommandQueue.Values;
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (values[i].Id == command.Id)
                        {
                            _nonPollingCommandQueue.Remove(i);
                            if (EnableLogging)
                            {
                                Log(string.Format("Removed queued command {0}", command.CommandSet.CommandName));
                            }
                            removedEntry = true;
                            break;
                        }
                    }
                }
                else if (EnableLogging)
                {
                    Log("Unable to remove command from queue - queue is empty");
                }
            }
            catch (Exception e)
            {
                if (EnableLogging)
                {
                    Log(string.Format("Unable to remove {0} command from queue - {1}", command.CommandSet.CommandName, e.Message));
                }
            }
            finally
            {
                _nonPollingCommandLock.Leave();
            }

            if (!removedEntry)
            {
                if (EnableLogging)
                {
                    Logger.Error(string.Format("Unable to remove command {0} from the queue because it was not found in the queue. - Queue count: {0}",
                        command == null || command.CommandSet == null ? "NULL COMMAND" : command.CommandSet.CommandName,
                        queuedCount));
                }
            }
        }

        /// <summary>
        /// Thread-safe method that will remove a polling command from the queue
        /// </summary>
        private void RemoveCommandFromPollingQueue(Command command)
        {
            try
            {
                _pollingCommandLock.Enter();

                if (_pollingCommandQueue.Count > 0)
                {
                    _pollingCommandQueue.Remove(command);

                    if (EnableLogging)
                    {
                        Log(string.Format("Removed queued command {0}", command.CommandSet.CommandName));
                    }
                }
                else if (EnableLogging)
                {
                    Log("Unable to remove command from queue - queue is empty");
                }
            }
            catch (Exception e)
            {
                if (EnableLogging)
                {
                    Log(string.Format("Unable to remove {0} command from queue - {1}",
                        command == null || command.CommandSet == null ? "NULL COMMAND" : command.CommandSet.CommandName,
                        e.Message));
                }
            }
            finally
            {
                _pollingCommandLock.Leave();
            }
        }

        /// <summary>
        /// Defines the rules for determining if a non-polling command can be removed from the device.
        /// </summary>
        /// <param name="command">The command that is being questioned</param>
        /// <param name="nonPollingCommands">Some rules require knowing what else is in the queue</param>
        /// <returns>True if the command can be removed</returns>
        private bool CanRemoveNonPollingCommand(Command command, List<Command> nonPollingCommands)
        {
            var canRemove = false;
            CommonCommandGroupType commandGroup = command.CommandSet.CommandGroup;

            if (!IsConnected)
            {
                if (!commandGroup.Equals(CommonCommandGroupType.Power) &&
                    !commandGroup.Equals(CommonCommandGroupType.Input) &&
                    !commandGroup.Equals(CommonCommandGroupType.MediaService) &&
                    !commandGroup.Equals(CommonCommandGroupType.AudioInput))
                {
                    canRemove = true;
                }
            }

            if (WarmingUp || CoolingDown)
            {
                if (WarmingUp &&
                   (!commandGroup.Equals(CommonCommandGroupType.Power) &&
                    !commandGroup.Equals(CommonCommandGroupType.Input)) &&
                    !commandGroup.Equals(CommonCommandGroupType.AudioInput) &&
                    !commandGroup.Equals(CommonCommandGroupType.MediaService))
                {
                    canRemove = true;
                }
                else if (CoolingDown)
                {
                    if (commandGroup.Equals(CommonCommandGroupType.Input) ||
                        commandGroup.Equals(CommonCommandGroupType.AudioInput) ||
                        commandGroup.Equals(CommonCommandGroupType.MediaService))
                    {
                        // Only remove the input commands if there is no pending PowerOn command in the queue
                        if (nonPollingCommands.FirstOrDefault(x => x.CommandSet.StandardCommand.Equals(StandardCommandsEnum.PowerOn)) == null)
                        {
                            canRemove = true;
                        }
                    }
                    else if (!commandGroup.Equals(CommonCommandGroupType.Power))
                    {
                        canRemove = true;
                    }
                }
            }
            else if (!PowerIsOn &&
                    SupportsPowerFeedback &&
                    (!commandGroup.Equals(CommonCommandGroupType.Power) &&
                    !commandGroup.Equals(CommonCommandGroupType.Input) &&
                    !commandGroup.Equals(CommonCommandGroupType.AudioInput) &&
                    !commandGroup.Equals(CommonCommandGroupType.MediaService)))
            {
                canRemove = true;
            }
            return canRemove;
        }

        /// <summary>
        /// Defines the rules for determining if a non-polling command can be removed from the device.
        /// </summary>
        /// <param name="commandSet">The command that is being questioned</param>
        /// <param name="powerOnCommandExistsInQueue">True if there is a PowerOn command in the queue</param>
        /// <returns>True if the command can be removed</returns>
        protected virtual bool CanRemoveNonPollingCommandOverride(CommandSet commandSet, bool powerOnCommandExistsInQueue)
        {
            return false;
        }


        /// <summary>
        /// This attemps to add a command to the polling queue. 
        /// If the standard command already exists in the queue, it will not replace it.
        /// </summary>
        /// <param name="command">The command to the add to the polling queue</param>
        /// <returns>True if the comand was added. False if the standard command already exists in the queue</returns>
        private bool AddCommandToPollingQueue(Command command)
        {
            var added = false;
            // We do not want to add a polling command that is already in the polling queue
            // If this is a non-standard command, then allow it to be added even if there are others
            // Otherwise look for a command with a matching commandname and standardcommand
            if (command.CommandSet.IsNonStandardPollingCommand ||
                _pollingCommandQueue.FirstOrDefault(
                x =>
                    x.CommandSet.StandardCommand.Equals(command.CommandSet.StandardCommand) &&
                    x.CommandSet.CommandName.Equals(command.CommandSet.CommandName)) == null)
            {
                try
                {
                    _pollingCommandLock.Enter();

                    _pollingCommandQueue.Add(command);
                    added = true;
                }
                catch (Exception e)
                {
                    if (EnableLogging)
                    {
                        Log(string.Format("Error adding polling command {0} of group {1} to queue: {2}",
                            command.CommandSet.CommandName, command.CommandSet.CommandGroup,
                            e.Message));
                    }
                }
                finally
                {
                    _pollingCommandLock.Leave();
                }
                if (added && EnableLogging)
                {
                    Log(string.Format("Added {0} command to group {1} and queue with a priority of {2}, ID {3}, and IsPersistent {4}",
                        command.CommandSet.CommandName, command.CommandSet.CommandGroup,
                        (int)command.CommandSet.CommandPriority,
                        command.Id,
                        command.Persistent));
                }
            }
            else
            {
                if (EnableLogging)
                {
                    Log(string.Format("Unable to add polling command {0} to queue since it already exists in the queue",
                        command.CommandSet.CommandName));
                }
            }
            return added;
        }

        /// <summary>
        /// Thread-safe method that will add a command to the non-polling queue.
        /// This will perform no checks before adding it.
        /// </summary>
        /// <returns>True if the command was addd to the non-polling queue</returns>
        private bool AddCommandToNonPollingQueue(Command command)
        {
            var added = false;
            try
            {
                _nonPollingCommandLock.Enter();

                _nonPollingCommandQueue.Add(command, (int)command.CommandSet.CommandPriority, command.Id);
                added = true;
            }
            catch (Exception e)
            {
                if (EnableLogging)
                {
                    Log(string.Format("Error adding non-polling command {0}  and group {1} to queue: {2}",
                        command.CommandSet.CommandName, command.CommandSet.CommandGroup,
                        e.Message));
                }
            }
            finally
            {
                _nonPollingCommandLock.Leave();
            }

            if (added && EnableLogging)
            {
                Log(string.Format("Added {0} command and group {1} to queue with a priority of {2} and ID {3}",
                    command.CommandSet.CommandName, command.CommandSet.CommandGroup,
                    (int)command.CommandSet.CommandPriority,
                    command.Id));
            }
            return added;
        }



        /// <summary>
        /// Finds the first occourence of the the command's StandardCommandEnum value and CommandGroup in the
        /// non-polling queue and replaces it with the given instance.
        /// </summary>
        /// <returns>True if something was modified</returns>
        private bool ModifyNonPollingQueueWithUpdatedStandardCommand(Command command)
        {
            var modified = false;
            try
            {
                _nonPollingCommandLock.Enter();

                var queuedValues = _nonPollingCommandQueue.Values;

                for (int i = 0; i < queuedValues.Length; i++)
                {
                    if (queuedValues[i].CommandSet.StandardCommand.Equals(command.CommandSet.StandardCommand) &&
                        queuedValues[i].CommandSet.CommandGroup.Equals(command.CommandSet.CommandGroup))
                    {
                        _nonPollingCommandQueue.Modify(i, command);
                        modified = true;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                if (EnableLogging)
                {
                    Logger.Error(string.Format("Unable to modify queue with updated standard command {0} : {1}",
                        command.CommandSet.CommandName,
                        e));
                }
            }
            finally
            {
                _nonPollingCommandLock.Leave();
            }

            if (EnableLogging)
            {
                if (modified)
                {
                    Log(string.Format("Modified queue by Standard Command with updated {0} command",
                        command.CommandSet.StandardCommand));
                }
                else
                {
                    Log(string.Format("Unable to modify queue with standard command {0} because it does not exist",
                         command.CommandSet.CommandName));
                }
            }
            return modified;
        }
        /// <summary>
        /// Finds the first occourence of the the command's CommandGroup value in the
        /// non-polling queue and replaces it with the given instance.
        /// </summary>
        /// <param name="command">CommandGroup of this object is used to find a match in the queue and replace it</param>
        /// <returns>True if something was modified</returns>
        private bool ModifyNonPollingQueueWithUpdatedCommandGroup(Command command)
        {
            var modified = false;
            try
            {
                _nonPollingCommandLock.Enter();

                var queuedValues = _nonPollingCommandQueue.Values;

                for (int i = 0; i < queuedValues.Length; i++)
                {
                    if (queuedValues[i].CommandSet.CommandGroup.Equals(command.CommandSet.CommandGroup))
                    {
                        _nonPollingCommandQueue.Modify(i, command);
                        modified = true;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                if (EnableLogging)
                {
                    Logger.Error(string.Format("Unable to modify queue by command group with updated command {0} : {1}",
                        command.CommandSet.CommandName,
                        e));
                }
            }
            finally
            {
                _nonPollingCommandLock.Leave();
            }

            if (EnableLogging)
            {
                if (modified)
                {
                    Log(string.Format("Modified queue by command group with updated {0} command",
                        command.CommandSet.CommandName));
                }
                else
                {
                    Log(string.Format("Unable to modify queue by command group with {0} command: no matches to command group found {1}",
                        command.CommandSet.CommandName, command.CommandSet.CommandGroup.ToString()));
                }
            }

            return modified;
        }

        /// <summary>
        /// Finds the first occourence of the the command's SubCommandGroup value in the
        /// non-polling queue and replaces it with the given instance. 
        /// This allows for sharing commands between command groups with priority.
        /// </summary>
        /// <param name="command">CommandGroup of this object is used to find a match in the queue and replace it</param>
        /// <returns>True if something was modified</returns>
        private bool ModifyNonPollingQueueWithUpdatedSubCommandGroup(Command command)
        {
            var modified = false;
            try
            {
                _nonPollingCommandLock.Enter();

                var queuedValues = _nonPollingCommandQueue.Values;

                for (int i = 0; i < queuedValues.Length; i++)
                {
                    if (queuedValues[i].CommandSet.CommandGroup.Equals(command.CommandSet.CommandGroup) &&
                        queuedValues[i].CommandSet.SubCommandGroup.Equals(command.CommandSet.SubCommandGroup))
                    {
                        _nonPollingCommandQueue.Modify(i, command);
                        modified = true;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                if (EnableLogging)
                {
                    Logger.Error(string.Format("Unable to modify queue by command group with updated command {0} : {1}",
                        command.CommandSet.CommandName,
                        e));
                }
            }
            finally
            {
                _nonPollingCommandLock.Leave();
            }

            if (EnableLogging)
            {
                if (modified)
                {
                    Log(string.Format("Modified queue by command group with updated {0} command",
                        command.CommandSet.CommandName));
                }
                else
                {
                    Log(string.Format("Unable to modify queue by commnd group with {0} command: no matches to command group found",
                        command.CommandSet.CommandName));
                }
            }
            return modified;
        }


        /// <summary>
        /// Finds the first occourence of the the command's CommandGroup value in the
        /// non-polling queue and replaces it with the given instance. If it can't find that command group,
        /// it will attempt the alternate command group. 
        /// This allows for sharing commands between command groups with priority.
        /// </summary>
        /// <param name="command">CommandGroup of this object is used to find a match in the queue and replace it</param>
        /// <param name="alternateTargetGroup">If the CommandGroup of the object cannot be found, this group is used instead</param>
        /// <returns>True if something was modified</returns>
        private bool ModifyNonPollingQueueWithUpdatedCommandGroup(Command command, CommonCommandGroupType alternateTargetGroup)
        {
            var modified = false;
            try
            {
                _nonPollingCommandLock.Enter();

                var queuedValues = _nonPollingCommandQueue.Values;

                for (int i = 0; i < queuedValues.Length; i++)
                {
                    if (queuedValues[i].CommandSet.CommandGroup.Equals(command.CommandSet.CommandGroup) ||
                        queuedValues[i].CommandSet.CommandGroup.Equals(alternateTargetGroup))
                    {
                        _nonPollingCommandQueue.Modify(i, command);
                        modified = true;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                if (EnableLogging)
                {
                    Logger.Error(string.Format("Unable to modify queue by command group with updated command {0} : {1}",
                        command.CommandSet.CommandName,
                        e));
                }
            }
            finally
            {
                _nonPollingCommandLock.Leave();
            }

            if (EnableLogging)
            {
                if (modified)
                {
                    Log(string.Format("Modified queue by command group with updated {0} command",
                        command.CommandSet.CommandName));
                }
                else
                {
                    Log(string.Format("Unable to modify queue by commnd group with {0} command: no matches to command group found",
                        command.CommandSet.CommandName));
                }
            }

            return modified;
        }

        #endregion Thread-Safe collection modifiers

        /// <summary>
        /// This will send the non-polling command to directly to the transport if the queue is empty,
        /// TimeBetweenCommands is being honored, and the command is sendable based on business rules.
        /// 
        /// If it cannot be sent, then this will attempt to queue the command if it is allowed by business rules.
        /// </summary>
        /// <param name="command">The command to send/queue</param>
        private void SendOrAddNonPollingCommand(Command command)
        {
            var queuedNonPollingCommands = _nonPollingCommandQueue.Values.ToList();

            // If this driver is currently waiting for a response, then
            // this command cannot be sent right now
            var waitingForResponse = WaitForResponse && PendingRequest != null;

            // First check if the queue is empty, and if it is and we can send it, then just send it
            // without touching the queue
            if (waitingForResponse == false &&
                queuedNonPollingCommands.Count == 0 &&
                Math.Abs(CrestronEnvironment.TickCount - _lastSentCommandTick) >= TimeBetweenCommands &&
                (command.CommandSet.AllowIsSendableOverride ? CanSendCommand(command.CommandSet) == true :
                command.IsSendable(WarmingUp, CoolingDown, PowerIsOn, SupportsPowerFeedback) == true))
            {

                if (EnableLogging)
                {
                    Log(string.Format("Skipping queue and sending command directly to device: {0} of group {1}",
                        command.CommandSet.CommandName,
                        command.CommandSet.CommandGroup));
                }
                // Note the time sent to prevent sending commands too fast to the device
                // based on the setting TimeBetweenCommands
                _lastSentCommandTick = CrestronEnvironment.TickCount;

                // Set this flag to prevent polling in the middle of a ramp
                // Since we can skip the queue, and polling is determined by the queue being empty
                // We can run into cases where some poll commands get sent out in between ramps
                // which is bad since we slow down our ramp as a result if TimeBetweenCommands > 0
                _lastSentCommandThatSkippedQueueTick = CrestronEnvironment.TickCount;

                if (FakeFeedbackForCommand != null &&
                    command.IsFakeFeedbackCommand)
                {
                    // Fake feedback once the command is sent to the transport, not before by setting a callback on CommandSet
                    command.CommandSet.FakeFeedbackCallback = FakeFeedbackForCommand;
                }

                // Send it to the device
                SendToTransport(command.CommandSet, false);

                command.TransmissionTickCount = CrestronEnvironment.TickCount;
            }
            else if (command.CommandSet.AllowIsQueueableOverride ?
                CanQueueCommand(command.CommandSet, queuedNonPollingCommands.FirstOrDefault(
                x => x.CommandSet.StandardCommand.Equals(StandardCommandsEnum.PowerOn)) != null) :
                command.IsQueueable(WarmingUp, CoolingDown, PowerIsOn, SupportsPowerFeedback))
            {
                if (command.IsReplaceableByCommandGroup)
                {
                    var modified = false;
                    if (command.AlternateCommandGroup == CommonCommandGroupType.Unknown)
                    {
                        if (command.CommandSet.SubCommandGroup == CommonCommandGroupType.Unknown)
                        {
                            modified = ModifyNonPollingQueueWithUpdatedCommandGroup(command);
                        }
                        else
                        {
                            modified = ModifyNonPollingQueueWithUpdatedSubCommandGroup(command);
                        }
                    }
                    else
                    {
                        modified = ModifyNonPollingQueueWithUpdatedCommandGroup(command, command.CommandSet.CommandGroup);
                    }
                    if (!modified)
                    {
                        AddCommandToNonPollingQueue(command);
                    }   
                }
                else if (command.IsReplaceableByStandardCommand)
                {
                    if (ModifyNonPollingQueueWithUpdatedStandardCommand(command) == false)
                    {
                        AddCommandToNonPollingQueue(command);
                    }
                }
                else
                {
                    AddCommandToNonPollingQueue(command);
                }
            }
            else if (EnableLogging)
            {
                if (Logger.CurrentLevel == Crestron.Panopto.Common.Logging.LoggingLevel.Debug)
                {
                    Logger.Debug(string.Format("Unable to send/queue command {0}",
                        command.CommandSet.CommandName));

                    var isSendableLogMessage = string.Empty;
                    var isQueueableLogMessage = string.Empty;

                    var isSendable = command.IsSendable(WarmingUp, CoolingDown, PowerIsOn, SupportsPowerFeedback, true, out isSendableLogMessage);
                    var isQueueable = command.IsQueueable(WarmingUp, CoolingDown, PowerIsOn, SupportsPowerFeedback, true, out isQueueableLogMessage);

                    if (!isSendable)
                        Logger.Debug(string.Format("Command {0} is not sendable: {1}",
                            command.CommandSet.CommandName,
                            isSendableLogMessage));

                    if (!isQueueable)
                        Logger.Debug(string.Format("Command {0} is not queueable: {1}",
                            command.CommandSet.CommandName,
                            isQueueableLogMessage));
                }
                else
                {
                    Log(string.Format("Unable to send/queue command {0}", command.CommandSet.CommandName));
                }
            }
        }

        /// <summary>
        /// Resolves the CommonCommandGroupType from a standard command that is a polling command.
        /// </summary>
        /// <param name="standardCommand">Polling command</param>
        /// <returns>Matching command group for that polling command</returns>
        private CommonCommandGroupType FindCommandGroupFromStandardCommand(StandardCommandsEnum standardCommand)
        {
            CommonCommandGroupType commandGroup = CommonCommandGroupType.Unknown;

            switch (standardCommand)
            {
                case StandardCommandsEnum.PowerPoll:
                    commandGroup = CommonCommandGroupType.Power;
                    break;
                case StandardCommandsEnum.InputPoll:
                    commandGroup = CommonCommandGroupType.Input;
                    break;
                case StandardCommandsEnum.VolumePoll:
                    commandGroup = CommonCommandGroupType.Volume;
                    break;
                case StandardCommandsEnum.MutePoll:
                    commandGroup = CommonCommandGroupType.Mute;
                    break;
                case StandardCommandsEnum.VideoMutePoll:
                    commandGroup = CommonCommandGroupType.VideoMute;
                    break;
                case StandardCommandsEnum.TunerFrequencyPoll:
                    commandGroup = CommonCommandGroupType.TunerFrequency;
                    break;
                case StandardCommandsEnum.TrackPoll:
                    commandGroup = CommonCommandGroupType.TrackFeedback;
                    break;
                case StandardCommandsEnum.TrackRemainingTimePoll:
                    commandGroup = CommonCommandGroupType.TrackRemainingTime;
                    break;
                case StandardCommandsEnum.TrackElapsedTimePoll:
                    commandGroup = CommonCommandGroupType.TrackElapsedTime;
                    break;
                case StandardCommandsEnum.TotalRemainingTimePoll:
                    commandGroup = CommonCommandGroupType.TotalRemainingTime;
                    break;
                case StandardCommandsEnum.TotalElapsedTimePoll:
                    commandGroup = CommonCommandGroupType.TotalElapsedTime;
                    break;
                case StandardCommandsEnum.ToneTreblePoll:
                    commandGroup = CommonCommandGroupType.Treble;
                    break;
                case StandardCommandsEnum.ToneStatePoll:
                    commandGroup = CommonCommandGroupType.ToneState;
                    break;
                case StandardCommandsEnum.ToneBassPoll:
                    commandGroup = CommonCommandGroupType.Bass;
                    break;
                case StandardCommandsEnum.SurroundModePoll:
                    commandGroup = CommonCommandGroupType.SurroundMode;
                    break;
                case StandardCommandsEnum.SelfViewPoll:
                    commandGroup = CommonCommandGroupType.Selfview;
                    break;
                case StandardCommandsEnum.PlayBackStatusPoll:
                    commandGroup = CommonCommandGroupType.PlayBackStatus;
                    break;
                /*  case StandardCommandsEnum.PipLocationPoll:
                      break;*/
                case StandardCommandsEnum.OnScreenDisplayPoll:
                    commandGroup = CommonCommandGroupType.OnScreenDisplay;
                    break;
                /*  case StandardCommandsEnum.MicMutePoll:
                      break;*/
                case StandardCommandsEnum.LoudnessPoll:
                    commandGroup = CommonCommandGroupType.Loudness;
                    break;
                case StandardCommandsEnum.LampHoursPoll:
                    commandGroup = CommonCommandGroupType.LampHours;
                    break;
                case StandardCommandsEnum.EnergyStarPoll:
                    commandGroup = CommonCommandGroupType.EnergyStar;
                    break;
                case StandardCommandsEnum.ChapterRemainingTimePoll:
                    commandGroup = CommonCommandGroupType.ChapterRemainingTime;
                    break;
                case StandardCommandsEnum.ChapterPoll:
                    commandGroup = CommonCommandGroupType.ChapterFeedback;
                    break;
                case StandardCommandsEnum.ChapterElapsedTimePoll:
                    commandGroup = CommonCommandGroupType.ChapterElapsedTime;
                    break;
                case StandardCommandsEnum.ChannelPoll:
                    commandGroup = CommonCommandGroupType.Channel;
                    break;
                /* case StandardCommandsEnum.AvPoll:
                     break;*/
                case StandardCommandsEnum.AudioInputPoll:
                    commandGroup = CommonCommandGroupType.AudioInput;
                    break;
                case StandardCommandsEnum.RequestAreaInformation:
                    commandGroup = CommonCommandGroupType.MonitoringAreaInfo;
                    break;
                case StandardCommandsEnum.RequestAreaResourceStatus:
                    commandGroup = CommonCommandGroupType.MonitoringAreaResourceStatus;
                    break;
                case StandardCommandsEnum.RequestPermissionArea:
                    commandGroup = CommonCommandGroupType.MonitoringResourcePermissionArea;
                    break;
                case StandardCommandsEnum.RequestAreaCount:
                    commandGroup = CommonCommandGroupType.MonitoringAreaCount;
                    break;
                case StandardCommandsEnum.RequestOutputInformation:
                    commandGroup = CommonCommandGroupType.MonitoringOutputInfo;
                    break;
                case StandardCommandsEnum.RequestOutputResourceStatus:
                    commandGroup = CommonCommandGroupType.MonitoringOutputResourceStatus;
                    break;
                case StandardCommandsEnum.RequestOutputCount:
                    commandGroup = CommonCommandGroupType.MonitoringOutputCount;
                    break;
                case StandardCommandsEnum.RequestPermissionOutput:
                    commandGroup = CommonCommandGroupType.MonitoringResourcePermissionOutput;
                    break;
                case StandardCommandsEnum.RequestZoneInformation:
                    commandGroup = CommonCommandGroupType.MonitoringZoneInfo;
                    break;
                case StandardCommandsEnum.RequestZoneResourceStatus:
                    commandGroup = CommonCommandGroupType.MonitoringZoneResourceStatus;
                    break;
                case StandardCommandsEnum.RequestZoneCount:
                    commandGroup = CommonCommandGroupType.MonitoringZoneCount;
                    break;
                case StandardCommandsEnum.RequestPermissionZone:
                    commandGroup = CommonCommandGroupType.MonitoringResourcePermissionZone;
                    break;
                case StandardCommandsEnum.RequestDeviceInformation:
                    commandGroup = CommonCommandGroupType.MonitoringDeviceInfo;
                    break;
                case StandardCommandsEnum.RequestDeviceResourceStatus:
                    commandGroup = CommonCommandGroupType.MonitoringDeviceResourceStatus;
                    break;
                case StandardCommandsEnum.RequestDeviceCount:
                    commandGroup = CommonCommandGroupType.MonitoringDeviceCount;
                    break;
                case StandardCommandsEnum.RequestPermissionDevice:
                    commandGroup = CommonCommandGroupType.MonitoringResourcePermissionDevice;
                    break;
                case StandardCommandsEnum.RequestBypassedResources:
                    commandGroup = CommonCommandGroupType.MonitoringBypassedResources;
                    break;
                case StandardCommandsEnum.RequestDoorInformation:
                    commandGroup = CommonCommandGroupType.MonitoringDoorInfo;
                    break;
                case StandardCommandsEnum.RequestDoorResourceStatus:
                    commandGroup = CommonCommandGroupType.MonitoringDoorResourceStatus;
                    break;
                case StandardCommandsEnum.RequestPermissionDoor:
                    commandGroup = CommonCommandGroupType.MonitoringResourcePermissionDoor;
                    break;
            }

            return commandGroup;
        }

        /// <summary>
        /// This will mark the appropriate command in the queue as having received feedback.
        /// </summary>
        private void AcknowledgeValidatedFeedback(CommonCommandGroupType commandGroup, string customCommandGroup)
        {
            Command queuedCommand = null;
            switch (commandGroup)
            {
                case CommonCommandGroupType.AvrZone1:
                case CommonCommandGroupType.AvrZone2:
                case CommonCommandGroupType.AvrZone3:
                case CommonCommandGroupType.AvrZone4:
                case CommonCommandGroupType.AvrZone5:
                    queuedCommand = _pollingCommandQueue.FirstOrDefault(x =>
                        x.CommandSet.CommandGroup.Equals(commandGroup) &&
                        x.CommandSet.CommandName.Equals(customCommandGroup));
                    break;
                default:
                    queuedCommand = _pollingCommandQueue.FirstOrDefault(x => x.CommandSet.CommandGroup.Equals(commandGroup));
                    break;
            }



            if (queuedCommand != null)
            {
                queuedCommand.WaitingForFeedback = false;
            }
        }

        /// <summary>
        /// Method to determine if a command can be sent, bypassing the command queue
        /// This method should be overridden in the SDK for devices for more specific rules to be applied
        /// </summary>
        /// <param name="commandSet">commandSet to evaluate if command can be sent</param>
        /// <returns>True if successful</returns>
        protected virtual bool CanSendCommand(CommandSet commandSet)
        {
            return true;
        }

        /// <summary>
        /// Method to determine if a command can be queued
        /// This method should be overridden in the SDK for devices for more specific rules to be applied
        /// </summary>
        /// <param name="commandSet">commandSet to evaluate if command can be queued</param>
        /// <returns>True if successful</returns>
        protected virtual bool CanQueueCommand(CommandSet commandSet, bool powerOnCommandInQueue)
        {
            return true;
        }
    }
}