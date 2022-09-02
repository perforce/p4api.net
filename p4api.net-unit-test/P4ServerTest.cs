
using Perforce.P4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using NLog;
using System.Threading;
using System.Threading.Tasks;

namespace p4api.net.unit.test
{

    /// <summary>
    ///This is a test class for P4ServerTest and is intended
    ///to contain all P4ServerTest Unit Tests
    ///</summary>
    [TestClass()]
#if NET462
    [DeploymentItem("x64", "x64")]
    [DeploymentItem("x86", "x86")]
#endif
    public class P4ServerTest
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static UnitTestConfiguration configuration;
        private string TestDir = "";

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            configuration = UnitTestSettings.GetApplicationConfiguration();
            TestDir = configuration.TestDirectory;

            Utilities.LogTestStart(TestContext);
        }

        [TestCleanup]
        public void CleanupTest()
        {
            Utilities.LogTestFinish(TestContext);
        }

        #region Additional test attributes

        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //

        #endregion

        /// <summary>
        ///A test for P4Server Constructor. Connect to a server check if it supports Unicode and disconnect
        ///</summary>
        [TestMethod()]
        public void P4ServerConstructorTest()
        {
            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    using (P4Server target = new P4Server(server, user, pass, ws_client))
                    {
                        if (cptype == Utilities.CheckpointType.U)
                            Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                        else
                            Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                        uint cmdId = 7;
                        Assert.IsTrue(target.RunCommand("dirs", cmdId, false, new string[] { "//depot/*" }, 1),
                            "\"dirs\" command failed");
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                    p4d?.Dispose();
                }
            }
        }


        /// <summary>
        /// A test for P4Server.UrlHandled() with ascii server. Connect to a server,
        /// check for existence of UrlHandled bool.
        ///</summary>
        [TestMethod()]
        public void P4ServerUrlHandledTestA()
        {
            P4ServerUrlHandledTest(Utilities.CheckpointType.A);
        }

        /// <summary>
        /// A test for P4Server.UrlHandled() with unicode server. Connect to a server,
        /// check for existence of UrlHandled bool.
        ///</summary>
        [TestMethod()]
        public void P4ServerUrlHandledTestU()
        {
            P4ServerUrlHandledTest(Utilities.CheckpointType.U);
        }

        /// <summary>
        /// A test for P4Server.UrlHandled(). Connect to a server, check for existence of
        /// UrlHandled bool.
        ///</summary>
        public void P4ServerUrlHandledTest(Utilities.CheckpointType cptype)
        {
            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            try
            {
                p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                Assert.IsNotNull(p4d, "Setup Failure");

                using (P4Server target = new P4Server(server, user, pass, ws_client))
                {
                    bool UrlHandled = target.UrlHandled();
                    // should be false, but should exist
                    Assert.IsFalse(UrlHandled);
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
                p4d?.Dispose();
            }
        }

        /// <summary>
        ///A test for RunCommand
        ///</summary>
        [TestMethod()]
        public void RunCommandTest()
        {
            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    using (P4Server target = new P4Server(server, user, pass, ws_client))
                    {
                        if (cptype == Utilities.CheckpointType.U)
                            Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                        else
                            Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                        uint cmdId = 7;
                        Assert.IsTrue(target.RunCommand("dirs", cmdId, false, new string[] { "//depot/*" }, 1),
                            "\"dirs\" command failed");
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                    p4d?.Dispose();
                }
            }
        }

        /// <summary>
        ///A test for Idle disconnect
        ///</summary>
        [TestMethod()]
        public void IdleDisconnectTest()
        {
            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    using (P4Server target = new P4Server(server, user, pass, ws_client))
                    {
                        if (cptype == Utilities.CheckpointType.U)
                            Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                        else
                            Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                        uint cmdId = 7;
                        Assert.IsTrue(target.RunCommand("dirs", cmdId, false, new string[] { "//depot/*" }, 1),
                            "\"dirs\" command failed");

                        System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(target.IdleDisconnectWaitTime + 100));
                        Assert.IsFalse(target.IsConnected());

                        cmdId = 7;
                        Assert.IsTrue(target.RunCommand("dirs", cmdId, false, new string[] { "//depot/*" }, 1),
                            "\"dirs\" command failed");
                        Assert.IsTrue(target.RunCommand("dirs", cmdId + 1, false, new string[] { "//depot/*" }, 1),
                            "\"dirs\" command failed");

                        System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(target.IdleDisconnectWaitTime + 100));
                        Assert.IsFalse(target.IsConnected());
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                    p4d?.Dispose();
                }
            }
        }

