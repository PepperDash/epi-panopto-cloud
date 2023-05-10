// Copyright (C) 2018 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
// Use of this source code is subject to the terms of the Crestron Software License Agreement 
// under which you licensed this source code.

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Crestron.RAD.Common;
using Crestron.RAD.Common.Enums;
using Newtonsoft.Json.Converters;

namespace RADCommonUnitTests
{
    /// <summary>
    /// Summary description for ConnectionsNodeDeserialization
    /// </summary>
    [TestClass]
    public class DatFileDeserialization
    {
        public DatFileDeserialization()
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

        [TestMethod]
        public void DeserializeWithBadVideoEnums()
        {
            var testString = "{\"connector\":\"FOO\",\"description\":\"Uncontrolled video output.\",\"type\":\"3409uu34ij\"}";


            var authenticationNodeConverter = new DatACommunicationJsonConverter();
            var stringToEnum = new StringEnumConverter();
            var videoInputConverter = new VideoInputDetailConverter();
            var videoOutputConverter = new VideoOutputDetailConverter();
            var audioInputConverter = new AudioInputDetailConverter();
            var audioOutputConverter = new AudioOutputDetailConverter();
            var serializerSettings = new JsonSerializerSettings { Converters = { authenticationNodeConverter, stringToEnum, videoInputConverter, videoOutputConverter, audioInputConverter, audioOutputConverter } };
            var videoIO = JsonConvert.DeserializeObject<VideoInputDetail>(testString, serializerSettings);
            Assert.AreEqual(VideoConnections.Unknown, videoIO.type);
            Assert.AreEqual(VideoConnectionTypes.Unknown, videoIO.connector);
        }

        [TestMethod]
        public void DeserializeWithBadAudioEnums()
        {
            var testString = "{\"connector\":\"FOO\",\"description\":\"Uncontrolled video output.\",\"type\":\"3409uu34ij\"}";
            
            var authenticationNodeConverter = new DatACommunicationJsonConverter();
            var stringToEnum = new StringEnumConverter();
            var videoInputConverter = new VideoInputDetailConverter();
            var videoOutputConverter = new VideoOutputDetailConverter();
            var audioInputConverter = new AudioInputDetailConverter();
            var audioOutputConverter = new AudioOutputDetailConverter();
            var serializerSettings = new JsonSerializerSettings { Converters = { authenticationNodeConverter, stringToEnum, videoInputConverter, videoOutputConverter, audioInputConverter, audioOutputConverter } };
            var audioIO = JsonConvert.DeserializeObject<AudioInputDetail>(testString, serializerSettings);
            Assert.AreEqual(AudioConnections.Unknown, audioIO.type);
            Assert.AreEqual(AudioConnectionTypes.Unknown, audioIO.connector);
        }

