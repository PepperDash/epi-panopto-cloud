using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Crestron.RAD.Common;
using Crestron.RAD.Common.Interfaces;
using Crestron.SimplSharp.CrestronDataStore;
using Crestron.RAD.Common.Helpers;
using Crestron.RAD.Common.Enums;

namespace RADCommonUnitTests
{
    [TestClass]
    public class VolumeHelperTests
    {
        public VolumeHelperTests()
        {
            
        }

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

        [TestMethod]
        public void VolumeHelper_ScaleVolume_WhenVolumeDetailNotRamping25Percent()
        {
            try
            {
                uint expectedValue = 25;
                uint result = VolumeHelper.ScaleVolume(new VolumeDetail(25, 0, 100, false, RampingVolumeState.None, 25));
                Assert.IsFalse(!(expectedValue == result), "VolumeHelper.ScaleVolume method failed WhenVolumeDetailNotRamping25Percent test.");
            }
            catch (System.Exception)
            {
                Assert.Fail("WhenVolumeDetailNotRamping25Percent test failed");
            }
        }

        [TestMethod]
        public void VolumeHelper_ScaleVolume_WhenVolumeDetailRangeBelow100NotRamping100Percent()
        {
            try
            {
                uint expectedValue = 75;
                uint result = VolumeHelper.ScaleVolume(new VolumeDetail(100, 5, 75, false, RampingVolumeState.None, 15));
                Assert.IsFalse(!(expectedValue == result), "VolumeHelper.ScaleVolume method failed WhenVolumeDetailRangeBelow100NotRamping100Percent test.");
            }
            catch (System.Exception)
            {
                Assert.Fail("WhenVolumeDetailRangeBelow100NotRamping100Percent test failed");
            }
        }

        [TestMethod]
        public void VolumeHelper_ScaleVolume_WhenVolumeDetailRampingUp30Percent()
        {
            try
            {
                uint expectedValue = 31; //30 + 1 from ramping up between 0 and 100
                uint result = VolumeHelper.ScaleVolume(new VolumeDetail(30, 0, 100, true, RampingVolumeState.Up, 30));
                Assert.IsFalse(!(expectedValue == result), "VolumeHelper.ScaleVolume method failed WhenVolumeDetailRampingUp30Percent test.");
            }
            catch (System.Exception)
            {
                Assert.Fail("WhenVolumeDetailRampingUp30Percent test failed");
            }
        }

        [TestMethod]
        public void VolumeHelper_ScaleVolume_WhenVolumeDetailUnscaledValueIsDifferentThenScaled()
        {
            try
            {
                uint expectedValue = 60;
                uint result = VolumeHelper.ScaleVolume(new VolumeDetail(60, 0, 100, false, RampingVolumeState.None, 17));
                Assert.IsFalse(!(expectedValue == result), "VolumeHelper.ScaleVolume method failed WhenVolumeDetailUnscaledValueIsDifferentThenScaled test.");
            }
            catch (System.Exception)
            {
                Assert.Fail("WhenVolumeDetailUnscaledValueIsDifferentThenScaled test failed");
            }
        }

        [TestMethod]
        public void VolumeHelper_ScaleVolume_WhenVolumeDetailNotRampingInversedMinMaxValues()
        {
            try
            {
                uint expectedValue = 0;
                uint result = VolumeHelper.ScaleVolume(new VolumeDetail(49, 100, 0, false, RampingVolumeState.None, 49));
                Assert.IsFalse(!(expectedValue == result), "VolumeHelper.ScaleVolume method failed WhenVolumeDetailNotRampingInversedMinMaxValues test.");
            }
            catch (System.Exception)
            {
                Assert.Fail("WhenVolumeDetailNotRampingInversedMinMaxValues test failed");
            }
        }

        [TestMethod]
        public void VolumeHelper_ScaleVolume_WhenVolumeDetailRampingDownInversedMinMaxValues()
        {
            try
            {
                uint expectedValue = 0;
                uint result = VolumeHelper.ScaleVolume(new VolumeDetail(45, 100, 0, true, RampingVolumeState.Down, 45));
                Assert.IsFalse(!(expectedValue == result), "VolumeHelper.ScaleVolume method failed WhenVolumeDetailRampingDownInversedMinMaxValues test.");
            }
            catch (System.Exception)
            {
                Assert.Fail("WhenVolumeDetailRampingDownInversedMinMaxValues test failed");
            }
        }

        [TestMethod]
        public void VolumeHelper_ScaleVolume_WhenVolumeDetailRampingUpInversedMinMaxValues()
        {
            try
            {
                uint expectedValue = 0;
                uint result = VolumeHelper.ScaleVolume(new VolumeDetail(49, 100, 0, true, RampingVolumeState.Up, 49));
                Assert.IsFalse(!(expectedValue == result), "VolumeHelper.ScaleVolume method failed WhenVolumeDetailRampingUpInversedMinMaxValues test.");
            }
            catch (System.Exception)
            {
                Assert.Fail("WhenVolumeDetailRampingUpInversedMinMaxValues test failed");
            }
        }

        [TestMethod]
        public void VolumeHelper_ScaleVolume_WhenVolumeDetailRampingDataUnallignedNone()
        {
            try
            {
                uint expectedValue = 50;
                uint result = VolumeHelper.ScaleVolume(new VolumeDetail(50, 0, 100, true, RampingVolumeState.None, 50));
                Assert.IsFalse(!(expectedValue == result), "VolumeHelper.ScaleVolume method failed WhenVolumeDetailRampingDataUnallignedNone test.");
            }
            catch (System.Exception)
            {
                Assert.Fail("WhenVolumeDetailRampingDataUnallignedNone test failed");
            }
        }

        [TestMethod]
        public void VolumeHelper_ScaleVolume_WhenVolumeDetailRampingDataUnallignedUp()
        {
            try
            {
                uint expectedValue = 50;
                uint result = VolumeHelper.ScaleVolume(new VolumeDetail(50, 0, 100, false, RampingVolumeState.Up, 50));
                Assert.IsFalse(!(expectedValue == result), "VolumeHelper.ScaleVolume method failed WhenVolumeDetailRampingDataUnallignedUp test.");
            }
            catch (System.Exception)
            {
                Assert.Fail("WhenVolumeDetailRampingDataUnallignedUp test failed");
            }
        }

        [TestMethod]
        public void VolumeHelper_ScaleVolume_WhenVolumeDetailRampingDataUnallignedDown()
        {
            try
            {
                uint expectedValue = 50;
                uint result = VolumeHelper.ScaleVolume(new VolumeDetail(50, 0, 100, false, RampingVolumeState.Down, 50));
                Assert.IsFalse(!(expectedValue == result), "VolumeHelper.ScaleVolume method failed WhenVolumeDetailRampingDataUnallignedDown test.");
            }
            catch (System.Exception)
            {
                Assert.Fail("WhenVolumeDetailRampingDataUnallignedUp test failed");
            }
        }

        [TestMethod]
        public void VolumeHelper_ScaleVolume_WhenVolumeDetailNull()
        {
            try
            {
                //if the method ScaleVolume does not pass a error back to this method and instead returns a volume check to make sure it isn't sending
                //back bogus data with a value greater then zero.
                VolumeDetail volumeDetail = null;
                uint expectedValue = 0;
                uint result = VolumeHelper.ScaleVolume(volumeDetail);
                Assert.IsFalse(!(expectedValue == result), "VolumeHelper.ScaleVolume method failed WhenVolumeDetailNull test.");
            }
            catch (System.Exception)
            {
                Assert.Fail("WhenVolumeDetailNull test failed");
            }
        }
    }
}
