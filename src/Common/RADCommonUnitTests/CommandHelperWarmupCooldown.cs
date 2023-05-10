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
    /// Summary description for CommandHelperWarmupCooldown
    /// </summary>
    [TestClass]
    public class CommandHelperWarmupCooldown
    {
        public CommandHelperWarmupCooldown()
        {
            _powerOnCommand = TestBuildCommand(StandardCommandsEnum.PowerOn, CommonCommandGroupType.Power,
                CommandPriority.Highest);
            _powerOffCommand = TestBuildCommand(StandardCommandsEnum.PowerOff, CommonCommandGroupType.Power,
                CommandPriority.Highest);
            _powerCommand = TestBuildCommand(StandardCommandsEnum.Power, CommonCommandGroupType.Power,
                CommandPriority.Highest);

            _testCommandBatch = new Dictionary<CommandSet, TestResult>();
            _testCommandBatch.Add(_powerOnCommand, new TestResult());
            _testCommandBatch.Add(_powerOffCommand, new TestResult());
            _testCommandBatch.Add(_powerCommand, new TestResult());
        }

        private CommandSet _powerOnCommand;
        private CommandSet _powerOffCommand;
        private CommandSet _powerCommand;

        private Dictionary<CommandSet, TestResult> _testCommandBatch;

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
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

        private bool HasPassed(CommandSet set)
        {
            return _testCommandBatch.ContainsKey(set) && _testCommandBatch[set].IsPassed;
        }

        private CommandSet TestBuildCommand(StandardCommandsEnum commandEnum,
                CommonCommandGroupType group,
                CommandPriority priority)
        {
            return new CommandSet(commandEnum.ToString(),
                "Testing " + commandEnum, group,
                null, false, priority, commandEnum);
        }

        private void RunWarmupTests(WarmupCallbackVariables variables)
        {
            foreach (var test in _testCommandBatch)
            {
                var command = test.Key;
                test.Value.IsPassed = CommandHelper.HandleWarmupCallback(variables, ref command);
            }
        }

        private void RunCooldownTests(CoolingCallbackVariables variables)
        {
            foreach (var test in _testCommandBatch)
            {
                var command = test.Key;
                test.Value.IsPassed = CommandHelper.HandleCooldownCallback(variables, ref command);
            }
        }

        /// <summary>
        /// If no callback is available to be set, no callback CAN be set. (without local timer)
        /// </summary>
        [TestMethod]
        public void HandleWarmupCallbackWhilePowerOnWithoutLocalTimerAndNoCallbackTest()
        {
            RunWarmupTests(new WarmupCallbackVariables
            {
                HasPower = true,
                Callback = null,
                IsWarmingUp = false,
                SupportsLocalTimer = false
            });

            Assert.IsFalse(HasPassed(_powerCommand));
            Assert.IsFalse(HasPassed(_powerOnCommand));
            Assert.IsFalse(HasPassed(_powerOffCommand));
        }

        /// <summary>
        /// If no callback is available to be set, no callback CAN be set. (with local timer)
        /// </summary>
        [TestMethod]
        public void HandleWarmupCallbackWhilePowerOnWithTimerAndNoCallbackTest()
        {
            RunWarmupTests(new WarmupCallbackVariables
            {
                HasPower = true,
                Callback = null,
                IsWarmingUp = false,
                SupportsLocalTimer = true
            });

            Assert.IsFalse(HasPassed(_powerCommand));
            Assert.IsFalse(HasPassed(_powerOnCommand));
            Assert.IsFalse(HasPassed(_powerOffCommand));
        }

        [TestMethod]
        public void HandleWarmupCallbackWhilePowerOnWithLocalTimerTest()
        {
            RunWarmupTests(new WarmupCallbackVariables
            {
                HasPower = true,
                Callback = () => { },
                IsWarmingUp = false,
                SupportsLocalTimer = true
            });

            Assert.IsFalse(HasPassed(_powerCommand));
            Assert.IsFalse(HasPassed(_powerOnCommand));
            Assert.IsFalse(HasPassed(_powerOffCommand));
        }

        [TestMethod]
        public void HandleWarmupCallbackWhilePowerOnWithoutLocalTimerTest()
        {
            RunWarmupTests(new WarmupCallbackVariables
            {
                HasPower = true,
                Callback = () => { },
                IsWarmingUp = false,
                SupportsLocalTimer = false
            });

            Assert.IsFalse(HasPassed(_powerCommand));
            Assert.IsFalse(HasPassed(_powerOnCommand));
            Assert.IsFalse(HasPassed(_powerOffCommand));
        }

        [TestMethod]
        public void HandleWarmupCallbackWhilePowerOffWithLocalTimerTest()
        {
            RunWarmupTests(new WarmupCallbackVariables
            {
                HasPower = false,
                Callback = () => { },
                IsWarmingUp = false,
                SupportsLocalTimer = true
            });

            Assert.IsTrue(HasPassed(_powerCommand));
            Assert.IsTrue(HasPassed(_powerOnCommand));
            Assert.IsTrue(HasPassed(_powerOffCommand));
        }

        [TestMethod]
        public void HandleWarmupCallbackWhilePowerOffWithoutLocalTimerTest()
        {
            RunWarmupTests(new WarmupCallbackVariables
            {
                HasPower = false,
                Callback = () => { },
                IsWarmingUp = false,
                SupportsLocalTimer = false
            });

            Assert.IsFalse(HasPassed(_powerCommand));
            Assert.IsFalse(HasPassed(_powerOnCommand));
            Assert.IsFalse(HasPassed(_powerOffCommand));
        }

                /// <summary>
        /// If no callback is available to be set, no callback CAN be set. (without local timer)
        /// </summary>
        [TestMethod]
        public void HandleCooldownCallbackWhilePowerOnWithoutLocalTimerAndNoCallbackTest()
        {
            RunCooldownTests(new CoolingCallbackVariables
            {
                HasPower = true,
                Callback = null,
                IsCoolingDown = false,
                SupportsLocalTimer = false
            });

            Assert.IsFalse(HasPassed(_powerCommand));
            Assert.IsFalse(HasPassed(_powerOnCommand));
            Assert.IsFalse(HasPassed(_powerOffCommand));
        }

        /// <summary>
        /// If no callback is available to be set, no callback CAN be set. (with local timer)
        /// </summary>
        [TestMethod]
        public void HandleCooldownCallbackWhilePowerOnWithTimerAndNoCallbackTest()
        {
            RunCooldownTests(new CoolingCallbackVariables
            {
                HasPower = true,
                Callback = null,
                IsCoolingDown = false,
                SupportsLocalTimer = true
            });

            Assert.IsFalse(HasPassed(_powerCommand));
            Assert.IsFalse(HasPassed(_powerOnCommand));
            Assert.IsFalse(HasPassed(_powerOffCommand));
        }

        [TestMethod]
        public void HandleCooldownCallbackWhilePowerOnWithLocalTimerTest()
        {
            RunCooldownTests(new CoolingCallbackVariables
            {
                HasPower = true,
                Callback = () => { },
                IsCoolingDown = false,
                SupportsLocalTimer = true
            });

            Assert.IsTrue(HasPassed(_powerCommand));
            Assert.IsTrue(HasPassed(_powerOnCommand));
            Assert.IsTrue(HasPassed(_powerOffCommand));
        }

        [TestMethod]
        public void HandleCooldownCallbackWhilePowerOnWithoutLocalTimerTest()
        {
            RunCooldownTests(new CoolingCallbackVariables
            {
                HasPower = true,
                Callback = () => { },
                IsCoolingDown = false,
                SupportsLocalTimer = false
            });

            Assert.IsFalse(HasPassed(_powerCommand));
            Assert.IsFalse(HasPassed(_powerOnCommand));
            Assert.IsFalse(HasPassed(_powerOffCommand));
        }

        [TestMethod]
        public void HandleCooldownCallbackWhilePowerOffWithLocalTimerTest()
        {
            RunCooldownTests(new CoolingCallbackVariables
            {
                HasPower = false,
                Callback = () => { },
                IsCoolingDown = false,
                SupportsLocalTimer = true
            });

            Assert.IsFalse(HasPassed(_powerCommand));
            Assert.IsFalse(HasPassed(_powerOnCommand));
            Assert.IsFalse(HasPassed(_powerOffCommand));
        }

        [TestMethod]
        public void HandleCooldownCallbackWhilePowerOffWithoutLocalTimerTest()
        {
            RunCooldownTests(new CoolingCallbackVariables
            {
                HasPower = false,
                Callback = () => { },
                IsCoolingDown = false,
                SupportsLocalTimer = false
            });

            Assert.IsFalse(HasPassed(_powerCommand));
            Assert.IsFalse(HasPassed(_powerOnCommand));
            Assert.IsFalse(HasPassed(_powerOffCommand));
        }
    }
}
