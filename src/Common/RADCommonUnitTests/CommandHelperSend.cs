// Copyright (C) 2017 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System.Collections.Generic;
using Crestron.RAD.Common.BasicDriver;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.Common.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Crestron.RAD.Common.UnitTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class CommandHelperSend
    {
        public CommandHelperSend()
        {
            _testCommandBatch = new Dictionary<CommandSet, SendCommandResult>();
            _powerOnCommand = TestBuildCommand(StandardCommandsEnum.PowerOn,
                CommonCommandGroupType.Power,
                CommandPriority.Highest);
            _powerOffCommand = TestBuildCommand(StandardCommandsEnum.PowerOff,
                CommonCommandGroupType.Power,
                CommandPriority.Highest);
            _powerPollCommand = TestBuildCommand(StandardCommandsEnum.PowerPoll,
                CommonCommandGroupType.Power,
                CommandPriority.Low);
            _inputPollCommand = TestBuildCommand(StandardCommandsEnum.InputPoll,
                CommonCommandGroupType.Input,
                CommandPriority.Highest);
            _input3Command = TestBuildCommand(StandardCommandsEnum.Input3,
                CommonCommandGroupType.Input,
                CommandPriority.Highest);
            _hdmi1Command = TestBuildCommand(StandardCommandsEnum.Hdmi1,
                CommonCommandGroupType.Input,
                CommandPriority.Highest);
            _pendingPowerOnCommand = TestBuildCommand(StandardCommandsEnum.Power,
                CommonCommandGroupType.Power,
                CommandPriority.High);

            _testCommandBatch.Add(_powerOnCommand, new SendCommandResult());
            _testCommandBatch.Add(_powerOffCommand, new SendCommandResult());
            _testCommandBatch.Add(_powerPollCommand, new SendCommandResult());
            _testCommandBatch.Add(_inputPollCommand, new SendCommandResult());
            _testCommandBatch.Add(_input3Command, new SendCommandResult());
            _testCommandBatch.Add(_hdmi1Command, new SendCommandResult());
        }

        private CommandSet _powerOnCommand;
        private CommandSet _powerOffCommand;
        private CommandSet _powerPollCommand;
        private CommandSet _inputPollCommand;
        private CommandSet _input3Command;
        private CommandSet _hdmi1Command;
        private CommandSet _pendingPowerOnCommand;

        private CommandSet TestBuildCommand(StandardCommandsEnum commandEnum,
            CommonCommandGroupType group,
            CommandPriority priority)
        {
            return new CommandSet(commandEnum.ToString(),
                "Testing " + commandEnum, group,
                null, false, priority, commandEnum);
        }

        private Dictionary<CommandSet, SendCommandResult> _testCommandBatch;
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>c
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        #region Additional test attributes

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //

        #endregion

        private bool TransportBound(CommandSet set)
        {
            return _testCommandBatch.ContainsKey(set)
                && _testCommandBatch[set].SendToTransport;
        }

        private bool QueueBound(CommandSet set)
        {
            return _testCommandBatch.ContainsKey(set)
                && _testCommandBatch[set].SendToQueue;
        }

        private void RunSendTest(SendCommandVariables variables)
        {
            foreach (var test in _testCommandBatch)
            {
                var sendResult = CommandHelper.Send(variables, test.Key);

                test.Value.SendToQueue = sendResult.SendToQueue;
                test.Value.SendToTransport = sendResult.SendToTransport;
            }            
        }

        [TestMethod]
        public void SendingWithoutQueueWhenPowerIsOffAndDriverIsWarmingUpTest()
        {
            RunSendTest(new SendCommandVariables
            {
                CanSendCommands = true,
                CoolingDown = false,
                WarmingUp = true,
                DriverLoaded = true,
                HasPower = false,
                Queue = false,
                PendingRequest = null
            });

            Assert.IsTrue(TransportBound(_powerOnCommand));
            Assert.IsTrue(TransportBound(_powerOffCommand));
            Assert.IsTrue(TransportBound(_powerPollCommand));
            Assert.IsFalse(TransportBound(_inputPollCommand));
            Assert.IsFalse(TransportBound(_input3Command));
            Assert.IsFalse(TransportBound(_hdmi1Command));

            Assert.IsFalse(QueueBound(_powerOnCommand));
            Assert.IsFalse(QueueBound(_powerOffCommand));
            Assert.IsFalse(QueueBound(_powerPollCommand));
            Assert.IsFalse(QueueBound(_inputPollCommand));
            Assert.IsFalse(QueueBound(_input3Command));
            Assert.IsFalse(QueueBound(_hdmi1Command));
        }

        [TestMethod]
        public void SendingWithQueueWhenPowerIsOffAndDriverIsWarmingUpTest()
        {
            RunSendTest(new SendCommandVariables
            {
                CanSendCommands = true,
                CoolingDown = false,
                WarmingUp = true,
                DriverLoaded = true,
                HasPower = false,
                Queue = true,
                PendingRequest = null
            });
            Assert.IsFalse(TransportBound(_powerOnCommand));
            Assert.IsFalse(TransportBound(_powerOffCommand));
            Assert.IsTrue(TransportBound(_powerPollCommand));
            Assert.IsFalse(TransportBound(_inputPollCommand));
            Assert.IsFalse(TransportBound(_input3Command));
            Assert.IsFalse(TransportBound(_hdmi1Command));

            Assert.IsFalse(QueueBound(_powerOnCommand));
            Assert.IsFalse(QueueBound(_powerOffCommand));
            Assert.IsFalse(QueueBound(_powerPollCommand));
            Assert.IsFalse(QueueBound(_inputPollCommand));
            Assert.IsTrue(QueueBound(_input3Command));
            Assert.IsTrue(QueueBound(_hdmi1Command));
        }

        [TestMethod]
        public void SendingWithoutQueueWhenPowerIsOffAndDriverIsCoolingDownTest()
        {
            RunSendTest(new SendCommandVariables
            {
                CanSendCommands = true,
                CoolingDown = true,
                WarmingUp = false,
                DriverLoaded = true,
                HasPower = false,
                Queue = false,
                PendingRequest = null
            });
            Assert.IsTrue(TransportBound(_powerOnCommand));
            Assert.IsTrue(TransportBound(_powerOffCommand));
            Assert.IsTrue(TransportBound(_powerPollCommand));
            Assert.IsFalse(TransportBound(_inputPollCommand));
            Assert.IsFalse(TransportBound(_input3Command));
            Assert.IsFalse(TransportBound(_hdmi1Command));

            Assert.IsFalse(QueueBound(_powerOnCommand));
            Assert.IsFalse(QueueBound(_powerOffCommand));
            Assert.IsFalse(QueueBound(_powerPollCommand));
            Assert.IsFalse(QueueBound(_inputPollCommand));
            Assert.IsFalse(QueueBound(_input3Command));
            Assert.IsFalse(QueueBound(_hdmi1Command));
        }

        [TestMethod]
        public void SendingWithQueueWhenPowerIsOffAndDriverIsCoolingDownTest()
        {
            RunSendTest(new SendCommandVariables
            {
                CanSendCommands = true,
                CoolingDown = true,
                WarmingUp = false,
                DriverLoaded = true,
                HasPower = false,
                Queue = true,
                PendingRequest = null
            });
            Assert.IsFalse(TransportBound(_powerOnCommand));
            Assert.IsFalse(TransportBound(_powerOffCommand));
            Assert.IsTrue(TransportBound(_powerPollCommand));
            Assert.IsFalse(TransportBound(_inputPollCommand));
            Assert.IsFalse(TransportBound(_input3Command));
            Assert.IsFalse(TransportBound(_hdmi1Command));

            Assert.IsFalse(QueueBound(_powerOnCommand));
            Assert.IsFalse(QueueBound(_powerOffCommand));
            Assert.IsFalse(QueueBound(_powerPollCommand));
            Assert.IsFalse(QueueBound(_inputPollCommand));
            Assert.IsFalse(QueueBound(_input3Command));
            Assert.IsFalse(QueueBound(_hdmi1Command));
        }

        [TestMethod]
        public void SendingWithoutQueueWhenPowerIsOnAndDriverIsWarmingUpTest()
        {
            RunSendTest(new SendCommandVariables
            {
                CanSendCommands = true,
                CoolingDown = false,
                WarmingUp = true,
                DriverLoaded = true,
                HasPower = true,
                Queue = false,
                PendingRequest = null
            });
            Assert.IsTrue(TransportBound(_powerOnCommand));
            Assert.IsTrue(TransportBound(_powerOffCommand));
            Assert.IsTrue(TransportBound(_powerPollCommand));
            Assert.IsTrue(TransportBound(_inputPollCommand));
            Assert.IsTrue(TransportBound(_input3Command));
            Assert.IsTrue(TransportBound(_hdmi1Command));

            Assert.IsFalse(QueueBound(_powerOnCommand));
            Assert.IsFalse(QueueBound(_powerOffCommand));
            Assert.IsFalse(QueueBound(_powerPollCommand));
            Assert.IsFalse(QueueBound(_inputPollCommand));
            Assert.IsFalse(QueueBound(_input3Command));
            Assert.IsFalse(QueueBound(_hdmi1Command));
        }

        [TestMethod]
        public void SendingWithQueueWhenPowerIsOnAndDriverIsWarmingUpTest()
        {
            RunSendTest(new SendCommandVariables
            {
                CanSendCommands = true,
                CoolingDown = false,
                WarmingUp = true,
                DriverLoaded = true,
                HasPower = true,
                Queue = true,
                PendingRequest = null
            });
            Assert.IsFalse(TransportBound(_powerOnCommand));
            Assert.IsFalse(TransportBound(_powerOffCommand));
            Assert.IsTrue(TransportBound(_powerPollCommand));
            Assert.IsFalse(TransportBound(_inputPollCommand));
            Assert.IsFalse(TransportBound(_input3Command));
            Assert.IsFalse(TransportBound(_hdmi1Command));

            Assert.IsFalse(QueueBound(_powerOnCommand));
            Assert.IsFalse(QueueBound(_powerOffCommand));
            Assert.IsFalse(QueueBound(_powerPollCommand));
            Assert.IsFalse(QueueBound(_inputPollCommand));
            Assert.IsTrue(QueueBound(_input3Command));
            Assert.IsTrue(QueueBound(_hdmi1Command));
        }

        [TestMethod]
        public void SendingWithoutQueueWhenPowerIsOnAndDriverIsCoolingDownTest()
        {
            RunSendTest(new SendCommandVariables
            {
                CanSendCommands = true,
                CoolingDown = true,
                WarmingUp = false,
                DriverLoaded = true,
                HasPower = true,
                Queue = false,
                PendingRequest = null
            });
            Assert.IsTrue(TransportBound(_powerOnCommand));
            Assert.IsTrue(TransportBound(_powerOffCommand));
            Assert.IsTrue(TransportBound(_powerPollCommand));
            Assert.IsTrue(TransportBound(_inputPollCommand));
            Assert.IsTrue(TransportBound(_input3Command));
            Assert.IsTrue(TransportBound(_hdmi1Command));

            Assert.IsFalse(QueueBound(_powerOnCommand));
            Assert.IsFalse(QueueBound(_powerOffCommand));
            Assert.IsFalse(QueueBound(_powerPollCommand));
            Assert.IsFalse(QueueBound(_inputPollCommand));
            Assert.IsFalse(QueueBound(_input3Command));
            Assert.IsFalse(QueueBound(_hdmi1Command));
        }

        [TestMethod]
        public void SendingWithoutQueueWhenPowerIsOnAndDriverIsNotWarmingAndNotCoolingTest()
        {
            RunSendTest(new SendCommandVariables
            {
                CanSendCommands = true,
                CoolingDown = false,
                WarmingUp = false,
                DriverLoaded = true,
                HasPower = true,
                Queue = false,
                PendingRequest = null
            });
            Assert.IsTrue(TransportBound(_powerOnCommand));
            Assert.IsTrue(TransportBound(_powerOffCommand));
            Assert.IsTrue(TransportBound(_powerPollCommand));
            Assert.IsTrue(TransportBound(_inputPollCommand));
            Assert.IsTrue(TransportBound(_input3Command));
            Assert.IsTrue(TransportBound(_hdmi1Command));

            Assert.IsFalse(QueueBound(_powerOnCommand));
            Assert.IsFalse(QueueBound(_powerOffCommand));
            Assert.IsFalse(QueueBound(_powerPollCommand));
            Assert.IsFalse(QueueBound(_inputPollCommand));
            Assert.IsFalse(QueueBound(_input3Command));
            Assert.IsFalse(QueueBound(_hdmi1Command));
        }

        [TestMethod]
        public void SendingWithoutQueueWhenPowerIsOffAndDriverIsNotWarmingAndNotCoolingTest()
        {
            RunSendTest(new SendCommandVariables
            {
                CanSendCommands = true,
                CoolingDown = false,
                WarmingUp = false,
                DriverLoaded = true,
                HasPower = false,
                Queue = false,
                PendingRequest = null
            });
            Assert.IsTrue(TransportBound(_powerOnCommand));
            Assert.IsTrue(TransportBound(_powerOffCommand));
            Assert.IsTrue(TransportBound(_powerPollCommand));
            Assert.IsFalse(TransportBound(_inputPollCommand));
            Assert.IsFalse(TransportBound(_input3Command));
            Assert.IsFalse(TransportBound(_hdmi1Command));

            Assert.IsFalse(QueueBound(_powerOnCommand));
            Assert.IsFalse(QueueBound(_powerOffCommand));
            Assert.IsFalse(QueueBound(_powerPollCommand));
            Assert.IsFalse(QueueBound(_inputPollCommand));
            Assert.IsFalse(QueueBound(_input3Command));
            Assert.IsFalse(QueueBound(_hdmi1Command));
        }

        [TestMethod]
        public void SendingWithQueueWhenPowerIsOnAndDriverIsCoolingDownTest()
        {
            RunSendTest(new SendCommandVariables
            {
                CanSendCommands = true,
                CoolingDown = true,
                WarmingUp = false,
                DriverLoaded = true,
                HasPower = true,
                Queue = true,
                PendingRequest = null
            });
            Assert.IsFalse(TransportBound(_powerOnCommand));
            Assert.IsFalse(TransportBound(_powerOffCommand));
            Assert.IsTrue(TransportBound(_powerPollCommand));
            Assert.IsFalse(TransportBound(_inputPollCommand));
            Assert.IsFalse(TransportBound(_input3Command));
            Assert.IsFalse(TransportBound(_hdmi1Command));

            Assert.IsFalse(QueueBound(_powerOnCommand));
            Assert.IsFalse(QueueBound(_powerOffCommand));
            Assert.IsFalse(QueueBound(_powerPollCommand));
            Assert.IsFalse(QueueBound(_inputPollCommand));
            Assert.IsFalse(QueueBound(_input3Command));
            Assert.IsFalse(QueueBound(_hdmi1Command));
        }

        [TestMethod]
        public void SendingWithDriverNotLoadedAndPowerOnTest()
        {
            RunSendTest(new SendCommandVariables
            {
                CanSendCommands = true,
                CoolingDown = false,
                WarmingUp = false,
                DriverLoaded = false,
                HasPower = true,
                Queue = false,
                PendingRequest = null
            });
            Assert.IsFalse(TransportBound(_powerOnCommand));
            Assert.IsFalse(TransportBound(_powerOffCommand));
            Assert.IsFalse(TransportBound(_powerPollCommand));
            Assert.IsFalse(TransportBound(_inputPollCommand));
            Assert.IsFalse(TransportBound(_input3Command));
            Assert.IsFalse(TransportBound(_hdmi1Command));

            Assert.IsFalse(QueueBound(_powerOnCommand));
            Assert.IsFalse(QueueBound(_powerOffCommand));
            Assert.IsFalse(QueueBound(_powerPollCommand));
            Assert.IsFalse(QueueBound(_inputPollCommand));
            Assert.IsFalse(QueueBound(_input3Command));
            Assert.IsFalse(QueueBound(_hdmi1Command));
        }

        [TestMethod]
        public void SendingWithDriverNotLoadedAndPowerOffTest()
        {
            RunSendTest(new SendCommandVariables
            {
                CanSendCommands = true,
                CoolingDown = false,
                WarmingUp = false,
                DriverLoaded = false,
                HasPower = false,
                Queue = false,
                PendingRequest = null
            });
            Assert.IsFalse(TransportBound(_powerOnCommand));
            Assert.IsFalse(TransportBound(_powerOffCommand));
            Assert.IsFalse(TransportBound(_powerPollCommand));
            Assert.IsFalse(TransportBound(_inputPollCommand));
            Assert.IsFalse(TransportBound(_input3Command));
            Assert.IsFalse(TransportBound(_hdmi1Command));

            Assert.IsFalse(QueueBound(_powerOnCommand));
            Assert.IsFalse(QueueBound(_powerOffCommand));
            Assert.IsFalse(QueueBound(_powerPollCommand));
            Assert.IsFalse(QueueBound(_inputPollCommand));
            Assert.IsFalse(QueueBound(_input3Command));
            Assert.IsFalse(QueueBound(_hdmi1Command));
        }

        [TestMethod]
        public void SendingWhenCannotSendCommandsWithoutQueueTest()
        {
            RunSendTest(new SendCommandVariables
            {
                CanSendCommands = false,
                CoolingDown = false,
                WarmingUp = false,
                DriverLoaded = true,
                HasPower = false,
                Queue = false,
                PendingRequest = null
            });
            Assert.IsTrue(TransportBound(_powerOnCommand));
            Assert.IsTrue(TransportBound(_powerOffCommand));
            Assert.IsTrue(TransportBound(_powerPollCommand));
            Assert.IsFalse(TransportBound(_inputPollCommand));
            Assert.IsFalse(TransportBound(_input3Command));
            Assert.IsFalse(TransportBound(_hdmi1Command));

            Assert.IsFalse(QueueBound(_powerOnCommand));
            Assert.IsFalse(QueueBound(_powerOffCommand));
            Assert.IsFalse(QueueBound(_powerPollCommand));
            Assert.IsFalse(QueueBound(_inputPollCommand));
            Assert.IsFalse(QueueBound(_input3Command));
            Assert.IsFalse(QueueBound(_hdmi1Command));
        }

        [TestMethod]
        public void SendingWhenCannotSendCommandsWithQueueTest()
        {
            RunSendTest(new SendCommandVariables
            {
                CanSendCommands = false,
                CoolingDown = false,
                WarmingUp = false,
                DriverLoaded = true,
                HasPower = false,
                Queue = true,
                PendingRequest = null
            });
            Assert.IsTrue(TransportBound(_powerOnCommand));
            Assert.IsTrue(TransportBound(_powerOffCommand));
            Assert.IsTrue(TransportBound(_powerPollCommand));
            Assert.IsFalse(TransportBound(_inputPollCommand));
            Assert.IsFalse(TransportBound(_input3Command));
            Assert.IsFalse(TransportBound(_hdmi1Command));

            Assert.IsFalse(QueueBound(_powerOnCommand));
            Assert.IsFalse(QueueBound(_powerOffCommand));
            Assert.IsFalse(QueueBound(_powerPollCommand));
            Assert.IsFalse(QueueBound(_inputPollCommand));
            Assert.IsFalse(QueueBound(_input3Command));
            Assert.IsFalse(QueueBound(_hdmi1Command));
        }

        [TestMethod]
        public void SendingWithQueueWhenPowerIsOnAndNotWarmingAndNotCoolingWithNoPendingRequestTest()
        {
            RunSendTest(new SendCommandVariables
            {
                CanSendCommands = true,
                CoolingDown = false,
                WarmingUp = false,
                DriverLoaded = true,
                HasPower = true,
                Queue = true,
                PendingRequest = null
            });
            Assert.IsTrue(TransportBound(_powerOnCommand));
            Assert.IsTrue(TransportBound(_powerOffCommand));
            Assert.IsTrue(TransportBound(_powerPollCommand));
            Assert.IsTrue(TransportBound(_inputPollCommand));
            Assert.IsTrue(TransportBound(_input3Command));
            Assert.IsTrue(TransportBound(_hdmi1Command));

            Assert.IsFalse(QueueBound(_powerOnCommand));
            Assert.IsFalse(QueueBound(_powerOffCommand));
            Assert.IsFalse(QueueBound(_powerPollCommand));
            Assert.IsFalse(QueueBound(_inputPollCommand));
            Assert.IsFalse(QueueBound(_input3Command));
            Assert.IsFalse(QueueBound(_hdmi1Command));
        }

        [TestMethod]
        public void SendingWithQueueWhenPowerIsOffAndNotWarmingAndNotCoolingWithNoPendingRequestTest()
        {
            RunSendTest(new SendCommandVariables
            {
                CanSendCommands = true,
                CoolingDown = false,
                WarmingUp = false,
                DriverLoaded = true,
                HasPower = false,
                Queue = true,
                PendingRequest = null
            });
            Assert.IsTrue(TransportBound(_powerOnCommand));
            Assert.IsTrue(TransportBound(_powerOffCommand));
            Assert.IsTrue(TransportBound(_powerPollCommand));
            Assert.IsFalse(TransportBound(_inputPollCommand));
            Assert.IsFalse(TransportBound(_input3Command));
            Assert.IsFalse(TransportBound(_hdmi1Command));

            Assert.IsFalse(QueueBound(_powerOnCommand));
            Assert.IsFalse(QueueBound(_powerOffCommand));
            Assert.IsFalse(QueueBound(_powerPollCommand));
            Assert.IsFalse(QueueBound(_inputPollCommand));
            Assert.IsFalse(QueueBound(_input3Command));
            Assert.IsFalse(QueueBound(_hdmi1Command));
        }

        [TestMethod]
        public void SendingWithQueueWhenPowerIsOnAndNotWarmingAndNotCoolingWithPendingRequestTest()
        {
            RunSendTest(new SendCommandVariables
            {
                CanSendCommands = true,
                CoolingDown = false,
                WarmingUp = false,
                DriverLoaded = true,
                HasPower = true,
                Queue = true,
                PendingRequest = _pendingPowerOnCommand
            });
            Assert.IsFalse(TransportBound(_powerOnCommand));
            Assert.IsFalse(TransportBound(_powerOffCommand));
            Assert.IsFalse(TransportBound(_powerPollCommand));
            Assert.IsFalse(TransportBound(_inputPollCommand));
            Assert.IsFalse(TransportBound(_input3Command));
            Assert.IsFalse(TransportBound(_hdmi1Command));

            Assert.IsTrue(QueueBound(_powerOnCommand));
            Assert.IsTrue(QueueBound(_powerOffCommand));
            Assert.IsTrue(QueueBound(_powerPollCommand));
            Assert.IsTrue(QueueBound(_inputPollCommand));
            Assert.IsTrue(QueueBound(_input3Command));
            Assert.IsTrue(QueueBound(_hdmi1Command));
        }

        [TestMethod]
        public void SendingWithQueueWhenPowerIsOffAndNotWarmingAndNotCoolingWithPendingRequestTest()
        {
            RunSendTest(new SendCommandVariables
            {
                CanSendCommands = true,
                CoolingDown = false,
                WarmingUp = false,
                DriverLoaded = true,
                HasPower = false,
                Queue = true,
                PendingRequest = _pendingPowerOnCommand
            });
            Assert.IsTrue(TransportBound(_powerOnCommand));
            Assert.IsTrue(TransportBound(_powerOffCommand));
            Assert.IsTrue(TransportBound(_powerPollCommand));
            Assert.IsFalse(TransportBound(_inputPollCommand));
            Assert.IsFalse(TransportBound(_input3Command));
            Assert.IsFalse(TransportBound(_hdmi1Command));

            Assert.IsFalse(QueueBound(_powerOnCommand));
            Assert.IsFalse(QueueBound(_powerOffCommand));
            Assert.IsFalse(QueueBound(_powerPollCommand));
            Assert.IsFalse(QueueBound(_inputPollCommand));
            Assert.IsFalse(QueueBound(_input3Command));
            Assert.IsFalse(QueueBound(_hdmi1Command));
        }
    }
}
