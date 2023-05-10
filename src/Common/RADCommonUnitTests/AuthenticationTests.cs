using Microsoft.VisualStudio.TestTools.UnitTesting;
using Crestron.RAD.Common;
using Crestron.RAD.Common.Interfaces;
using Crestron.SimplSharp.CrestronDataStore;
using Moq;

namespace RADCommonUnitTests
{
    [TestClass]
    public class AuthenticationTests
    {
        public AuthenticationTests()
        {
            
        }

        private Mock<IDataStore> _dataStoreMock;
        private UsernamePasswordAuthentication _node;
        private TestContext testContextInstance;

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

        [TestInitialize]
        public void TestInitialize()
        {
            _dataStoreMock = new Mock<IDataStore>(); 
            _dataStoreMock.Setup(dsm => dsm.SetLocalValue(It.IsAny<string>(), It.IsAny<object>())).Returns(CrestronDataStore.CDS_ERROR.CDS_SUCCESS);
            _node = new UsernamePasswordAuthentication();
        }

        [TestMethod]
        public void SupportsUsernameShouldReturnTrue()
        {
            _node.UsernameRequired = true;
            var sut = new Authentication(_node, _dataStoreMock.Object);
            Assert.IsTrue(sut.SupportsUsername);
        }

        [TestMethod]
        public void SupportsPasswordShouldReturnTrue()
        {
            _node.PasswordRequired = true;
            var sut = new Authentication(_node, _dataStoreMock.Object);
            Assert.IsTrue(sut.SupportsPassword);
        }

        [TestMethod]
        public void UsernameMaskShouldMatchNodeValue()
        {
            var expectedValue = "Mask";
            _node.UsernameMask = expectedValue;
            var sut = new Authentication(_node, _dataStoreMock.Object);
            Assert.AreEqual(expectedValue, sut.UsernameMask);
        }

        [TestMethod]
        public void PasswordMaskShouldMatchNodeValue()
        {
            var expectedValue = "Mask";
            _node.PasswordMask = expectedValue;
            var sut = new Authentication(_node, _dataStoreMock.Object);
            Assert.AreEqual(expectedValue, sut.PasswordMask);
        }

        [TestMethod]
        public void StoreUsernameShouldAllowEmptyUsername()
        {
            var username = string.Empty;
            var usernameKey = "obscureKey";
            _node.UsernameRequired = true;
            var sut = new Authentication(_node, _dataStoreMock.Object);
            sut.UsernameKey = usernameKey;
            Assert.IsTrue(sut.StoreUsername(username));
        }

        [TestMethod]
        public void StorePasswordShouldAllowEmptyPassword()
        {
            var password = string.Empty;
            var passwordKey = "obscureKey";
            _node.PasswordRequired = true;
            var sut = new Authentication(_node, _dataStoreMock.Object);
            sut.PasswordKey = passwordKey;
            Assert.IsTrue(sut.StorePassword(password));
        }

        [TestMethod]
        public void StoreUsernameShouldStoreToIDataStore()
        {
            var username = "username";
            var usernameKey = "obscureKey";
            _dataStoreMock.Setup(dsm => dsm.SetLocalValue(usernameKey, It.IsAny<object>())).Returns(CrestronDataStore.CDS_ERROR.CDS_SUCCESS);
            _node.UsernameRequired = true;
            var sut = new Authentication(_node, _dataStoreMock.Object);
            sut.UsernameKey = usernameKey;
            Assert.IsTrue(sut.StoreUsername(username));
        }

        [TestMethod]
        public void StorePasswordShouldStoreToIDataStore()
        {
            var password = "password";
            var passwordKey = "obscureKey";
            _dataStoreMock.Setup(dsm => dsm.SetLocalValue(passwordKey, It.IsAny<object>())).Returns(CrestronDataStore.CDS_ERROR.CDS_SUCCESS);
            _node.PasswordRequired = true;
            var sut = new Authentication(_node, _dataStoreMock.Object);
            sut.PasswordKey = passwordKey;
            Assert.IsTrue(sut.StorePassword(password));
        }

        [TestMethod]
        public void StoreUsernameShouldNotStoreBecauseError()
        {
            var username = "username";
            _dataStoreMock.Setup(dsm => dsm.SetLocalValue(It.IsAny<string>(), It.IsAny<object>())).Returns(CrestronDataStore.CDS_ERROR.CDS_ACCESS_DENIED);
            _node.UsernameRequired = true;
            var sut = new Authentication(_node, _dataStoreMock.Object);
            Assert.IsFalse(sut.StoreUsername(username));
        }

        [TestMethod]
        public void StorePasswordShouldNotStoreBecauseError()
        {
            var password = "password";
            _dataStoreMock.Setup(dsm => dsm.SetLocalValue(It.IsAny<string>(), It.IsAny<object>())).Returns(CrestronDataStore.CDS_ERROR.CDS_ACCESS_DENIED);
            _node.PasswordRequired = true;
            var sut = new Authentication(_node, _dataStoreMock.Object);
            sut.UsernameKey = "obscureKey";
            Assert.IsFalse(sut.StorePassword(password));
        }

        [TestMethod]
        public void StoreUsernameShouldNotStoreBecauseEmptyKey()
        {
            var username = "username";
            _dataStoreMock.Setup(dsm => dsm.SetLocalValue(It.IsAny<string>(), It.IsAny<object>())).Returns(CrestronDataStore.CDS_ERROR.CDS_ACCESS_DENIED);
            _node.UsernameRequired = true;
            var sut = new Authentication(_node, _dataStoreMock.Object);
            Assert.IsFalse(sut.StoreUsername(username));
        }

        [TestMethod]
        public void StorePasswordShouldNotStoreBecauseEmptyKey()
        {
            var password = "password";
            _dataStoreMock.Setup(dsm => dsm.SetLocalValue(It.IsAny<string>(), It.IsAny<object>())).Returns(CrestronDataStore.CDS_ERROR.CDS_ACCESS_DENIED);
            _node.PasswordRequired = true;
            var sut = new Authentication(_node, _dataStoreMock.Object);
            Assert.IsFalse(sut.StorePassword(password));
        }
    }
}