#if DEBUG_TIMEOUT
        /// <summary>
        ///A test for RunCommand timeout
        ///</summary>
        [TestMethod()]
        public void RunCommandTimeOutTest()
        {
            var cptype = Utilities.CheckpointType.A;
            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            string cmd = "verify";  // Was "TimeOutTest"
            var args = new string[] { "-v","//..." };

            Process p4d = Utilities.DeployP4TestServer(TestDir, cptype);
            Assert.IsNotNull(p4d, "Setup Failure");

            uint cmdId = 7;

            try
            {
                using (P4Server target = new P4Server(server, user, pass, ws_client))
                {
                    try
                    {
                        Assert.IsTrue(target.RunCommand(cmd, cmdId, false, args, 2));
                    }
                    catch (P4CommandTimeOutException)
                    {
                        Assert.Fail("Should not have timed out");
                    }
                    catch (Exception)
                    {
                        Assert.Fail("Wrong exception thrown for timeout");
                    }

                    try
                    {
                        Assert.IsFalse(target.RunCommand(cmd, cmdId, false, args, 2));
                        Assert.Fail("Didn't timeout");
                    }
                    catch (P4CommandTimeOutException)
                    {
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        Assert.Fail("Wrong exception thrown for timeout");
                    }
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
            }
        }

        /// <summary>
        ///A test for RunCommand timeout
        ///</summary>
        [TestMethod()]
        public void RunCommandLongTimeOutTest()
        {
            var cptype = Utilities.CheckpointType.A;
            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = Utilities.DeployP4TestServer(TestDir, cptype);
            Assert.IsNotNull(p4d, "Setup Failure");
            uint cmdId = 7;

            try
            {
                using (P4Server target = new P4Server(server, user, pass, ws_client))
                {
                    try
                    {
                        Assert.IsTrue(target.RunCommand("LongTimeOutTest", cmdId, false, new String[] { "10" }, 1));
                    }
                    catch (P4CommandTimeOutException)
                    {
                        Assert.Fail("Should not have timed out");
                    }
                    catch (Exception)
                    {
                        Assert.Fail("Wrong exception thrown for timeout");
                    }
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
            }
        }
#endif

        /// <summary>
        ///A test for GetBinaryResults
        ///</summary>
        [TestMethod()]
        public void GetBinaryResultsTest()
        {
            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    using (P4Server target = new P4Server(server, user, pass, ws_client))
                    {
                        if (cptype == Utilities.CheckpointType.U)
                            Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                        else
                            Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                        string[] parms = new string[] { "//depot/MyCode/Silly.bmp" };

                        uint cmdId = 7;
                        Assert.IsTrue(target.RunCommand("print", cmdId, false, parms, 1),
                            "\"print\" command failed");

                        byte[] results = target.GetBinaryResults(cmdId);

                        Assert.IsNotNull(results, "GetBinaryResults returned null data");

                        Assert.AreEqual(results.Length, 3126);

                        Assert.AreEqual(results[0], 0x42);
                        Assert.AreEqual(results[1], 0x4d);
                        Assert.AreEqual(results[2], 0x36);

                        Assert.AreEqual(results[0x10], 0x00);
                        Assert.AreEqual(results[0xA0], 0xC0);
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                    p4d?.Dispose();
                }
            }
        }

        /// <summary>
        ///A test for GetErrorResults
        ///</summary>
        [TestMethod()]
        public void GetErrorResultsTest()
        {
            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    using (P4Server target = new P4Server(server, user, pass, ws_client))
                    {
                        if (cptype == Utilities.CheckpointType.U)
                            Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                        else
                            Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                        string[] parms = new string[] { "//depot/MyCode/NoSuchFile.bmp" };

                        uint cmdId = 7;
                        target.RunCommand("print", cmdId, false, parms, 1);

                        P4ClientErrorList results = target.GetErrorResults(cmdId);

                        Assert.IsNotNull(results, "GetErrorResults returned null data");

                        Assert.AreEqual(results.Count, 1);

                        P4ClientError firstError = (P4ClientError)results[0];
                        Assert.AreEqual(firstError.ErrorMessage.TrimEnd(new char[] { '\r', '\n' }),
                            "//depot/MyCode/NoSuchFile.bmp - no such file(s).");
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                    p4d?.Dispose();
                }
            }
        }

        /// <summary>
        ///A test for GetInfoResults
        ///</summary>
        [TestMethod()]
        public void GetInfoResultsTest()
        {
            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    using (P4Server target = new P4Server(server, user, pass, ws_client))
                    {
                        if (cptype == Utilities.CheckpointType.U)
                            Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                        else
                            Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                        string[] parms = new string[] { "//depot/mycode/*" };

                        uint cmdId = 7;
                        Assert.IsTrue(target.RunCommand("files", cmdId, false, parms, 1),
                            "\"files\" command failed");

                        P4ClientInfoMessageList results = target.GetInfoResults(cmdId);

                        Assert.IsNotNull(results, "GetInfoResults returned null data");

                        if (cptype == Utilities.CheckpointType.U)
                            Assert.AreEqual(3, results.Count);
                        else
                            Assert.AreEqual(3, results.Count);

                        P4ClientInfoMessage firstResult = results[0];
                        Assert.AreEqual(firstResult.MessageCode, 352327915);
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                    p4d?.Dispose();
                }
            }
        }

        /// <summary>
        ///A test for GetTaggedOutput
        ///</summary>
        [TestMethod()]
        public void GetTaggedOutputTest()
        {
            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    using (P4Server target = new P4Server(server, user, pass, ws_client))
                    {
                        if (cptype == Utilities.CheckpointType.U)
                            Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                        else
                            Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                        string[] parms = new string[] { "//depot/mycode/*" };

                        uint cmdId = 7;
                        Assert.IsTrue(target.RunCommand("files", cmdId, true, parms, 1),
                            "\"files\" command failed");

                        TaggedObjectList results = target.GetTaggedOutput(cmdId);

                        Assert.IsNotNull(results, "GetTaggedOutput returned null data");

                        if (cptype == Utilities.CheckpointType.U)
                            Assert.AreEqual(3, results.Count);
                        else
                            Assert.AreEqual(3, results.Count);
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                    p4d?.Dispose();
                }
            }
        }

        /// <summary>
        ///A test for GetTextResults
        ///</summary>
        [TestMethod()]
        public void GetTextResultsTest()
        {
            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    using (P4Server target = new P4Server(server, user, pass, ws_client))
                    {
                        if (cptype == Utilities.CheckpointType.U)
                            Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                        else
                            Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                        string[] parms = new string[] { "//depot/MyCode/ReadMe.txt" };

                        uint cmdId = 7;
                        Assert.IsTrue(target.RunCommand("print", cmdId, false, parms, 1),
                            "\"print\" command failed");

                        string results = target.GetTextResults(cmdId);

                        Assert.IsNotNull(results, "GetErrorResults GetTextResults null data");
#if _WINDOWS
                        int expectedLen = 30;
#else
                        int expectedLen = 32;
#endif
                        if (cptype == Utilities.CheckpointType.U)
                            Assert.AreEqual(expectedLen, results.Length);
                        else
                            Assert.AreEqual(expectedLen, results.Length);

                        Assert.IsTrue(results.StartsWith("Don't Read This!"));
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                    p4d?.Dispose();
                }
            }
        }

        private byte[] BinaryCallbackResults;

        private void BinaryResultsCallback(uint cmdId, byte[] data)
        {
            if (data != null && data.Length > 0)
                BinaryCallbackResults = data;
        }

        /// <summary>
        ///A test for SetBinaryResultsCallback
        ///</summary>
        [TestMethod()]
        public void SetBinaryResultsCallbackTest()
        {
            P4Server.BinaryResultsDelegate cb =
                new P4Server.BinaryResultsDelegate(BinaryResultsCallback);

            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    using (P4Server target = new P4Server(server, user, pass, ws_client))
                    {
                        if (cptype == Utilities.CheckpointType.U)
                            Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                        else
                            Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                        target.BinaryResultsReceived += cb;

                        string[] parms = new string[] { "//depot/MyCode/Silly.bmp" };

                        uint cmdId = 7;
                        Assert.IsTrue(target.RunCommand("print", cmdId, false, parms, 1),
                            "\"print\" command failed");

                        byte[] results = target.GetBinaryResults(cmdId);

                        Assert.IsNotNull(BinaryCallbackResults, "BinaryCallbackResults is null");
                        Assert.IsNotNull(results, "InfoCallbackResults is null");

                        Assert.AreEqual(results.Length, BinaryCallbackResults.Length);
                        Assert.AreEqual(BinaryCallbackResults.Length, 3126);

                        for (int idx = 0; idx < BinaryCallbackResults.Length; idx++)
                        {
                            Assert.AreEqual(results[idx], BinaryCallbackResults[idx]);
                        }
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                    p4d?.Dispose();
                }
            }
        }

        private string ErrorCallbackResultsMessage;
        private uint ErrorCallbackResultsMessageId;
        private int ErrorCallbackResultsErrorNumber;
        private int ErrorCallbackResultsSeverity;

        private void ErrorResultsCallback(uint cmdId, int severity, int errorNumber, string message)
        {
            ErrorCallbackResultsMessageId = cmdId;
            ErrorCallbackResultsMessage = message;
            ErrorCallbackResultsSeverity = severity;
            ErrorCallbackResultsErrorNumber = errorNumber;
        }

        /// <summary>
        ///A test for SetErrorCallback
        ///</summary>
        [TestMethod()]
        public void SetErrorCallbackTest()
        {
            P4Server.ErrorDelegate cb =
                new P4Server.ErrorDelegate(ErrorResultsCallback);

            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    using (P4Server target = new P4Server(server, user, pass, ws_client))
                    {
                        if (cptype == Utilities.CheckpointType.U)
                            Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                        else
                            Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                        target.ErrorReceived += cb;

                        string[] parms = new string[] { "//depot/MyCode/NoSuchFile.bmp" };

                        uint cmdId = 7;
                        Assert.IsTrue(target.RunCommand("fstat", cmdId, false, parms, 1),
                            "\"fstat\" command failed");

                        P4ClientErrorList results = target.GetErrorResults(cmdId);

                        Assert.IsFalse(string.IsNullOrEmpty(ErrorCallbackResultsMessage),
                            "ErrorCallbackResultsMessage is null or empty");
                        Assert.IsNotNull(results, "GetErrorResults returned null");

                        Assert.AreEqual(results.Count, 1);

                        P4ClientError firstError = (P4ClientError)results[0];
                        Assert.AreEqual(firstError.ErrorMessage.TrimEnd(new char[] { '\r', '\n' }),
                            ErrorCallbackResultsMessage.TrimEnd(new char[] { '\r', '\n' }));
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                    p4d?.Dispose();
                }
            }
        }

        private class InfoCbData
        {
            public InfoCbData(string m, int l, int id)
            {
                Message = m;
                Level = l;
                MsgId = id;
            }

            public string Message;
            public int Level;
            public int MsgId;

            public override string ToString()
            {
                return string.Format("{0}:{1}", Level, Message);
            }
        }

        private List<InfoCbData> InfoCallbackResults;

        private void InfoResultsCallback(uint cmdId, int msgId, int level, string message)
        {
            InfoCallbackResults.Add(new InfoCbData(message, level, msgId));
        }

        private void BadInfoResultsCallback(uint cmdId, int msgId, int level, string message)
        {
            throw new Exception("I'm a bad delegate");
        }

        /// <summary>
        ///A test for SetInfoResultsCallback
        ///</summary>
        [TestMethod()]
        public void SetInfoResultsCallbackTest()
        {
            P4Server.InfoResultsDelegate cb =
                new P4Server.InfoResultsDelegate(InfoResultsCallback);

            P4Server.InfoResultsDelegate bcb =
                new P4Server.InfoResultsDelegate(BadInfoResultsCallback);

            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                Utilities.CheckpointType cptype = (Utilities.CheckpointType)i;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    using (P4Server target = new P4Server(server, user, pass, ws_client))
                    {
                        if (cptype == Utilities.CheckpointType.U)
                            Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                        else
                            Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                        target.InfoResultsReceived += cb;

                        // add in the bad handler that throws an exception, to 
                        // make sure the event broadcaster can handle it.
                        target.InfoResultsReceived += bcb;

                        string[] parms = new string[] { "//depot/mycode/*" };

                        InfoCallbackResults = new List<InfoCbData>();

                        uint cmdId = 7;
                        Assert.IsTrue(target.RunCommand("files", cmdId, false, parms, 1),
                            "\"files\" command failed");

                        P4ClientInfoMessageList results = target.GetInfoResults(cmdId);

                        Assert.IsNotNull(InfoCallbackResults, "InfoCallbackResults is null");
                        Assert.IsNotNull(results, "GetInfoResults returned null");

                        Assert.AreEqual(results.Count, InfoCallbackResults.Count);

                        for (int idx = 0; idx < InfoCallbackResults.Count; idx++)
                        {
                            Assert.AreEqual(results[idx].MessageCode, InfoCallbackResults[idx].MsgId);
                        }
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                    p4d?.Dispose();
                }
            }
        }

        private TaggedObjectList TaggedCallbackResults;

        private void TaggedOutputCallback(uint cmdId, int objId, TaggedObject obj)
        {
            TaggedCallbackResults.Add(obj);
        }

        /// <summary>
        ///A test for SetTaggedOutputCallback
        ///</summary>
        [TestMethod()]
        public void SetTaggedOutputCallbackTest()
        {
            P4Server.TaggedOutputDelegate cb =
                new P4Server.TaggedOutputDelegate(TaggedOutputCallback);

            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                Utilities.CheckpointType cptype = (Utilities.CheckpointType)i;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    using (P4Server target = new P4Server(server, user, pass, ws_client))
                    {
                        if (cptype == Utilities.CheckpointType.U)
                            Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                        else
                            Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                        target.TaggedOutputReceived += cb;

                        StringList parms = new string[] { "//depot/mycode/*" };

                        TaggedCallbackResults = new TaggedObjectList();

                        uint cmdId = 7;
                        Assert.IsTrue(target.RunCommand("fstat", cmdId, true, parms, 1),
                            "\"fstat\" command failed");

                        TaggedObjectList results = target.GetTaggedOutput(cmdId);

                        Assert.IsNotNull(TaggedCallbackResults, "TaggedCallbackResults is null");
                        Assert.IsNotNull(results, "GetTaggedOutput returned null");

                        Assert.AreEqual(results.Count, TaggedCallbackResults.Count);

                        for (int idx = 0; idx < TaggedCallbackResults.Count; idx++)
                        {
                            Assert.AreEqual((results[idx])["depotFile"],
                                (TaggedCallbackResults[idx])["depotFile"]);
                        }
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                    p4d?.Dispose();
                }
            }
        }

        private string TextCallbackResults;

        private void TextResultsCallback(uint cmdId, string info)
        {
            TextCallbackResults += info;
        }

        /// <summary>
        ///A test for SetTextResultsCallback
        ///</summary>
        [TestMethod()]
        public void SetTextResultsCallbackTest()
        {
            P4Server.TextResultsDelegate cb =
                new P4Server.TextResultsDelegate(TextResultsCallback);

            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    using (P4Server target = new P4Server(server, user, pass, ws_client))
                    {
                        if (cptype == Utilities.CheckpointType.U)
                            Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                        else
                            Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                        target.TextResultsReceived += cb;

                        string[] parms = new string[] { "//depot/mycode/ReadMe.txt" };

                        TextCallbackResults = string.Empty;

                        uint cmdId = 7;
                        Assert.IsTrue(target.RunCommand("print", cmdId, true, parms, 1),
                            "\"print\" command failed");

                        string results = target.GetTextResults(cmdId);

                        Assert.IsFalse(string.IsNullOrEmpty(TextCallbackResults), "TextCallbackResults is null");
                        Assert.IsFalse(string.IsNullOrEmpty(results), "GetTextResults is null");

                        Assert.AreEqual(results.Length, TextCallbackResults.Length);

                        Assert.AreEqual(TextCallbackResults, results);
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                    p4d?.Dispose();
                }
            }
        }

        P4Server forParallelTransferTest = null;
        bool callbackHappened = false;

        private int ParallelTransferCallback(IntPtr pServer, string cmd, string[] args, uint argCount, IntPtr dictIter, uint threads)
        {
            callbackHappened = true;
            // verify the args
            Assert.IsNotNull(pServer);
            Assert.IsTrue(cmd == "transmit");
            Assert.IsTrue(args.Length == argCount);
            StrDictListIterator iter = new StrDictListIterator(forParallelTransferTest, dictIter);
            // iterate over the members to validate that it works
            while (iter.NextItem())
            {
                Perforce.P4.KeyValuePair kv = null;
                while ((kv = iter.NextEntry()) != null)
                {
                    // walking the data is enough
                }
            }
            Assert.IsTrue(threads > 0);

            return 0;
        }

        /// <summary>
        /// A test for SetParallelSyncUnicodeCallback
        ///</summary>
        [TestMethod()]
        public void SetParallelSyncCallbackTestUnicode()
        {
            SetParallelSyncCallbackTest(Utilities.CheckpointType.U);
        }

        /// <summary>
        /// A test for SetParallelSyncUnicodeCallback
        ///</summary>
        [TestMethod()]
        public void SetParallelSyncCallbackTestAscii()
        {
            SetParallelSyncCallbackTest(Utilities.CheckpointType.A);
        }

        StringList getClientFiles(P4Server target)
        {
            Assert.IsTrue(target.RunCommand("fstat", 0, true, new string[] { "//..." }, 1));
            TaggedObjectList list = target.GetTaggedOutput(0);
            StringList ret = new StringList();
            foreach (TaggedObject obj in list)
            {
                string action, localPath;
                if (obj.TryGetValue("headAction", out action))
                {
                    if (action.Contains("delete"))
                        continue;
                }
                else
                {
                    continue;
                }
                if (obj.TryGetValue("clientFile", out localPath))
                {
                    ret.Add(localPath);
                }
            }
            return ret;
        }

        public void OpenAndModifyFiles(StringList localFiles, P4Server target)
        {
            foreach (string localPath in localFiles)
            {
                // Make writable
                System.IO.File.SetAttributes(localPath, FileAttributes.Normal);

                string name = Path.GetFileNameWithoutExtension(localPath);

                if (name == "pup" || name == "Пюп")
                {
                    // modify the file UTF-16
                    System.IO.File.WriteAllText(localPath, DateTime.Now.ToLongDateString(), Encoding.Unicode);
                }
                else
                {
                    // modify the file Text
                System.IO.File.WriteAllText(localPath, DateTime.Now.ToLongDateString());
                }

                // add to the default change?
                if (target != null)
                    Assert.IsTrue(target.RunCommand("edit", 0, true, new string[] { localPath }, 1));
            }
        }

        /*
         *  Helper class for catching P4Server output
         */
        class OutputReceiver
        {
            P4Server target = null;

            public OutputReceiver(P4Server _target)
            {
                target = _target;
                target.TaggedOutputReceived += TaggedOutputReceived;
                target.ErrorReceived += ErrorReceived;
                target.InfoResultsReceived += InfoReceived;
                target.TextResultsReceived += TextReceived;
                target.BinaryResultsReceived += BinaryReceived;
            }

            public void Detach()
            {
                // remove from the delegate list
                target.TaggedOutputReceived -= TaggedOutputReceived;
                target.ErrorReceived -= ErrorReceived;
                target.InfoResultsReceived -= InfoReceived;
                target.TextResultsReceived -= TextReceived;
                target.BinaryResultsReceived -= BinaryReceived;
            }

            public List<int> threadTagged = new List<int>();
            public List<uint> cmdIdTagged = new List<uint>();
            public List<int> objIdTagged = new List<int>();
            public List<TaggedObject> taggedObjs = new List<TaggedObject>();

            public void TaggedOutputReceived(uint cmdId, int ObjId, TaggedObject obj)
            {
                threadTagged.Add(Thread.CurrentThread.ManagedThreadId);
                cmdIdTagged.Add(cmdId);
                objIdTagged.Add(ObjId);
                taggedObjs.Add(obj);
            }

            public List<int> threadErrors = new List<int>();
            public List<uint> cmdIdErrors = new List<uint>();
            public List<int> severities = new List<int>();
            public List<int> errorNums = new List<int>();
            public List<string> errorData = new List<string>();

            public void ErrorReceived(uint cmdId, int severity, int errorNum, string data)
            {
                threadErrors.Add(Thread.CurrentThread.ManagedThreadId);
                cmdIdErrors.Add(cmdId);
                severities.Add(severity);
                errorNums.Add(errorNum);
                errorData.Add(data);
            }

            public List<int> threadInfo = new List<int>();
            public List<uint> cmdIdInfo = new List<uint>();
            public List<int> infoMsgIds = new List<int>();
            public List<int> infoLevels = new List<int>();
            public List<string> infoData = new List<string>();

            public void InfoReceived(uint cmdId, int msgId, int level, string data)
            {
                threadInfo.Add(Thread.CurrentThread.ManagedThreadId);
                cmdIdInfo.Add(cmdId);
                infoMsgIds.Add(msgId);
                infoLevels.Add(level);
                infoData.Add(data);
            }

            public List<int> threadText = new List<int>();
            public List<uint> cmdIdText = new List<uint>();
            public List<string> textData = new List<string>();

            public void TextReceived(uint cmdId, string data)
            {
                threadText.Add(Thread.CurrentThread.ManagedThreadId);
                cmdIdText.Add(cmdId);
                textData.Add(data);
            }

            public List<int> threadBinary = new List<int>();
            public List<uint> cmdIdBinary = new List<uint>();
            public List<byte[]> binaryData = new List<byte[]>();

            public void BinaryReceived(uint cmdId, byte[] data)
            {
                threadBinary.Add(Thread.CurrentThread.ManagedThreadId);
                cmdIdBinary.Add(cmdId);
                binaryData.Add(data);
            }
        }

        public void SetParallelSyncCallbackTest(Utilities.CheckpointType cptype)
        {
            P4Server.ParallelTransferDelegate cb =
                new P4Server.ParallelTransferDelegate(ParallelTransferCallback);

            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;
            Repository rep = null;

            try
            {
                //int checkpoint = unicode ? 19 : 21;
                p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                Assert.IsNotNull(p4d, "Setup Failure");

                Server svr = new Server(new ServerAddress(server));
                rep = new Repository(svr);
                Utilities.SetClientRoot(rep, TestDir, cptype, ws_client, false);

                using (P4Server target = new P4Server(server, user, pass, ws_client))
                {
                    // step 1 : set net.parallel.threads to > 0
                    Assert.IsTrue(target.RunCommand("configure", 7, true, new string[] { "set", "net.parallel.max=4" }, 2));

                    // disconnect so that we get a connection that recognizes the net.parallel.max configurable
                }

                using (P4Server target = new P4Server(server, user, pass, ws_client))
                {
                    forParallelTransferTest = target;
                    callbackHappened = false;
                    // revert anything in progress, it just makes testing harder
                    Assert.IsTrue(target.RunCommand("revert", 7, true, new string[] { "//..." }, 1));

                    // get the big file list
                    StringList localFiles = getClientFiles(target);

                    {
                        // step 2: positive tests
                        // 2a: test with our custom (nothing) handler
                        target.SetParallelTransferCallback(cb);
                        // set up event receivers to ensure that we're getting the events that we expect (error, tagged output, info?)
                        OutputReceiver outputCatcher = new OutputReceiver(target);
                        Assert.IsTrue(target.RunCommand("sync", 7, true, new string[] { "--parallel", "threads=4,batch=8,batchsize=1,min=1,minsize=1", "-f", "//..." }, 4));
                        Assert.IsTrue(callbackHappened);
                        // assert some info about outputCatcher
                        // although we didn't perform the sync, weget tagged info on what would have happened
                        Assert.AreEqual(7, outputCatcher.cmdIdTagged.Count);
                        outputCatcher.Detach();
                    }

                    {
                        // 2b: use the default handler, verify that we got synced files and no errors
                        target.SetParallelTransferCallback(null);
                        // set up event receivers to ensure that we're getting the events that we expect (error, tagged output, info?)
                        OutputReceiver outputCatcher = new OutputReceiver(target);
                        Assert.IsTrue(target.RunCommand("sync", 7, true, new string[] { "--parallel", "threads=4,batch=8,batchsize=1,min=1,minsize=1", "-f", "//..." }, 4));
                        // verify that we got the right files
                        foreach (string localPath in localFiles)
                        {
                            // verify that the file exists, throws an exception if it doesn't
                            System.IO.File.GetAttributes(localPath);
                        }
                        // assert some info about outputCatcher
                        // we should have 1 tagged data per file
                        Assert.AreEqual(7, outputCatcher.cmdIdTagged.Count);
                        outputCatcher.Detach();
                    }

                    {
                        // step 3: error case (clobber not allowed)
                        //  to set up clobber errors for a file we need
                        //      - revision > 1
                        //      - synced revision < revision
                        //      - modified local file (writeable)
                        //      - perform a normal sync without clobber set in the client
                        // make all the files writeable, modify them, and add to the default CL
                        OpenAndModifyFiles(localFiles, target);

                        // submit the new revs
                        Assert.IsTrue(target.RunCommand("submit", 0, true, new string[] { "-d", "Add a revision to each file" }, 2));
                        // revert (the unicode server has reopen set, and this is easier than client modification)
                        Assert.IsTrue(target.RunCommand("revert", 0, true, new string[] { "//..." }, 1));

                        // sync to revision #1
                        Assert.IsTrue(target.RunCommand("sync", 0, true, new string[] { "-f", "//...#1" }, 2));
                        // make all of the files writeable again, but do not add to a CL
                        OpenAndModifyFiles(localFiles, null);

                        // set up event receivers to ensure that we're getting the events that we expect (error, tagged output, info?)
                        OutputReceiver outputCatcher = new OutputReceiver(target);

                        // run the sync without -f to produce the clobber issue
                        try
                        {
                            // the RunCommand should throw an exception, so this Assert should never be evaluated.  If it is, it should fail
                            Assert.IsFalse(target.RunCommand("sync", 7, true, new string[] { "--parallel", "threads=4,batch=8,batchsize=1,min=1,minsize=1", "//..." }, 3));
                        }
                        catch (P4Exception e)
                        {
                            Assert.IsTrue(e.Message == "Error detected during parallel operation");
                        }

                        // assert some info about outputCatcher
                        // we should have 1 error data per file (from a different thread than ours) and one generic "Error detected..." error from the initiator
                        Assert.AreEqual(8, outputCatcher.cmdIdErrors.Count);
                        outputCatcher.Detach();

                        // error list should contain all of the files that were writable
                        P4ClientErrorList errors = target.GetErrorResults(7);
                        Assert.IsTrue(errors != null);
                        // this is a n^2 approach, but the files in the sync don't necessarily
                        // come back in the same order (threads, so you know)
                        for (int i = 0; i < errors.Count; i++)
                        {
                            bool foundIt = false;
                            for (int j = 0; j < localFiles.Count; j++)
                            {
                                // confirm that the filename is in our list
                                if (errors[i].ErrorMessage.Contains(localFiles[j]))
                                {
                                    foundIt = true;
                                    break;
                                }
                            }

                            System.Console.WriteLine(errors[i].ErrorMessage);
                            Assert.IsTrue(foundIt);
                        }
                    }
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
                p4d?.Dispose();
                rep?.Dispose();
            }
        }

        /// <summary>
        /// A test for ParallelSubmitCallbackTest
        ///</summary>
        [TestMethod()]
        public void ParallelSubmitCallbackTestA()
        {
            ParallelSubmitCallbackTest(Utilities.CheckpointType.A);
        }

        /// <summary>
        /// A test for ParallelSubmitCallbackTest
        ///</summary>
        [TestMethod()]
        public void ParallelSubmitCallbackTestU()
        {
            ParallelSubmitCallbackTest(Utilities.CheckpointType.U);
        }

        public void ParallelSubmitCallbackTest(Utilities.CheckpointType cptype)
        {
            P4Server.ParallelTransferDelegate cb =
                new P4Server.ParallelTransferDelegate(ParallelTransferCallback);

            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;
            Repository rep = null;

            try
            {
                int checkpoint = (cptype == Utilities.CheckpointType.U) ? 19 : 21;
                p4d = Utilities.DeployP4TestServer(TestDir, checkpoint, cptype, TestContext.TestName);
                Assert.IsNotNull(p4d, "Setup Failure");

                Server svr = new Server(new ServerAddress(server));
                rep = new Repository(svr);
                Utilities.SetClientRoot(rep, TestDir, cptype, ws_client);

                using (P4Server target = new P4Server(server, user, pass, ws_client))
                {
                    // step 1 : set net.parallel.max to > 0
                    Assert.IsTrue(target.RunCommand("configure", 7, true, new string[] { "set", "net.parallel.max=4" }, 2));
                    // disconnect so that we get a connection that recognizes the configurable
                }

                using (P4Server target = new P4Server(server, user, pass, ws_client))
                {
                    // revert anything in progress, it just makes testing harder
                    Assert.IsTrue(target.RunCommand("revert", 7, true, new string[] { "//..." }, 1));
                    // also sync everything
                    Assert.IsTrue(target.RunCommand("sync", 7, true, new string[] { "-f", "//..." }, 2));

                    // get the big file list
                    StringList localFiles = getClientFiles(target);

                    // step 2: positive tests
                    // 2a: test with our custom (nothing) handler
                    // open everything for edit and modify it for the submit
                    OpenAndModifyFiles(localFiles, target);
                    target.SetParallelTransferCallback(cb);

                    forParallelTransferTest = target;
                    callbackHappened = false;
                    try
                    {
                        Assert.IsTrue(target.RunCommand("submit", 7, true, new string[] { "--parallel", "threads=4,batch=8,min=1", "-d", "this should not create a submitted CL" }, 4));
                    }
                    catch (P4Exception)
                    {
                        Assert.IsTrue(callbackHappened);
                        // we should get lots of library errors because we're trying to submit files that didn't get transferred
                        // at this point the server is a little messed up, so remove and redeploy to keep testing
                        P4ClientErrorList errs = target.GetErrorResults(7);
                        Assert.IsTrue(errs.Count > 0); // casually looking at the results it generates about 17 errors
                    }
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
                p4d?.Dispose();
                rep?.Dispose();
            }

            p4d = null;
            rep = null;
            try
            {
                p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                Assert.IsNotNull(p4d, "Setup Failure");

                Server svr = new Server(new ServerAddress(server));
                rep = new Repository(svr);
                Utilities.SetClientRoot(rep, TestDir, cptype, ws_client);

                using (P4Server target = new P4Server(server, user, pass, ws_client))
                {
                    // step 1 : set net.parallel.max to > 0
                    Assert.IsTrue(target.RunCommand("configure", 7, true, new string[] { "set", "net.parallel.max=4" }, 2));
                    // disconnect so that we get a connection that recognizes the configurable
                }

                using (P4Server target = new P4Server(server, user, pass, ws_client))
                {
                    // revert anything in progress, it just makes testing harder
                    Assert.IsTrue(target.RunCommand("revert", 7, true, new string[] { "//..." }, 1));
                    // also sync everything
                    Assert.IsTrue(target.RunCommand("sync", 7, true, new string[] { "-f", "//..." }, 2));

                    // get the big file list
                    StringList localFiles = getClientFiles(target);

                    // 2b: use the default handler, verify that we got synced files and no errors
                    target.SetParallelTransferCallback(null);
                    OpenAndModifyFiles(localFiles, target);
                    Assert.IsTrue(target.RunCommand("submit", 7, true, new string[] { "--parallel", "threads=4,batch=8,min=1", "-d", "this should create a submitted CL" }, 4));

                    // verify that we submitted
                    Assert.IsTrue(target.RunCommand("changes", 7, true, new string[] { "-m1", "-L" }, 2));
                    TaggedObjectList results = target.GetTaggedOutput(7);
                    TaggedObject res = results[0];
                    string desc = "";
                    res.TryGetValue("desc", out desc);
                    Assert.AreEqual(desc, "this should create a submitted CL");
                    string status = "";
                    res.TryGetValue("status", out status);
                    Assert.AreEqual(status, "submitted");

                    // step 3: error case
                    // edit the files, but delete them before the submit.  the transmit errors should
                    // happen on the other threads and get forwarded to the initiator
                    OpenAndModifyFiles(localFiles, target);
                    foreach (string file in localFiles)
                        System.IO.File.Delete(file);
                    // now submit
                    try
                    {
                        // this should throw an error
                        Assert.IsFalse(target.RunCommand("submit", 7, true, new string[] { "--parallel", "threads=4,batch=8,min=1", "-d", "this should not create a submitted CL" }, 4));
                    }
                    catch (P4Exception e)
                    {
                        // loop through the NextErrors looking for the
                        // Submit aborted message
                        bool foundIt = false;
                        bool notNull = true;
                        P4Exception current = e;
                        while (notNull)
                        {
                            if (foundIt = current.Message.StartsWith("Submit aborted"))
                            {
                                break;
                            }
                            if (notNull = current.NextError != null)
                            {
                                current = current.NextError;
                            }
                        }
                        Assert.IsTrue(foundIt);
                        P4ClientErrorList errs = target.GetErrorResults(7);
                        Assert.IsTrue(errs.Count > 0);  // casually looking at the data, I get 8, one for each file and a general "this failed"
                    }
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
                p4d?.Dispose();
                rep?.Dispose();
            }
        }

        [TestMethod()]
        public void ParallelServerCancelTest()
        {
            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";
            Process p4d = null;
            var cptype = Utilities.CheckpointType.A;

            Thread proc = null;
            // to let us know that the cancel was caught
            ManualResetEvent caughtCancel = new ManualResetEvent(false);

            try
            {
                p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                Assert.IsNotNull(p4d, "Setup Failure");

                using (P4Server target = new P4Server(server, user, pass, ws_client))
                {
                    // step 1 : set net.parallel.threads to > 0
                    Assert.IsTrue(target.RunCommand("configure", 7, true, new string[] { "set", "net.parallel.max=4" }, 2));
                }

                // sometimes because of timing issues we miss-fail the test (it hangs)
                // to counter that, run the test N times until it passes with a limited
                // loop count for sleeping
                int testTries = 5;
                while (!caughtCancel.WaitOne(0) && testTries-- >= 0)
                {
                    using (P4Server target = new P4Server(server, user, pass, ws_client))
                    {
                        // to let the main thread know when to start looking for parallel ops to start
                        ManualResetEvent starting = new ManualResetEvent(false);
                        // to let the main thread know that the sync competed
                        ManualResetEvent done = new ManualResetEvent(false);

                        proc = new Thread(() =>
                        {
                            try
                            {
                                target.SetThreadOwner(Thread.CurrentThread.ManagedThreadId);
                                Thread.CurrentThread.IsBackground = true;
                                starting.Set();
                                Assert.IsTrue(target.RunCommand("sync", 7, true, new string[] { "--parallel", "threads=4,batch=8,batchsize=1,min=1,minsize=1", "-f", "//..." }, 4));
                                // the sync will throw over this Set() if we are successful
                                done.Set();
                            }
                            catch (Exception e)
                            {
                                caughtCancel.Set();
                                logger.Debug(e.Message);
                            }
                        });
                        // after this runs, the desired state is done() is not set and caughtCancel() is set

                        // these things have to happen at just the right moment or we miss the test
                        proc.Start();
                        starting.WaitOne();
                        // similarly if we cancel too soon, we will never see the threads' connections created
                        while (target.GetParallelOperationCount() < 4)
                            Thread.Sleep(100);

                        // connections are up, now cancel them
                        target.CancelCommand(7);

                        // recheck the cancel 
                        int retries = 5;
                        while (!caughtCancel.WaitOne(250) && retries-- >= 0 && !done.WaitOne(250)) ;
                        // we only sync a small number of files, so if it doesn't complete in ~3s it's hung
                        // check that either we got the cancel or done is set
                        Assert.IsTrue(caughtCancel.WaitOne(0) || done.WaitOne(0));

                        // missing the cancel means we missed it (success sync before cancel called), so maybe try the whole thing again
                    }
                }
                Assert.IsTrue(caughtCancel.WaitOne(0));
            }
            catch (P4CommandCanceledException ex)
            {
                // I don't think that it ever gets here
                caughtCancel.Set();
                logger.Debug(ex.Message);
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
                p4d?.Dispose();
            }
        }

        /// <summary>
        ///A test for Client
        ///</summary>
        [TestMethod()]
        public void ClientTestA()
        {
            ClientTest(Utilities.CheckpointType.A);
        }
        [TestMethod()]
        public void ClientTestU()
        {
            ClientTest(Utilities.CheckpointType.U);
        }

        private void ClientTest(Utilities.CheckpointType cptype)
        {
            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            // turn off exceptions for this test
            ErrorSeverity oldExceptionLevel = P4Exception.MinThrowLevel;
            P4Exception.MinThrowLevel = ErrorSeverity.E_NOEXC;

            Process p4d = null;

            try
            {
                p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                Assert.IsNotNull(p4d, "Setup Failure");

                using (P4Server target = new P4Server(server, user, pass, ws_client))
                {
                    if (cptype == Utilities.CheckpointType.U)
                        Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                    else
                        Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                    uint cmdId = 7;
                    Assert.IsTrue(target.RunCommand("dirs", cmdId, false, new string[] { "//depot/*" }, 1),
                        "\"dirs\" command failed");

                    string actual = target.Client;
                    Assert.AreEqual(actual, "admin_space");

                    target.Client = "admin_space2";

                    // run a command to trigure a reconnect
                    Assert.IsTrue(target.RunCommand("dirs", ++cmdId, false, new string[] { "//admin_space2/*" }, 1),
                        "\"dirs //admin_space2/*\" command failed");

                    // try a bad value
                    target.Client = "admin_space3";

                    Assert.IsTrue(target.RunCommand("dirs", ++cmdId, false, new string[] { "//admin_space3/*" }, 1),
                        "\"dirs //admin_space3/*\" command failed");

                    P4ClientErrorList ErrorList = target.GetErrorResults(cmdId);

                    Assert.IsNotNull(ErrorList);
                    Assert.AreEqual(ErrorSeverity.E_WARN, ErrorList[0].SeverityLevel);
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
                p4d?.Dispose();
            }

            // reset the exception level
            P4Exception.MinThrowLevel = oldExceptionLevel;
        }

        /// <summary>
        ///A test for DataSet
        ///</summary>
        [TestMethod()]
        public void DataSetTest()
        {
            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    using (P4Server target = new P4Server(server, user, pass, ws_client))
                    {
                        if (cptype == Utilities.CheckpointType.U)
                            Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                        else
                            Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                        string expected = "The quick brown fox jumped over the tall white fence.";
                        target.SetDataSet(7, expected);

                        string actual = target.GetDataSet(7);
                        Assert.AreEqual(actual, expected);
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                    p4d?.Dispose();
                }
            }
        }

        /// <summary>
        ///A test for Password
        ///</summary>
        [TestMethod()]
        public void PasswordTestA()
        {
            PasswordTest(Utilities.CheckpointType.A);
        }

        [TestMethod()]
        public void PasswordTestU()
        {
            PasswordTest(Utilities.CheckpointType.U);
        }

        private void PasswordTest(Utilities.CheckpointType cptype)
        {
            string server = configuration.ServerPort;
            string user = "Alex";
            string pass = "pass";
            string ws_client = "admin_space";

            // turn off exceptions for this test
            ErrorSeverity oldExceptionLevel = P4Exception.MinThrowLevel;
            P4Exception.MinThrowLevel = ErrorSeverity.E_NOEXC;

            Process p4d = null;

            try
            {
                if (cptype == Utilities.CheckpointType.U)
                    user = "Алексей";

                p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                Assert.IsNotNull(p4d, "Setup Failure");

                using (P4Server target = new P4Server(server, user, pass, ws_client))
                {
                    if (cptype == Utilities.CheckpointType.U)
                        Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                    else
                        Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                    string actual = target.Password;
                    Assert.IsFalse(string.IsNullOrEmpty(actual));

                    /// try a bad value
                    target.Password = "ssap";

                    uint cmdId = 7;
                    // command triggers a reconnect
                    Assert.IsFalse(target.RunCommand("dirs", cmdId, false, new string[] { "//depot/*" }, 1),
                        "\"dirs\" command failed");

                    // try a user with no password
                    target.User = "admin";
                    target.Password = string.Empty;

                    // command triggers a reconnect
                    Assert.IsTrue(target.RunCommand("dirs", ++cmdId, false, new string[] { "//depot/*" }, 1),
                        "\"dirs\" command failed");
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
                p4d?.Dispose();
            }

            // reset the exception level
            P4Exception.MinThrowLevel = oldExceptionLevel;
        }

        /// <summary>
        ///A test for Password
        ///</summary>
        [TestMethod()]
        public void PasswordTest2A()
        {
            PasswordTest2(Utilities.CheckpointType.A);
        }

        // From case 00808744

        private void PasswordTest2(Utilities.CheckpointType cptype)
        {
            string p4port = configuration.ServerPort;
            string user = "Alex";
            string pass = "pass";

            // turn off exceptions for this test
            ErrorSeverity oldExceptionLevel = P4Exception.MinThrowLevel;
            P4Exception.MinThrowLevel = ErrorSeverity.E_NOEXC;

            Process p4d = null;

            P4Server.ErrorDelegate OnConnectionErrorRecieved =
                new P4Server.ErrorDelegate(ErrorResultsCallback);

            try
            {
                if (cptype == Utilities.CheckpointType.U)
                    user = "Алексей";

                p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                Assert.IsNotNull(p4d, "Setup Failure");

                var server = new Server(new ServerAddress(p4port));
                using (var repository = new Repository(server, _multithreaded: true))
                {
                    var connection = repository.Connection;
                    connection.UserName = user;
                    connection.Connect(null);
                    connection.Client = null;
                    connection.ErrorReceived += OnConnectionErrorRecieved;
                    connection.CommandTimeout = TimeSpan.FromSeconds(5);

                    var credential = connection.Login(pass,
                        new LoginCmdOptions(LoginCmdFlags.DisplayTicket, null));
                    if (credential is null)
                    {
                        connection.Disconnect();
                        Assert.Fail();
                        return;
                    }

                    connection.Credential = credential;
                    var svr = connection.getP4Server();
                    var pw = svr.Password;
                    Task.Run(() =>
                    {
                        try
                        {
                            //connection.Credential = connection.Credential;
                            // connection.UserName = connection.UserName;
                            // connection.Client = connection.Client;

                            var foo = repository.GetUser(connection.UserName);
                        }
                        catch (Exception ex)
                        {
                            var msg = ex.Message;
                            // error handling
                        }
                    }).Wait();
                    var svr1 = connection.getP4Server();
                    var pw1 = svr1.Password;
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
                p4d?.Dispose();
            }
        }



        /// <summary>
        ///A test for Port
        ///</summary>
        [TestMethod()]
        public void PortTestA()
        {
            PortTest(Utilities.CheckpointType.A);
        }

        [TestMethod()]
        public void PortTestU()
        {
            PortTest(Utilities.CheckpointType.U);
        }

        public void PortTest(Utilities.CheckpointType cptype)
        {
            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            // turn off exceptions for this test
            ErrorSeverity oldExceptionLevel = P4Exception.MinThrowLevel;
            P4Exception.MinThrowLevel = ErrorSeverity.E_NOEXC;

            Process p4d = null;
            try
            {
                p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                Assert.IsNotNull(p4d, "Setup Failure");

                using (P4Server target = new P4Server(server, user, pass, ws_client))
                {
                    if (cptype == Utilities.CheckpointType.U)
                        Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                    else
                        Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                    string expected = configuration.ServerPort;
                    target.Port = expected;

                    string actual = target.Port;
                    Assert.AreEqual(actual, expected);

                    // try a bad value
                    target.Port = "null:0";

                    uint cmdId = 7;

                    // command triggers a reconnect
                    bool reconectSucceeded = true;
                    try
                    {
                        reconectSucceeded = target.RunCommand("dirs", cmdId, false, new string[] { "//depot/*" }, 1);
                    }
                    catch
                    {
                        reconectSucceeded = false;
                    }

                    Assert.IsFalse(reconectSucceeded, "Reconnect to \"null:0\" did not fail");
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
                p4d?.Dispose();
            }
            // reset the exception level
            P4Exception.MinThrowLevel = oldExceptionLevel;
            // release the connection error if any
            P4Bridge.ClearConnectionError();
        }

        /// <summary>
        ///A test for User
        ///</summary>
        [TestMethod()]
        public void UserTest()
        {
            string server = configuration.ServerPort;
            string user = "Alex";
            string pass = "pass";
            string ws_client = "admin_space";

            // turn off exceptions for this test
            ErrorSeverity oldExceptionLevel = P4Exception.MinThrowLevel;
            P4Exception.MinThrowLevel = ErrorSeverity.E_NOEXC;

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                Utilities.CheckpointType cptype = (Utilities.CheckpointType)i;
                try
                {
                    if (cptype == Utilities.CheckpointType.U)
                        user = "Алексей";

                    p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    using (P4Server target = new P4Server(server, user, pass, ws_client))
                    {
                        if (cptype == Utilities.CheckpointType.U)
                            Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                        else
                            Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                        string actual = target.User;
                        Assert.AreEqual(actual, user);

                        // try a bad value
                        target.User = "John";

                        uint cmdId = 7;
                        // command triggers a reconnect
                        bool success = target.RunCommand("dirs", cmdId, false, new string[] { "//depot/*" }, 1);
                        Assert.IsTrue(success, "\"dirs\" command failed");

                        P4ClientErrorList errors = target.GetErrorResults(cmdId);

                        // try a user with no password
                        target.User = "admin";
                        target.Password = string.Empty;

                        // command triggers a reconnect
                        Assert.IsTrue(target.RunCommand("dirs", ++cmdId, false, new string[] { "//depot/*" }, 1),
                            "\"dirs\" command failed");
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                    p4d?.Dispose();
                }
            }
            // reset the exception level
            P4Exception.MinThrowLevel = oldExceptionLevel;
        }

        /// <summary>
        ///A test for ErrorList
        ///</summary>
        [TestMethod()]
        public void ErrorListTest()
        {
            string server = configuration.ServerPort;
            string user = "Alex";
            string pass = "pass";
            string ws_client = "admin_space";

            // turn off exceptions for this test
            ErrorSeverity oldExceptionLevel = P4Exception.MinThrowLevel;
            P4Exception.MinThrowLevel = ErrorSeverity.E_NOEXC;

            Process p4d = null;

            // refactor this to test each one individually as its own test
            for (int i = 0; i < 3; i++) // run once for ascii, once for unicode, once for the security level 3 server
            {
                var cptype = (Utilities.CheckpointType)i;
                try
                {
                    if (cptype == Utilities.CheckpointType.U)
                    {
                        user = "Алексей";
                        pass = "pass";
                    }
                    if (cptype == Utilities.CheckpointType.S3)
                    {
                        user = "Alex";
                        pass = "Password";  // was Password
                    }

                    p4d = Utilities.DeployP4TestServer(TestDir, 10, cptype, "ErrorListTest");

                    using (P4Server target = new P4Server(server, user, pass, ws_client))
                    {
                        if (cptype == Utilities.CheckpointType.U)
                            Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                        else
                            Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                        P4ClientErrorList errors = null;

                        uint cmdId = 7;
                        // a bad user will not fail on the servers from a.tar and u.tar, 
                        // so only test on s3.tar
                        if (cptype == Utilities.CheckpointType.S3)
                        {
                            // setting the user name will trigger a disconnect
                            target.User = "badboy"; // nonexistent user

                            // command triggers an attempt to reconnect
                            Assert.IsFalse(target.RunCommand("dirs", ++cmdId, false, new string[] { "//depot/*" }, 1));

                            errors = target.GetErrorResults(cmdId);

                            Assert.IsNotNull(errors);
                            Assert.IsTrue(
                                errors[0].ErrorMessage.Contains("Password must be set before access can be granted"));

                            target.User = user; // back to the good  user
                        }

                        target.Password = "NoWayThisWillWork"; // bad password
                        if (cptype == Utilities.CheckpointType.S3)
                        {
                            Assert.IsFalse(target.Login("NoWayThisWillWork", null));
                            P4ClientError conError = target.ConnectionError;

                            Assert.IsNotNull(conError);
                            Assert.IsTrue(conError.ErrorMessage.Contains("Password invalid"));
                        }
                        // command triggers an attempt to reconnect
                        Assert.IsFalse(target.RunCommand("dirs", ++cmdId, false, new string[] { "//depot/*" }, 1));

                        errors = target.GetErrorResults(cmdId);

                        Assert.IsNotNull(errors);
                        Assert.AreEqual(errors[0].ErrorCode, 807672853);
                        target.Password = pass; // back to the good password
                        if (i == 2)
                        {
                            Assert.IsTrue(target.Login(pass, null));
                        }

                        target.Client = "AintNoClientNamedLikeThis";

                        // command triggers an attempt to reconnect (have requires a client)
                        Assert.IsFalse(target.RunCommand("have", ++cmdId, false, null, 0));

                        errors = target.GetErrorResults(cmdId);

                        Assert.IsNotNull(errors);
                        Assert.AreEqual(errors[0].ErrorCode, 855775267);
                        //Assert.IsTrue(errors[0].ErrorMessage.Contains("unknown - use 'client' command to create it"));

                        target.Client = ws_client; // back to the good client name
                        target.Port = "NoServerAtThisAddress:666";

                        // command triggers an attempt to reconnect
                        Assert.IsFalse(target.RunCommand("dirs", ++cmdId, false, new string[] { "//depot/*" }, 1));

                        errors = target.GetErrorResults(cmdId);
                        P4ClientInfoMessageList info = target.GetInfoResults(cmdId);

                        System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(target.IdleDisconnectWaitTime + 10));

                        P4ClientError conErr = target.ConnectionError;
                        if (errors == null)
                        {
                            Assert.IsNotNull(conErr);
                            Assert.AreEqual(conErr.ErrorCode, 824577061);
                            //Assert.IsTrue(conErr.ErrorMessage.Contains("Connect to server failed"));
                        }
                        else
                        {
                            Assert.IsNotNull(errors);
                            Assert.AreEqual(errors[0].ErrorCode, 824577061);
                            //Assert.IsTrue(errors[0].ErrorMessage.Contains("Connect to server failed"));
                        }
                        target.Port = server; // back to the good port name

                        //if (i == 2)
                        //{
                        target.Password = null;
                        Assert.IsTrue(target.Login(pass, null));
                        //}
                        System.Threading.Thread.Sleep(100);

                        // command triggers an attempt to reconnect
                        // run a good command to make sure the connection is solid
                        bool success = target.RunCommand("dirs", ++cmdId, false, new string[] { "//depot/*" }, 1);

                        errors = target.GetErrorResults(cmdId);
                        if (!success)
                        {
                            P4ClientInfoMessageList InfoOut = target.GetInfoResults(cmdId);
                            Assert.IsNull(InfoOut);
                        }

                        Assert.IsTrue(success);
                        Assert.IsNull(errors);

                        // try a bad command name
                        Assert.IsFalse(target.RunCommand("dirrrrrs", ++cmdId, false, new string[] { "//depot/*" }, 1));

                        errors = target.GetErrorResults(cmdId);

                        Assert.IsNotNull(errors);
                        Assert.AreEqual(errors[0].ErrorCode, 805379098);
                        //Assert.IsTrue(errors[0].ErrorMessage.Contains("Unknown command.  Try 'p4 help' for info"));

                        target.Port = server; // back to the good port name
                        if (i == 2)
                        {
                            target.Password = null;
                            Assert.IsTrue(target.Login(pass, null));
                            P4ClientError conError = target.ConnectionError;
                        }

                        // try a bad command parameter
                        Assert.IsFalse(target.RunCommand("dirs", ++cmdId, false, new string[] { "//freebird/*" }, 1));

                        errors = target.GetErrorResults(cmdId);

                        Assert.IsNotNull(errors);
                        Assert.AreEqual(errors[0].ErrorCode, 838998116);
                        //						Assert.IsTrue(errors[0].ErrorMessage.Contains("must refer to client"));

                        // try a bad command flag
                        Assert.IsFalse(target.RunCommand("dirs", ++cmdId, false, new string[] { "-UX", "//depot/*" }, 1));

                        errors = target.GetErrorResults(cmdId);

                        Assert.IsNotNull(errors);
                        Assert.AreEqual(errors[0].ErrorCode, 822150148);
                        //						Assert.IsTrue(errors[0].ErrorMessage.Contains("Invalid option: -UX"));
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                    p4d?.Dispose();
                }
            }
            // reset the exception level
            P4Exception.MinThrowLevel = oldExceptionLevel;
        }

        /// <summary>
        ///A test for ApiLevel
        ///</summary>
        [TestMethod()]
        public void ApiLevelTest()
        {
            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    using (P4Server target = new P4Server(server, user, pass, ws_client))
                    {
                        if (cptype == Utilities.CheckpointType.U)
                            Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                        else
                            Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                        Assert.IsTrue(target.ApiLevel > 0);
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                    p4d?.Dispose();
                }
            }
        }

        /// <summary>
        ///A test for GetTaggedOutput
        ///</summary>
        [TestMethod()]
        public void TestTaggedUntaggedOutputTest()
        {
            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    using (P4Server target = new P4Server(server, user, pass, ws_client))
                    {
                        if (cptype == Utilities.CheckpointType.U)
                            Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                        else
                            Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                        string[] parms = new string[] { "//depot/mycode/*" };

                        uint cmdId = 7;
                        Assert.IsTrue(target.RunCommand("files", cmdId, false, parms, 1),
                            "\"files\" command failed");

                        TaggedObjectList results = target.GetTaggedOutput(cmdId);

                        Assert.IsNull(results, "GetTaggedOutput did not return null data");

                        Assert.IsTrue(target.RunCommand("files", ++cmdId, true, parms, 1),
                            "\"files\" command failed");

                        results = target.GetTaggedOutput(cmdId);

                        Assert.IsNotNull(results, "GetTaggedOutput returned null data");

                        if (cptype == Utilities.CheckpointType.U)
                            Assert.AreEqual(3, results.Count);
                        else
                            Assert.AreEqual(3, results.Count);

                        Assert.IsTrue(target.RunCommand("files", ++cmdId, false, parms, 1),
                            "\"files\" command failed");

                        results = target.GetTaggedOutput(cmdId);

                        Assert.IsNull(results, "GetTaggedOutput did not return null data");
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                    p4d?.Dispose();
                }
            }
        }

        /// <summary>
        /// Another test for IKeepAlive
        /// </summary>
        [TestMethod()]
        public void KeepAliveTest2()
        {
            // set a prompt handler, wait until the prompt comes in, cancel the command, get the "cancelled" state
            // then launch another command and watch it succeed

            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    using (P4Server target = new P4Server(server, user, pass, ws_client))
                    {
                        if (cptype == Utilities.CheckpointType.U)
                            Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                        else
                            Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                        target.PromptHandler = PromptWaiter;
                        // attempt to change the password, this guarantees a prompt
                        using (P4Command cmd = new P4Command(target, "passwd", false))
                        {
                        // start a thread to cancel as soon as the prompt comes in
                        System.Threading.Thread t = new System.Threading.Thread(KeepAliveKiller);
                        testKiller = new KillParams();
                        testKiller.server = target;
                        testKiller.promptCalled = new ManualResetEvent(false);
                        testKiller.cancelCalled = new ManualResetEvent(false);
                        testKiller.cmd = cmd;
                        testKiller.cmdId = cmd.CommandId;
                        t.Start();
                        cmd.CmdPromptHandler = PromptWaiter;

                        bool cancelledException = false;
                        try
                        {
                            P4CommandResult result = cmd.Run();
                            // Run() should throw an error
                        }
                        catch (P4CommandCanceledException)
                        {
                            cancelledException = true;
                        }

                        Assert.IsTrue(cancelledException, "Did not receive cancel exception");
                        }

                        // run another command, but run a reconnect as "cancel" terminates the session
                        target.Reconnect();
                        target.PromptHandler = null;
                        using (P4Command cmd1 = new P4Command(target, "depots", false))
                        {
                            bool cancelledException = false;
                        try
                        {
                                P4CommandResult result = cmd1.Run();
                            // no exception
                        }
                        catch (P4CommandCanceledException)
                        {
                            cancelledException = true;
                        }

                        Assert.IsFalse(cancelledException, "Received cancel exception");
                    }
                }
                }
                finally
                {
                    testKiller = new KillParams();
                    Utilities.RemoveTestServer(p4d, TestDir);
                    p4d?.Dispose();
                }
            }
        }

        private struct KillParams
        {
            public ManualResetEvent promptCalled;
            public ManualResetEvent cancelCalled;
            public uint cmdId;
            public P4Server server;
            public P4Command cmd;
        }

        private static KillParams testKiller;

        private string PromptWaiter(uint cmdId, string msg, bool displayText)
        {
            testKiller.promptCalled.Set();
            // wait until the cancel got called
            testKiller.cancelCalled.WaitOne();
            // wait for a few milliseconds for the C++ level to notice that the command was cancelled
            Thread.Sleep(300);
            return "nothing";
        }

        private static void KeepAliveKiller()
        {
            // wait for the prompt
            testKiller.promptCalled.WaitOne();
            // stop the command
            testKiller.server.CancelCommand(testKiller.cmdId);
            // let the prompter continue
            testKiller.cancelCalled.Set();
            // we're done
        }

        /// <summary>
        ///A test for IKeepAlive
        ///</summary>
        [TestMethod()]
        public void KeepAliveTest()
        {
            // set a prompt handler, wait until the prompt comes in, cancel the command, get the "cancelled" state

            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    using (P4Server target = new P4Server(server, user, pass, ws_client))
                    {
                        if (cptype == Utilities.CheckpointType.U)
                            Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                        else
                            Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                        target.PromptHandler = PromptWaiter;
                        // attempt to change the password, this guarantees a prompt
                        using (P4Command cmd = new P4Command(target, "passwd", false))
                        {
                        // start a thread to cancel as soon as the prompt comes in
                        System.Threading.Thread t = new System.Threading.Thread(KeepAliveKiller);
                        testKiller = new KillParams();
                        testKiller.server = target;
                        testKiller.promptCalled = new ManualResetEvent(false);
                        testKiller.cancelCalled = new ManualResetEvent(false);
                        testKiller.cmd = cmd;
                        testKiller.cmdId = cmd.CommandId;
                        t.Start();
                        cmd.CmdPromptHandler = PromptWaiter;

                        bool cancelledException = false;
                        try
                        {
                            P4CommandResult result = cmd.Run();
                            // Run() should throw an error
                        }
                        catch (P4CommandCanceledException)
                        {
                            cancelledException = true;
                        }

                        Assert.IsTrue(cancelledException);
                    }
                }
                }
                finally
                {
                    testKiller = new KillParams();
                    Utilities.RemoveTestServer(p4d, TestDir);
                    p4d?.Dispose();
                }
            }
        }

        private string ClearP4VarSetting(string var)
        {
            try
            {
                using (Process p = new Process())
                {
                ProcessStartInfo ps = new ProcessStartInfo();
                ps.FileName = "P4";
                ps.Arguments = string.Format("set {0}=", var);
                ps.RedirectStandardOutput = true;
                ;
                ps.UseShellExecute = false;
                ;

                p.StartInfo = ps;

                p.Start();

                string output = p.StandardOutput.ReadToEnd();

                p.WaitForExit();

                return output;
            }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        ///A test for Get/Set
        ///</summary>
        [TestMethod()]
        public void GetSetTest()
        {
            string expected = "C:\\login.bat";
            P4Server.Set("P4FOOBAR", expected);
            string value = P4Server.Get("P4FOOBAR");
            Assert.AreEqual(expected, value);

            expected = "D:\\login.bat";
            P4Server.Set("P4FOOBAR", expected);
            value = P4Server.Get("P4FOOBAR");
            Assert.AreEqual(expected, value);

            P4Server.Set("P4FOOBAR", null);
            value = P4Server.Get("P4FOOBAR");
            // Assert.IsNull(value);    // FIXME  reenable test after job092355 is fixed
        }

        /// <summary>
        ///A test for IsIgnored
        ///</summary>
        [TestMethod()]
        public void IsIgnoredTest()
        {
            var cptype = Utilities.CheckpointType.A;
            string oldIgnore = null;
            var clientRoot = Utilities.TestClientRoot(TestDir, cptype);

            var adminSpace = Path.Combine(clientRoot, "admin_space");
            string mycode = Path.Combine(adminSpace, "MyCode");
            string myp4ignore = Path.Combine(mycode, "myp4ignore.txt");
            string foofoo = Path.Combine(mycode, "foofoofoo.foo");
            string moomoo = Path.Combine(mycode, "moomoomoo.foo");
            string ruabvg = Path.Combine(mycode, "myp4ignore_АБВГ.txt");

            try
            {
                Directory.CreateDirectory(mycode);

                oldIgnore = P4Server.Get("P4IGNORE");

                P4Server.Update("P4IGNORE", "myp4ignore.txt");
                string val = P4Server.Get("P4IGNORE");
                Assert.AreEqual("myp4ignore.txt", val);

                Environment.CurrentDirectory = mycode;
                if (System.IO.File.Exists(myp4ignore))
                {
                    System.IO.File.Delete(myp4ignore);
                }
                using (System.IO.StreamWriter sw = new StreamWriter(myp4ignore))
                {
                    sw.WriteLine("foofoofoo.foo");
                    sw.Close();
                }
                Assert.IsTrue(P4Server.IsIgnored(foofoo));
                Assert.IsFalse(P4Server.IsIgnored(moomoo));
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message + Environment.NewLine + ex.StackTrace);
            }
            finally
            {
                P4Server.Set("P4IGNORE", oldIgnore);

                if (System.IO.File.Exists(myp4ignore))
                {
                    System.IO.File.Delete(myp4ignore);
                }
                if (System.IO.File.Exists(ruabvg))
                {
                    System.IO.File.Delete(ruabvg);
                }
                P4Server.Update("P4IGNORE", "");
            }
        }

        /// <summary>
        ///A test for IsIgnoredjob100206
        ///</summary>
        [TestMethod()]
        public void IsIgnoredTestjob100206()
        {
            string oldIgnore = P4Server.Get("P4IGNORE");

            string noignore = Path.Combine(TestDir, "NoIgnore");
            string myignore = Path.Combine(TestDir, "MyIgnore");
            string p4ignore = Path.Combine(myignore, ".p4ignore");

            try
            {
                Directory.CreateDirectory(TestDir);
                Directory.CreateDirectory(myignore);
                Directory.CreateDirectory(noignore);

                // set P4IGNORE in enviro
                P4Server.Update("P4IGNORE", ".p4ignore");
                string val = P4Server.Get("P4IGNORE");
                Assert.AreEqual(val, ".p4ignore");

                // Create a .p4ignore file
                System.IO.File.Create(p4ignore).Dispose();
                System.IO.File.AppendAllText(p4ignore, ".p4ignore");

                // Test in a directory containing the .p4ignore file
                Environment.CurrentDirectory = myignore;
                Assert.IsTrue(P4Server.IsIgnored(".p4ignore"));
                Assert.IsTrue(P4Server.IsIgnored(p4ignore));

                // Test in a directory NOT containing the .p4ignore file
                Environment.CurrentDirectory = noignore;
                Assert.IsFalse(P4Server.IsIgnored(".p4ignore"));
            }

            finally
            {
                P4Server.Update("P4IGNORE", null);
                P4Server.Set("P4IGNORE", oldIgnore);
                System.IO.File.Delete(p4ignore);
            }
        }

        /// <summary>
        ///A test for GetConfig()
        ///</summary>
        [TestMethod()]
        public void GetConfigTestA()
        {
            GetConfigTest(Utilities.CheckpointType.A);
        }
        /// <summary>
        ///A test for GetConfig()
        ///</summary>
        [TestMethod()]
        public void GetConfigTestU()
        {
            GetConfigTest(Utilities.CheckpointType.U);
        }

        public void GetConfigTest(Utilities.CheckpointType cptype)
        {
            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            string adminSpace = "";

            try
            {
                p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                Assert.IsNotNull(p4d, "Setup Failure");

                var clientRoot = Utilities.TestClientRoot(TestDir, cptype);
                adminSpace = Path.Combine(clientRoot, "admin_space");

                using (P4Server target = new P4Server(server, user, pass, ws_client))
                {
                    if (cptype == Utilities.CheckpointType.U)
                        Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                    else
                        Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                    string expected = Path.Combine(adminSpace, "MyCode", "myP4Config.txt");
                    P4Server.Update("P4CONFIG", "myP4Config.txt");
                    if (System.IO.File.Exists(expected))
                    {
                        System.IO.File.Delete(expected);
                    }

                    //make sure it returns null if no config file
                    string actual = P4Server.GetConfig(Path.Combine(adminSpace, "MyCode"));
                    if (actual != null)
                    {
                        Assert.AreEqual(actual, "noconfig", true);
                    }
                    using (System.IO.StreamWriter sw = new StreamWriter(expected))
                    {
                        sw.WriteLine("P4CLIENT=admin_space");
                    }

                    actual = P4Server.GetConfig(Path.Combine(adminSpace, "MyCode"));
                    Assert.AreEqual(actual, expected, true);

                    target.Disconnect();

                    target.CurrentWorkingDirectory = Path.Combine(adminSpace, "MyCode");

                    actual = target.Config;
                    Assert.AreEqual(actual, expected, true);
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
                p4d?.Dispose();

                P4Server.Update("P4CONFIG", null);

                if (System.IO.File.Exists(Path.Combine(adminSpace, "MyCode", "myP4Config.txt")))
                {
                    System.IO.File.Delete(Path.Combine(adminSpace, "MyCode", "myP4Config.txt"));
                }
            }
        }

        /// <summary>
        ///A test for GetConfigjob100191A()
        ///</summary>
        [TestMethod()]
        public void GetConfigTestjob100191A()
        {
            GetConfigTestjob100191(Utilities.CheckpointType.A);
        }
        /// <summary>
        ///A test for GetConfigjob100191U()
        ///</summary>
        [TestMethod()]
        public void GetConfigTestjob100191U()
        {
            GetConfigTestjob100191(Utilities.CheckpointType.U);
        }
        public void GetConfigTestjob100191(Utilities.CheckpointType cptype)  // Fixme requires enviro fix
        {
            string uri = configuration.ServerPort;
            string pass = string.Empty;

            Process p4d = null;
            Repository rep = null;

            string adminSpace = "";
            string topSpace = "";
            string midSpace = "";
            string botSpace = "";
            string topConfig = "";
            string midConfig = "";
            string botConfig = "";

            try
            {
                p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                Assert.IsNotNull(p4d, "Setup Failure");

                var clientRoot = Utilities.TestClientRoot(TestDir, cptype);
                adminSpace = Path.Combine(clientRoot, "admin_space");

                topSpace = Path.Combine(adminSpace, "top_level");
                midSpace = Path.Combine(topSpace, "mid_level");
                botSpace = Path.Combine(midSpace, "bottom_level");

                topConfig = Path.Combine(topSpace, "p4config");
                midConfig = Path.Combine(midSpace, "p4config");
                botConfig = Path.Combine(botSpace, "p4config");

                Server server = new Server(new ServerAddress(uri));

                rep = new Repository(server);

                using (Connection con = rep.Connection)
                {
                    // create some directories
                    Directory.CreateDirectory(botSpace);

                    // create some config files
                    System.IO.File.Create(topConfig).Dispose();
                    System.IO.File.Create(midConfig).Dispose();
                    System.IO.File.Create(botConfig).Dispose();

                    string contents = "P4PORT=" + configuration.ServerPort + Environment.NewLine +
                                      "P4USER=admin" + Environment.NewLine +
                                      "P4CLIENT=admin_space" + Environment.NewLine;
                    System.IO.File.AppendAllText(topConfig, contents);
                    System.IO.File.AppendAllText(midConfig, contents);
                    System.IO.File.AppendAllText(botConfig, contents);

                    P4Server.Update("P4CONFIG", "p4config");

                    Options options = new Options();
                    options["cwd"] = botSpace;
                    con.Connect(options);

                    P4Bridge.ListEnviro();

                    string config1 = con.GetP4ConfigFile();
                    Assert.AreEqual(botConfig, config1);
                    config1 = con.GetP4ConfigFile(botSpace);
                    Assert.AreEqual(botConfig, config1);
                    config1 = P4Server.GetConfig(botSpace);
                    Assert.AreEqual(botConfig, config1);

                    con.Disconnect();
                    options["cwd"] = midSpace;
                    // Directory.SetCurrentDirectory(midSpace);
                    con.Connect(options);

                    string config2 = con.GetP4ConfigFile();
                    Assert.AreEqual(midConfig, config2);
                    config2 = con.GetP4ConfigFile(midSpace);
                    Assert.AreEqual(midConfig, config2);
                    config2 = P4Server.GetConfig(midSpace);
                    Assert.AreEqual(midConfig, config2);

                    con.Disconnect();
                    options["cwd"] = topSpace;
                    //Directory.SetCurrentDirectory(topSpace);
                    con.Connect(options);

                    string config3 = con.GetP4ConfigFile();
                    Assert.AreEqual(topConfig, config3);
                    config3 = con.GetP4ConfigFile(topSpace);
                    Assert.AreEqual(topConfig, config3);
                    config3 = P4Server.GetConfig(topSpace);
                    Assert.AreEqual(topConfig, config3);

                    //delete the config files
                    System.IO.File.Delete(topConfig);
                    System.IO.File.Delete(midConfig);
                    System.IO.File.Delete(botConfig);

                    con.Disconnect();
                    options["cwd"] = topSpace;
                    con.Connect(options);

                    string config4 = con.GetP4ConfigFile();
                    Assert.AreEqual("noconfig", config4);
                    config4 = con.GetP4ConfigFile(topSpace);
                    Assert.AreEqual("noconfig", config4);
                    config4 = P4Server.GetConfig(topSpace);
                    Assert.AreEqual("noconfig", config4);

                    con.Disconnect();
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
                p4d?.Dispose();
                rep?.Dispose();

                P4Server.Update("P4CONFIG", "");  // reset our P4CONFIG changes

                // delete the directories we created
                Utilities.DeleteDirectory(botSpace);
                Utilities.DeleteDirectory(midSpace);
                Utilities.DeleteDirectory(topSpace);
            }
        }

        /// <summary>
        ///A test for GetConfig()
        ///</summary>
        [TestMethod()]
        public void ConnectWithConfigFileTest()
        {
            string adminSpace = "";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    var clientRoot = Utilities.TestClientRoot(TestDir, cptype);
                    adminSpace = Path.Combine(clientRoot, "admin_space");
                    Directory.CreateDirectory(adminSpace);

                    // write a config file in the workspace root 

                    string expected = Path.Combine(adminSpace, "myP4Config.txt");
                    P4Server.Update("P4CONFIG", "myP4Config.txt");
                    try
                    {
                        if (System.IO.File.Exists(expected))
                        {
                            System.IO.File.Delete(expected);
                        }
                        using (System.IO.StreamWriter sw = new StreamWriter(expected))
                        {
                            sw.WriteLine("P4PORT=" + configuration.ServerPort);
                            sw.WriteLine("P4USER=admin");
                            sw.WriteLine("P4CLIENT=admin_space");
                        }
                    }
                    catch { Assert.Fail("Could not write config file"); }

                    {
                        string actual = P4Server.GetConfig(Path.Combine(adminSpace, "MyCode"));
                        Assert.AreEqual(expected, actual);
                    }

                    using (P4Server target = new P4Server(Path.Combine(adminSpace, "MyCode")))
                    {
                        if (cptype == Utilities.CheckpointType.U)
                            Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
                        else
                            Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

                        Assert.AreEqual("admin", target.User);
                        Assert.AreEqual("admin_space", target.Client);
                        Assert.AreEqual(configuration.ServerPort, target.Port);

                        string actual = target.Config;
                        Assert.AreEqual(expected, actual, true); // ignore case
                    }
                }
                finally
                {
                    P4Server.Update("P4CONFIG", null);

                    Utilities.RemoveTestServer(p4d, TestDir);
                    p4d?.Dispose();

                    if (System.IO.File.Exists(Path.Combine(adminSpace, "myP4Config.txt")))
                    {
                        System.IO.File.Delete(Path.Combine(adminSpace, "myP4Config.txt"));
                    }
                }
            }
        }

        /// <summary>
        ///A test for shiftjis
        ///</summary>
        [TestMethod()]
        public void ShiftjisFilesTest()
        {
            // only applies to Unicode servers
            var cptype = Utilities.CheckpointType.U;

            string uri = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;
            Repository rep = null;

            try
            {
                Environment.SetEnvironmentVariable("P4CHARSET", "shiftjis");
                p4d = Utilities.DeployP4TestServer(TestDir, 16, cptype, TestContext.TestName);
                Assert.IsNotNull(p4d, "Setup Failure");

                Server server = new Server(new ServerAddress(uri));

                rep = new Repository(server);
                Utilities.SetClientRoot(rep, TestDir, cptype, ws_client, false);

                var clientRoot = Utilities.TestClientRoot(TestDir, cptype);
                string adminSpace = Path.Combine(clientRoot, "admin_space");

                string localPath = Path.Combine(adminSpace, "ああえええええ");

                using (Connection con = rep.Connection)
                {
                    con.UserName = user;
                    con.Client = new Client();
                    con.Client.Name = ws_client;
                    Assert.AreEqual(con.Status, ConnectionStatus.Disconnected);

                    Assert.AreEqual(con.Server.State, ServerState.Unknown);

                    Assert.IsTrue(con.Connect(null));

                    Assert.AreEqual(con.Server.State, ServerState.Online);

                    Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                    Assert.AreEqual("admin", con.Client.OwnerName);

                    // Clear the local directory, so we get a clean sync
                    Utilities.DeleteDirectory(localPath);

                    // confirm that ああえええええ directory does not exist locally
                    Assert.IsFalse(System.IO.Directory.Exists(localPath));

                    // sync the ああえええええ dir
                    FileSpec dfs = new FileSpec(new DepotPath(@"//depot/ああえええええ/..."));
                    Options sFlags = new SyncFilesCmdOptions(
                        SyncFilesCmdFlags.Force,
                        100
                    );
                    IList<FileSpec> rFiles = con.Client.SyncFiles(sFlags, dfs);

                    // confirm that the ああえええええ directory now exists
                    // that the files were synced and that there are 3
                    // files total in the new local directory
                    Assert.IsTrue(System.IO.Directory.Exists(localPath));
                    Assert.IsNotNull(rFiles);
                    Assert.AreEqual(3, rFiles.Count);

                    // confirm that the local filenames were synced correctly
                    Assert.IsTrue(System.IO.File.Exists(rFiles[0].LocalPath.Path));
                    Assert.IsTrue(System.IO.File.Exists(rFiles[1].LocalPath.Path));
                    Assert.IsTrue(System.IO.File.Exists(rFiles[2].LocalPath.Path));

                    // sync to none and confirm directory is empty
                    dfs.Version = new Revision(0);
                    rFiles = con.Client.SyncFiles(sFlags, dfs);
                    Assert.AreEqual(System.IO.Directory.GetFiles(localPath).Length, 0);
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
                p4d?.Dispose();
                rep?.Dispose();
            }
        }

        /// <summary>
        ///A test for SetCharacterSet
        ///</summary>
        [TestMethod()]
        public void SetCharacterSetTest()
        {
            // only applies to Unicode servers
            var cptype = Utilities.CheckpointType.U;

            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;
            string adminSpace = "";

            string origCharset = Environment.GetEnvironmentVariable("P4CHARSET");

            try
            {
                p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                Assert.IsNotNull(p4d, "Setup Failure");

                var clientRoot = Utilities.TestClientRoot(TestDir, cptype);
                adminSpace = Path.Combine(clientRoot, "admin_space");
                Directory.CreateDirectory(adminSpace);

                Environment.SetEnvironmentVariable("P4CHARSET", "utf8-bom");
                // not picking it up from the environment on OSX - FIXME?
                P4Server.Update("P4CHARSET", "utf8-bom");

                using (P4Server target = new P4Server(server, user, pass, ws_client))
                {
                    string gotCharset = P4Server.Get("P4CHARSET");
                    Assert.AreEqual("utf8-bom", gotCharset);

                    //target.SetCharacterSet("utf8", "utf8-bom");
                    Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");

                    Assert.AreEqual("admin", target.User);
                    Assert.AreEqual("admin_space", target.Client);
                    Assert.AreEqual(configuration.ServerPort, target.Port);

                    string localPath = Path.Combine(adminSpace, "MyCode", "Пюп.txt");
                    string depotPath = "//depot/MyCode/Пюп-utf8.txt";

                    Assert.IsTrue(target.RunCommand("revert", 7, true, new string[] { depotPath }, 1));

                    Assert.IsTrue(target.RunCommand("print", 7, true, new string[] { depotPath }, 1));
                    string fileContents = target.GetTextResults(7);

                    Assert.IsTrue(target.RunCommand("sync", 7, true, new string[] { "-f", depotPath }, 2));
                }

            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
                Environment.SetEnvironmentVariable("P4CHARSET", origCharset);
                P4Server.Update("P4CHARSET", "");
                p4d?.Dispose();
            }

            try
            {
                p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                Assert.IsNotNull(p4d, "Setup Failure");

                Environment.SetEnvironmentVariable("P4CHARSET", "utf16le-bom");
                // not picking it up from the environment on OSX - FIXME?
                P4Server.Update("P4CHARSET", "utf16le-bom");

                using (P4Server target = new P4Server(server, user, pass, ws_client))
                {
                    string gotCharset = P4Server.Get("P4CHARSET");
                    Assert.AreEqual(gotCharset, "utf16le-bom");

                    //target.SetCharacterSet("utf8", "utf8-bom");
                    Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");

                    Assert.AreEqual("admin", target.User);
                    Assert.AreEqual("admin_space", target.Client);
                    Assert.AreEqual(configuration.ServerPort, target.Port);

                    string localPath = Path.Combine(adminSpace, "MyCode", "Пюп.txt");
                    string depotPath = "//depot/MyCode/Пюп-utf8.txt";

                    Assert.IsTrue(target.RunCommand("revert", 7, true, new string[] { depotPath }, 1));

                    Assert.IsTrue(target.RunCommand("print", 7, true, new string[] { depotPath }, 1));
                    string fileContents = target.GetTextResults(7);

                    Assert.IsTrue(target.RunCommand("sync", 7, true, new string[] { "-f", depotPath }, 2));
                }

            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
                Environment.SetEnvironmentVariable("P4CHARSET", origCharset);
                P4Server.Update("P4CHARSET", "");
                p4d?.Dispose();
            }
        }

        // helper
        bool FindInLog(string logFile, string msg, int count = -1)
        {
            string line;
            int foundCount = 0;
            // file must be explicitly closed or p4d will stop logging to it
            System.IO.StreamReader file = new System.IO.StreamReader(logFile);
            try
            {
                while ((line = file.ReadLine()) != null)
                {
                    if (line == msg)
                    {
                        if (count < 0) // find any instance
                            return true;
                        else
                            foundCount++;
                    }
                }
                return (foundCount == count);
            }
            finally
            {
                file.Close();
            }
        }

        /// <summary>
        /// A test for SetProtocol
        /// </summary>
        [TestMethod()]
        public void SetProtocolTest()
        {
            string server = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;
            var cptype = Utilities.CheckpointType.A;
            try
            {
                p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                Assert.IsNotNull(p4d, "Setup Failure");

                var serverRoot = Utilities.TestServerRoot(TestDir, cptype);

                // turn on rpc debugging
                using (P4Server target = new P4Server(server, user, pass, ws_client))
                {
                    Assert.IsTrue(target.RunCommand("configure", 0, true, new string[] { "set", "rpc=3" }, 2));
                }

                // reconnect
                using (P4Server target = new P4Server(server, user, pass, ws_client))
                {
                    // due to the way the .net api is implemented, we must disconnect first
                    // careful setting breakpoints through here, the auto-disconnect could
                    // cause a reconnect and re-send of protocols, which throws off the counts
                    target.Disconnect();
                    // we're not technically connected now, so call SetProtocol
                    target.SetProtocol("pizza", "3");
                    // now run a command, we should get an Rpc message in the log with our custom protocol
                    Assert.IsTrue(target.RunCommand("info", 0, true, null, 0));
                    // find RpcSendBuffer pizza = 3 in the log

                    string logPath = Path.Combine(serverRoot, "p4d.log");
                    Assert.IsTrue(FindInLog(logPath, "RpcRecvBuffer pizza = 3"));

                    // negative test: SetProtocol and nothing noted in the log
                    // (note that the log also does not show *pizza* after this request)
                    target.SetProtocol("calzone", "4");
                    Assert.IsTrue(target.RunCommand("info", 0, true, null, 0));
                    Assert.IsFalse(FindInLog(logPath, "RpcRecvBuffer pizza = 3", 2));
                    Assert.IsFalse(FindInLog(logPath, "RpcRecvBuffer calzone = 4"));

                    // positive test: reconnect and it should be there again (as well as 2 pizzas)
                    target.Disconnect();
                    Assert.IsTrue(target.RunCommand("info", 0, true, null, 0));
                    Assert.IsTrue(FindInLog(logPath, "RpcRecvBuffer pizza = 3", 2));
                    Assert.IsTrue(FindInLog(logPath, "RpcRecvBuffer calzone = 4"));
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
                p4d?.Dispose();
            }
        }
    }
}
