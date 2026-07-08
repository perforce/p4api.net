using Perforce.P4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;

namespace p4api.net.unit.test
{
    /// <summary>
    ///This is a test class for RepositoryTest and is intended
    ///to contain RepositoryTest Unit Tests
    ///</summary>
    public partial class RepositoryTest
    {
        /// <summary>
        ///A test for CreateBranchSpec
        ///</summary>
        [TestMethod()]
        public void CreateBranchSpecTest()
        {
            string uri = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                Process p4d = null;
                Repository rep = null;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, 8, cptype);
                    Assert.IsNotNull(p4d, "Setup Failure");

                Server server = new Server(new ServerAddress(uri));
                    rep = new Repository(server);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = user;
                        con.Client = new Client();
                        con.Client.Name = ws_client;

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        BranchSpec b = new BranchSpec();
                        b.Id = "newDepotBranch";
                        b.Owner = "admin";
                        b.Description = "created by perforce";
#pragma warning disable 618
                        b.Options = "locked";
#pragma warning restore 618
                        b.ViewMap = new ViewMap();
                        string v0 = "//depot/main/... //depot/rel1/...";
                        string v1 = "//depot/main/... //depot/rel2/...";
                        string v2 = "//depot/dev/... //depot/main/...";
                        b.ViewMap.Add(v0);
                        b.ViewMap.Add(v1);
                        b.ViewMap.Add(v2);

                        BranchSpec newBranchSpec = rep.CreateBranchSpec(b, null);

                        Assert.IsNotNull(newBranchSpec);
                        Assert.AreEqual("newDepotBranch", newBranchSpec.Id);


                        string v3 = "\"//depot/main/a file with spaces.txt\" \"//depot/rel1/a file with spaces.txt\"";
                        newBranchSpec.ViewMap.Add(v3);
                        newBranchSpec = rep.UpdateBranchSpec(newBranchSpec);
                        Assert.IsNotNull(newBranchSpec);
                        Assert.AreEqual(newBranchSpec.ViewMap.Count, 4);
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

        /// <summary>
        /// A test for DeleteStream
        ///</summary>
        [TestMethod()]
        public void DeleteBranchSpecTest()
        {
            string uri = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                Process p4d = null;
                Repository rep = null;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, 8, cptype);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    Server server = new Server(new ServerAddress(uri));
                    rep = new Repository(server);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = user;
                        con.Client = new Client();
                        con.Client.Name = ws_client;

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						IList<BranchSpec> blist = rep.GetBranchSpecs(null);

						Assert.IsNotNull(blist);

                        BranchSpec deleteTarget = new BranchSpec();
                        deleteTarget.Id = "MyCode->MyCode2";
                        rep.DeleteBranchSpec (deleteTarget, null);

						blist = rep.GetBranchSpecs(null);

						Assert.IsNull(blist);

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

        /// <summary>
        /// A test for LockBranchSpec
        ///</summary>
        [TestMethod()]
        public void LockBranchSpecTest()
        {
            string uri = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                Process p4d = null;
                Repository rep = null;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, 8, cptype);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    Server server = new Server(new ServerAddress(uri));
                    rep = new Repository(server);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = user;
                        con.Client = new Client();
                        con.Client.Name = ws_client;

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        BranchSpec b = rep.GetBranchSpec("MyCode->MyCode2");
                        b.Locked = true;
                        rep.UpdateBranchSpec(b);
                        BranchSpec b2 = rep.GetBranchSpec("MyCode->MyCode2");
                        Assert.IsTrue(b2.Locked);
                        b2.Locked = false;
                        rep.UpdateBranchSpec(b2);
                        BranchSpec b3 = rep.GetBranchSpec("MyCode->MyCode2");
                        Assert.IsFalse(b3.Locked);

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

        /// <summary>
        ///A test for GetBranchSpec
        ///</summary>
        [TestMethod()]
        public void GetBranchSpecTest()
        {
            string uri = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                Process p4d = null;
                Repository rep = null;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, 8, cptype);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    Server server = new Server(new ServerAddress(uri));
                    rep = new Repository(server);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = user;
                        con.Client = new Client();
                        con.Client.Name = ws_client;

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        string targetBranchSpec = "MyCode->MyCode2";

                        BranchSpec b = rep.GetBranchSpec(targetBranchSpec);

                        Assert.IsNotNull(b);

                        Assert.AreEqual(targetBranchSpec, b.Id);

						BranchSpec b1 = rep.GetBranchSpec("StreamBranchMapWithD3", "//flow/D3", null, null);

						Assert.IsNotNull(b1);

						Assert.IsTrue(b1.ViewMap[0].Left.Path.Contains("flow/D3/..."));

						BranchSpec b2 = rep.GetBranchSpec("StreamBranchMapWithD3OtherParent", "//flow/D3", "//flow/D2", null);

						Assert.IsNotNull(b2);

						Assert.AreEqual(b2.ViewMap.Count, 6);
						Assert.IsTrue(b2.ViewMap[0].Type.Equals(MapType.Exclude));
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

