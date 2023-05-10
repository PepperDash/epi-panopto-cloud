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
    public class AudioToneHelperTests
    {
        public AudioToneHelperTests()
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
        public void AudioToneHelper_ScaleAudioTone_WhenAudioToneDetailNotRamping25Percent()
        {
            try
            {
                uint expectedValue = 25;
                uint result = AudioToneHelper.ScaleAudioTone(new AudioToneDetail(25, 100, 0, false, RampingVolumeState.None, 25));
                Assert.IsFalse(!(expectedValue == result), "AudioToneHelper.ScaleAudioTone method failed WhenAudioToneDetailNotRamping25Percent test.");
            }
            catch (System.Exception)
            {
                Assert.Fail("WhenAudioToneDetailNotRamping25Percent test failed");
            }
        }

        [TestMethod]
        public void AudioToneHelper_ScaleAudioTone_WhenAudioToneDetailRangeBelow100NotRamping100Percent()
        {
            try
            {
                uint expectedValue = 75;
                uint result = AudioToneHelper.ScaleAudioTone(new AudioToneDetail(100, 75, 5, false, RampingVolumeState.None, 15));
                Assert.IsFalse(!(expectedValue == result), "AudioToneHelper.ScaleAudioTone method failed WhenAudioToneDetailRangeBelow100NotRamping100Percent test.");
            }
            catch (System.Exception)
            {
                Assert.Fail("WhenAudioToneDetailRangeBelow100NotRamping100Percent test failed");
            }
        }

        [TestMethod]
        public void AudioToneHelper_ScaleAudioTone_WhenAudioToneDetailRampingUp30Percent()
        {
            try
            {
                uint expectedValue = 31; //30 + 1 from ramping up between 0 and 100
                uint result = AudioToneHelper.ScaleAudioTone(new AudioToneDetail(30, 100, 0, true, RampingVolumeState.Up, 30));
                Assert.IsFalse(!(expectedValue == result), "AudioToneHelper.ScaleAudioTone method failed WhenAudioToneDetailRampingUp30Percent test.");
            }
            catch (System.Exception)
            {
                Assert.Fail("WhenAudioToneDetailRampingUp30Percent test failed");
            }
        }

        [TestMethod]
        public void AudioToneHelper_ScaleAudioTone_WhenAudioToneDetailUnscaledValueIsDifferentThenScaled()
        {
            try
            {
                uint expectedValue = 60;
                uint result = AudioToneHelper.ScaleAudioTone(new AudioToneDetail(60, 100, 0, false, RampingVolumeState.None, 17));
                Assert.IsFalse(!(expectedValue == result), "AudioToneHelper.ScaleAudioTone method failed WhenAudioToneDetailUnscaledValueIsDifferentThenScaled test.");
            }
            catch (System.Exception)
            {
                Assert.Fail("WhenAudioToneDetailUnscaledValueIsDifferentThenScaled test failed");
            }
        }

        [TestMethod]
        public void AudioToneHelper_ScaleAudioTone_WhenAudioToneDetailNotRampingInversedMinMaxValues()
        {
            try
            {
                uint expectedValue = 0;
                uint result = AudioToneHelper.ScaleAudioTone(new AudioToneDetail(49, 0, 100, false, RampingVolumeState.None, 49));
                Assert.IsFalse(!(expectedValue == result), "AudioToneHelper.ScaleAudioTone method failed WhenAudioToneDetailNotRampingInversedMinMaxValues test.");
            }
            catch (System.Exception)
            {
                Assert.Fail("WhenAudioToneDetailNotRampingInversedMinMaxValues test failed");
            }
        }

        [TestMethod]
        public void AudioToneHelper_ScaleAudioTone_WhenAudioToneDetailRampingDownInversedMinMaxValues()
        {
            try
            {
                uint expectedValue = 0;
                uint result = AudioToneHelper.ScaleAudioTone(new AudioToneDetail(45, 0, 100, true, RampingVolumeState.Down, 45));
                Assert.IsFalse(!(expectedValue == result), "AudioToneHelper.ScaleAudioTone method failed WhenAudioToneDetailRampingDownInversedMinMaxValues test.");
            }
            catch (System.Exception)
            {
                Assert.Fail("WhenAudioToneDetailRampingDownInversedMinMaxValues test failed");
            }
        }

        [TestMethod]
        public void AudioToneHelper_ScaleAudioTone_WhenAudioToneDetailRampingUpInversedMinMaxValues()
        {
            try
            {
                uint expectedValue = 0;
                uint result = AudioToneHelper.ScaleAudioTone(new AudioToneDetail(49, 0, 100, true, RampingVolumeState.Up, 49));
                Assert.IsFalse(!(expectedValue == result), "AudioToneHelper.ScaleAudioTone method failed WhenAudioToneDetailRampingUpInversedMinMaxValues test.");
            }
            catch (System.Exception)
            {
                Assert.Fail("WhenAudioToneDetailRampingUpInversedMinMaxValues test failed");
            }
        }

        [TestMethod]
        public void AudioToneHelper_ScaleAudioTone_WhenAudioToneDetailRampingDataUnallignedNone()
        {
            try
            {
                uint expectedValue = 50;
                uint result = AudioToneHelper.ScaleAudioTone(new AudioToneDetail(50, 100, 0, true, RampingVolumeState.None, 50));
                Assert.IsFalse(!(expectedValue == result), "AudioToneHelper.ScaleAudioTone method failed WhenAudioToneDetailRampingDataUnallignedNone test.");
            }
            catch (System.Exception)
            {
                Assert.Fail("WhenAudioToneDetailRampingDataUnallignedNone test failed");
            }
        }

        [TestMethod]
        public void AudioToneHelper_ScaleAudioTone_WhenAudioToneDetailRampingDataUnallignedUp()
        {
            try
            {
                uint expectedValue = 50;
                uint result = AudioToneHelper.ScaleAudioTone(new AudioToneDetail(50, 100, 0, false, RampingVolumeState.Up, 50));
                Assert.IsFalse(!(expectedValue == result), "AudioToneHelper.ScaleAudioTone method failed WhenAudioToneDetailRampingDataUnallignedUp test.");
            }
            catch (System.Exception)
            {
                Assert.Fail("WhenAudioToneDetailRampingDataUnallignedUp test failed");
            }
        }

        [TestMethod]
        public void AudioToneHelper_ScaleAudioTone_WhenAudioToneDetailRampingDataUnallignedDown()
        {
            try
            {
                uint expectedValue = 50;
                uint result = AudioToneHelper.ScaleAudioTone(new AudioToneDetail(50, 100, 0, false, RampingVolumeState.Down, 50));
                Assert.IsFalse(!(expectedValue == result), "AudioToneHelper.ScaleAudioTone method failed WhenAudioToneDetailRampingDataUnallignedDown test.");
            }
            catch (System.Exception)
            {
                Assert.Fail("WhenAudioToneDetailRampingDataUnallignedUp test failed");
            }
        }

        [TestMethod]
        public void AudioToneHelper_ScaleAudioTone_WhenAudioToneDetailNull()
        {
            try
            {
                AudioToneDetail audioToneDetail = null;
                uint expectedValue = 0;
                uint result = AudioToneHelper.ScaleAudioTone(audioToneDetail);
                Assert.IsFalse(!(expectedValue == result), "AudioToneHelper.ScaleVolume method failed WhenAudioToneDetailNull test.");
            }
            catch (System.Exception)
            {
                Assert.Fail("WhenAudioToneDetailNull test failed");
            }
        }
    }
}
