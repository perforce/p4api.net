using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Perforce.P4;
using System.IO;
using NLog;
using System.Diagnostics;

namespace p4api.net.unit.test
{
    [TestClass]
    public class RshTest
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private UnitTestConfiguration configuration;
        private string TestDir = "";

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

        [TestMethod]
        public void RshConnectionTest()
        {
            var cptype = Utilities.CheckpointType.A;

            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;
            Repository rep = null;
            try
            {
                p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                Assert.IsNotNull(p4d, "Setup Failure");

                Server server = new Server(new ServerAddress(configuration.ServerPort));
                rep = new Repository(server);

            using (Connection con = rep.Connection)
            {
                con.UserName = user;
                con.Client = new Client();
                con.Client.Name = ws_client;
                bool connected = con.Connect(null);
                Assert.IsTrue(connected);
                Assert.AreEqual(con.Status, ConnectionStatus.Connected);
                uint cmdID = 7;
                string[] args = new string[] { "stop" };
                Assert.IsTrue(con.getP4Server().RunCommand("admin", cmdID, false, args, args.Length));
                logger.Debug("Stopped launched server");
            }

                string uri = Utilities.TestRshServerPort(TestDir, cptype);
                Server server1 = new Server(new ServerAddress(uri));
                rep = new Repository(server1);
            logger.Debug("Created new server");
                
                Utilities.SetClientRoot(rep, TestDir, cptype, ws_client);
            
                using (Connection con = rep.Connection)
                {
                    con.UserName = user;
                    con.Client = new Client();
                    con.Client.Name = ws_client;

                    logger.Debug("About to connect");
                    Assert.AreEqual(con.Status, ConnectionStatus.Disconnected);
                    Assert.IsTrue(con.Connect(null));
                    Assert.AreEqual(con.Status, ConnectionStatus.Connected);
                    logger.Debug("Connected");


                    FileSpec fs = new FileSpec(new DepotPath("//depot/MyCode/ReadMe.txt"), null);
                    rep.Connection.Client.EditFiles(null, fs);
                    logger.Debug("File edited");
                }
            } 
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
                p4d?.Dispose();
                rep?.Dispose();
            }
        }
    }
}