        /// <summary>
        ///A test for GetBranchSpecs
        ///</summary>
		[TestMethod()]
		public void GetBranchSpecsTest()
		{
            string uri = configuration.ServerPort;
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
                var cptype = (Utilities.CheckpointType)i;
                Process p4d = null;
                Repository rep = null;
				try
				{
                    p4d = Utilities.DeployP4TestServer(TestDir, 8, cptype);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    Server server = new Server(new ServerAddress(uri));
                    rep = new Repository(server);

					using (Connection con = rep.Connection)
					{
						con.UserName = user;
						con.Client = new Client();
						con.Client.Name = ws_client;

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        Options bFlags = new Options(BranchSpecsCmdFlags.None, "admin", "", -1);

                        IList<BranchSpec> b = rep.GetBranchSpecs(bFlags);


						Assert.IsNotNull(b);
						Assert.AreEqual(1, b.Count);
                        Assert.AreEqual("Created by admin.", b[0].Description.Trim());

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

        /// <summary>
        /// AC-1: GetBranchSpecs with --user-case-insensitive returns branches owned
        /// by "alice" when searched with "Alice" on a case-sensitive server.
        /// </summary>
        [TestMethod()]
        public void GetBranchSpecsUserCaseInsensitiveTest()
        {
            string uri = configuration.ServerPort;
            string user = "admin";
            string ws_client = "admin_space";

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                Process p4d = null;
                Repository rep = null;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, 8, cptype);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    Server server = new Server(new ServerAddress(uri));
                    rep = new Repository(server);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = user;
                        con.Client = new Client();
                        con.Client.Name = ws_client;

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);
                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        // Create a branch spec owned by "alice" (all lowercase)
                        BranchSpec b = new BranchSpec();
                        b.Id = "alice-branch";
                        b.Owner = "alice";
                        b.Description = "branch owned by alice";
                        b.ViewMap = new ViewMap();
                        b.ViewMap.Add("//depot/main/... //depot/alice/...");
                        rep.CreateBranchSpec(b, null);

                        // Search with "Alice" (uppercase A) and the UserCaseInsensitive
                        // flag – the server is case-sensitive but the flag makes the user
                        // filter case-insensitive, so alice's branch must be returned.
                        Options bFlags = new Options(BranchSpecsCmdFlags.UserCaseInsensitive, "Alice", "", -1);
                        IList<BranchSpec> branches = rep.GetBranchSpecs(bFlags);

                        Assert.IsNotNull(branches);
                        Assert.AreEqual(1, branches.Count);
                        Assert.AreEqual("alice-branch", branches[0].Id);
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

        /// <summary>
        /// AC-2: GetBranchSpecs with --user-case-insensitive and a wildcard user
        /// pattern returns branches from all case-insensitively matched owners.
        /// </summary>
        [TestMethod()]
        public void GetBranchSpecsUserCaseInsensitiveWildcardTest()
        {
            string uri = configuration.ServerPort;
            string user = "admin";
            string ws_client = "admin_space";

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                Process p4d = null;
                Repository rep = null;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, 8, cptype);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    Server server = new Server(new ServerAddress(uri));
                    rep = new Repository(server);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = user;
                        con.Client = new Client();
                        con.Client.Name = ws_client;

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);
                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        // Create branch specs owned by two different users
                        BranchSpec b1 = new BranchSpec();
                        b1.Id = "alice-main-branch";
                        b1.Owner = "alice";
                        b1.Description = "branch owned by alice";
                        b1.ViewMap = new ViewMap();
                        b1.ViewMap.Add("//depot/main/... //depot/alice/...");
                        rep.CreateBranchSpec(b1, null);

                        BranchSpec b2 = new BranchSpec();
                        b2.Id = "alex-main-branch";
                        b2.Owner = "alex";
                        b2.Description = "branch owned by alex";
                        b2.ViewMap = new ViewMap();
                        b2.ViewMap.Add("//depot/main/... //depot/alex/...");
                        rep.CreateBranchSpec(b2, null);

                        // Wildcard "Al*" with UserCaseInsensitive should match both
                        // "alice" and "alex" (both start with 'al', case-insensitive).
                        // The pre-existing "admin" branch does not start with 'al' and
                        // should not be included.
                        Options bFlags = new Options(BranchSpecsCmdFlags.UserCaseInsensitive, "Al*", "", -1);
                        IList<BranchSpec> branches = rep.GetBranchSpecs(bFlags);

                        Assert.IsNotNull(branches);
                        Assert.AreEqual(2, branches.Count);
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

        /// <summary>
        /// AC-3: GetBranchSpecs WITHOUT --user-case-insensitive returns no results
        /// when the supplied user value differs in case from the actual branch owner
        /// on a case-sensitive server.
        /// </summary>
        [TestMethod()]
        public void GetBranchSpecsCaseSensitiveNoMatchTest()
        {
            string uri = configuration.ServerPort;
            string user = "admin";
            string ws_client = "admin_space";

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                Process p4d = null;
                Repository rep = null;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, 8, cptype);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    Server server = new Server(new ServerAddress(uri));
                    rep = new Repository(server);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = user;
                        con.Client = new Client();
                        con.Client.Name = ws_client;

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);
                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        // Create a branch spec owned by "alice" (all lowercase)
                        BranchSpec b = new BranchSpec();
                        b.Id = "alice-branch";
                        b.Owner = "alice";
                        b.Description = "branch owned by alice";
                        b.ViewMap = new ViewMap();
                        b.ViewMap.Add("//depot/main/... //depot/alice/...");
                        rep.CreateBranchSpec(b, null);

                        // Search for a user that does not own any branch spec, WITHOUT the
                        // UserCaseInsensitive flag.  The -u filter must not return the branch
                        // owned by "alice", regardless of the server's case-sensitivity setting.
                        Options bFlags = new Options(BranchSpecsCmdFlags.None, "zzz_no_such_user", "", -1);
                        IList<BranchSpec> branches = rep.GetBranchSpecs(bFlags);

                        Assert.IsNull(branches);
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
}
