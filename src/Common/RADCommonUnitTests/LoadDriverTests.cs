using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq; 
using Crestron.SimplSharp.Reflection;
using Crestron.RAD.Common.BasicDriver;
using Crestron.RAD.Common;
using Crestron.RAD.Common.ExtensionMethods;
using System.IO;
          

namespace RADCommonUnitTests
{
    /// <summary>
    /// Unit Tests for the LoadDriver Method in the SimplDriver class  
    /// RADAVReceiver.dll, RADVideoServer.dll, and driver dlls are added as dependencies on TeamCity which means the latest dlls will always be tested
    /// dlls are stored in \\tx-radserver\c$\BuildAgent2\work\[latest built folder] in the same folder as the Common.sln file 
    /// Testing the drivers below require RADAVReceiver.dll and RADVideoServer.dll which are also stored in the same folder as Common.sln
    /// Tests will fail locally unless the dlls are added to local trunk\RapidAgileDriver\Common folder 
    /// </summary>
    /// 
    [TestClass]
    public class LoadDriverTests
    {
        private SimplDriver<object>.DriverInfo<object> _driverInfo; // driverinfo class
        private SimplLoadDriver TestMethod;
        
        public LoadDriverTests()
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

        [TestInitialize]
        public void TestInitialize()
        {
            _driverInfo = new SimplDriver<object>.DriverInfo<object>();
            TestMethod = new SimplLoadDriver();
           
        }

        [TestMethod]
        public void DriverInfoTransport_WhenTcpDriverIsLoaded_ReturnsTCP()
        {
            string TcpFile = "\\AvReceiver_Anthem_MRX-720_IP.dll";
            string slnPath = new System.Diagnostics.StackFrame(true).GetFileName(); //LoadDriverTests directory
            slnPath = slnPath.Substring(0, slnPath.LastIndexOf("\\"));              //RadCommonUnitTests Folder 
            slnPath = slnPath.Substring(0, slnPath.LastIndexOf("\\"));              //Common Folder
            string tcpFilePath = string.Format("{0}{1}", slnPath, TcpFile);

            _driverInfo.TransportType = SimplDriver<object>.TransportType.ITcp;
            var LoadedDriver = TestMethod.LoadDriver<object>(tcpFilePath);
            Assert.AreEqual(_driverInfo.TransportType, LoadedDriver.TransportType);
        }

        [TestMethod]
        public void DriverInfoTransport_WhenCecDriverIsLoaded_ReturnsCEC()
        {
            string CecFile = "\\VideoServer_Apple_Apple-TV-4K_CEC.dll";
            string slnPath = new System.Diagnostics.StackFrame(true).GetFileName(); //LoadDriverTests directory
            slnPath = slnPath.Substring(0, slnPath.LastIndexOf("\\"));              //RadCommonUnitTests Folder 
            slnPath = slnPath.Substring(0, slnPath.LastIndexOf("\\"));              //Common Folder
            string CecFilePath = string.Format("{0}{1}", slnPath,CecFile);

            _driverInfo.TransportType = SimplDriver<object>.TransportType.ICecDevice;
            var LoadedDriver = TestMethod.LoadDriver<object>(CecFilePath);
            Assert.AreEqual(_driverInfo.TransportType, LoadedDriver.TransportType);
        }

        [TestMethod]
        public void DriverInfoTransportType_WhenSimplDriverIsLoaded_ReturnsSimpl()
        {
            string SimplFile = @"\AvReceiver_Anthem_MRX-720_Serial.dll";
            string slnPath = new System.Diagnostics.StackFrame(true).GetFileName(); //LoadDriverTests directory
            slnPath = slnPath.Substring(0, slnPath.LastIndexOf("\\"));              //RadCommonUnitTests Folder 
            slnPath = slnPath.Substring(0, slnPath.LastIndexOf("\\"));              //Common Folder
            string SimplFilePath = string.Format("{0}{1}", slnPath, SimplFile);

            _driverInfo.TransportType = SimplDriver<object>.TransportType.ISimpl;
            var LoadedDriver = TestMethod.LoadDriver<object>(SimplFilePath);
            Assert.AreEqual(_driverInfo.TransportType, LoadedDriver.TransportType);
        }

        [TestMethod]
        public void DriverInfoTransportType_WhenNoDriverIsFound_ReturnsNone()
        {
            string nonExistingFile = "gibberish12345";
            _driverInfo.TransportType = SimplDriver<object>.TransportType.None;

            var LoadedDriver = TestMethod.LoadDriver<object>(nonExistingFile);
            Assert.AreEqual(_driverInfo.TransportType, LoadedDriver.TransportType);
        }


    }

    public class SimplLoadDriver 
    {
        //rewriting LoadDriver method to use System.Reflection.Assembly instead of Crestron.SimplSharp.Reflection.Assembly 
        //and Sytem.Type instead of CType since Crestron.SimplSharp.Reflection requires Windows CE environment assemblies
        //i.e. it needs to be running on a crestron processor

        public SimplDriver<object>.DriverInfo<object> LoadDriver<DriverType>(string fileName)  //method to test
        {
            var driverInfo = new SimplDriver<object>.DriverInfo<object>();
            try
            {
                var dll = System.Reflection.Assembly.LoadFrom(fileName);
                Type[] types = dll.GetTypes();

                for (int onType = 0; onType < types.Length; onType++)
                {
                    Type cType = types[onType];
                    var interfaces = cType.GetInterfaces();
                    var simplDevice = interfaces.FirstOrDefault(x => x.Name.Equals(SimplDriver<object>.TransportType.ISimpl.ToString()));
                    var tcpDevice = interfaces.FirstOrDefault(x => x.Name.Equals(SimplDriver<object>.TransportType.ITcp.ToString()));
                    var cecDevice = interfaces.FirstOrDefault(x => x.Name.Equals(SimplDriver<object>.TransportType.ICecDevice.ToString()));

                    if (simplDevice.Exists())
                    {
                        driverInfo.Driver = cType.FullName;
                        driverInfo.TransportType = SimplDriver<object>.TransportType.ISimpl;
                        break;
                    }
                    else if (tcpDevice.Exists())
                    {
                        driverInfo.Driver = cType.FullName;
                        driverInfo.TransportType = SimplDriver<object>.TransportType.ITcp;
                        break;
                    }
                    else if (cecDevice.Exists())
                    {
                        driverInfo.Driver = cType.FullName;
                        driverInfo.TransportType = SimplDriver<object>.TransportType.ICecDevice;
                        break;
                    }
                }
            }
            catch (Exception)
            { 
            }

            return driverInfo;

        }
    }
}