        [TestMethod]
        public void DeserializeDatFile()
        {
            var testString = "{\"manufacturer\":\"Roku\",\"description\":\"\",\"basefile\":\"VideoServer_Roku_Premiere-(4620X).ir\",\"sdkVersion\":\"2.01.002\",\"supportedSeries\":[],\"basefileModel\":\"Premiere (4620X)\",\"communication\":{\"type\":\"Ir\"},\"driverVersion\":\"2.01.002.0080\",\"inputs\":[],\"connections\":{\"outputs\":{\"audio\":[{\"connector\":\"Hdmi\",\"description\":\"Uncontrolled audio output.\",\"type\":\"Uncontrolled\"}],\"video\":[{\"connector\":\"Hdmi\",\"description\":\"Uncontrolled video output.\",\"type\":\"Uncontrolled\"}]},\"inputs\":{\"video\":[],\"audio\":[]}},\"features\":[{\"displayName\":\"Feedback is supported\",\"id\":\"SupportsFeedback\",\"value\":false},{\"displayName\":\"Forward Scan is supported\",\"id\":\"SupportsForwardScan\",\"value\":true},{\"displayName\":\"Reverse Scan is supported\",\"id\":\"SupportsReverseScan\",\"value\":true},{\"displayName\":\"Play is supported\",\"id\":\"SupportsPlay\",\"value\":true},{\"displayName\":\"Pause is supported\",\"id\":\"SupportsPause\",\"value\":false},{\"displayName\":\"Stop is supported\",\"id\":\"SupportsStop\",\"value\":false},{\"displayName\":\"Forward Skip is supported\",\"id\":\"SupportsForwardSkip\",\"value\":false},{\"displayName\":\"Reverse Skip is supported\",\"id\":\"SupportsReverseSkip\",\"value\":false},{\"displayName\":\"Repeat is supported\",\"id\":\"SupportsRepeat\",\"value\":false},{\"displayName\":\"Return is supported\",\"id\":\"SupportsReturn\",\"value\":false},{\"displayName\":\"Back is supported\",\"id\":\"SupportsBack\",\"value\":true},{\"displayName\":\"Arrow Keys is supported\",\"id\":\"SupportsArrowKeys\",\"value\":true},{\"displayName\":\"Down Arrow Key is supported\",\"id\":\"SupportsDownArrowKey\",\"value\":true},{\"displayName\":\"Left Arrow Key is supported\",\"id\":\"SupportsLeftArrowKey\",\"value\":true},{\"displayName\":\"Right Arrow Key is supported\",\"id\":\"SupportsRightArrowKey\",\"value\":true},{\"displayName\":\"Up Arrow Key is supported\",\"id\":\"SupportsUpArrowKey\",\"value\":true},{\"displayName\":\"Enter is supported\",\"id\":\"SupportsEnter\",\"value\":false},{\"displayName\":\"Select is supported\",\"id\":\"SupportsSelect\",\"value\":true},{\"displayName\":\"Clear is supported\",\"id\":\"SupportsClear\",\"value\":false},{\"displayName\":\"Exit is supported\",\"id\":\"SupportsExit\",\"value\":false},{\"displayName\":\"Home is supported\",\"id\":\"SupportsHome\",\"value\":false},{\"displayName\":\"Menu is supported\",\"id\":\"SupportsMenu\",\"value\":false},{\"displayName\":\"Keypad Number is supported\",\"id\":\"SupportsKeypadNumber\",\"value\":false},{\"displayName\":\"Pound is supported\",\"id\":\"SupportsPound\",\"value\":false},{\"displayName\":\"Asterisk is supported\",\"id\":\"SupportsAsterisk\",\"value\":false},{\"displayName\":\"Period is supported\",\"id\":\"SupportsPeriod\",\"value\":false},{\"displayName\":\"Dash is supported\",\"id\":\"SupportsDash\",\"value\":false},{\"displayName\":\"Backspace is supported\",\"id\":\"SupportsBackspace\",\"value\":false},{\"displayName\":\"Letter Keys is supported\",\"id\":\"SupportsLetterKeys\",\"value\":false},{\"displayName\":\"Letter A is supported\",\"id\":\"SupportsLetterA\",\"value\":false},{\"displayName\":\"Letter B is supported\",\"id\":\"SupportsLetterB\",\"value\":false},{\"displayName\":\"Letter C is supported\",\"id\":\"SupportsLetterC\",\"value\":false}],\"driverVersionDate\":\"2018/03/09 08:31:39.000\",\"baseModel\":\"Premiere (4620X)\",\"deviceType\":\"Video Server\",\"supportedModels\":[\"Premiere (4620X)\",\"LT (2400X)\",\"LT (2450X)\",\"LT (2700X)\",\"1 (2710X)\",\"SE (2710X)\",\"2 (2720X)\",\"2 HD (3000X)\",\"2 XD (3050X)\",\"2 XS (3100X)\",\"Express (3700X)\",\"Express Plus (3710X)\",\"Express (3900X)\",\"Express+ (3910X)\",\"3 (4200X)\",\"2 (4210X)\",\"3 (4230X)\",\"4 (4400X)\",\"Premiere Plus (4630X)\",\"Ultra (4640X)\",\"Ultra (4660X)\"]}";
            var authenticationNodeConverter = new DatACommunicationJsonConverter();
            var stringToEnum = new StringEnumConverter();
            var videoInputConverter = new VideoInputDetailConverter();
            var videoOutputConverter = new VideoOutputDetailConverter();
            var audioInputConverter = new AudioInputDetailConverter();
            var audioOutputConverter = new AudioOutputDetailConverter();
            var serializerSettings = new JsonSerializerSettings { Converters = { authenticationNodeConverter, stringToEnum, videoInputConverter, videoOutputConverter, audioInputConverter, audioOutputConverter } };
            var rootObject = JsonConvert.DeserializeObject<DatFileRootObject>(testString, serializerSettings);
            Assert.AreEqual(VideoConnections.Uncontrolled, rootObject.connections.outputs.video[0].type);
            Assert.AreEqual(VideoConnectionTypes.Hdmi, rootObject.connections.outputs.video[0].connector);
        }

        [TestMethod]
        public void DeserializeVideoIODetailWithUncontrolledType()
        {
            var testString = "{\"connector\":\"Hdmi\",\"description\":\"Uncontrolled video output.\",\"type\":\"Uncontrolled\"}";
            var authenticationNodeConverter = new DatACommunicationJsonConverter();
            var stringToEnum = new StringEnumConverter();
            var serializerSettings = new JsonSerializerSettings { Converters = { authenticationNodeConverter, stringToEnum } };
            var videoIO = JsonConvert.DeserializeObject<VideoInputDetail>(testString, serializerSettings);
            Assert.AreEqual(VideoConnections.Uncontrolled, videoIO.type);
            Assert.AreEqual(VideoConnectionTypes.Hdmi, videoIO.connector);
        }

        [TestMethod]
        public void DeserializeVideoIODetailWithValidType()
        {
            var testString = "{\"connector\":\"Hdmi\",\"description\":\"Uncontrolled video output.\",\"type\":\"Hdmi3\"}";
            var stringToEnum = new StringEnumConverter();
            var serializerSettings = new JsonSerializerSettings { Converters = { stringToEnum } };
            var videoIO = JsonConvert.DeserializeObject<VideoInputDetail>(testString, serializerSettings);
            Assert.AreEqual(VideoConnections.Hdmi3, videoIO.type);
            Assert.AreEqual(VideoConnectionTypes.Hdmi, videoIO.connector);
        }
    }
}
