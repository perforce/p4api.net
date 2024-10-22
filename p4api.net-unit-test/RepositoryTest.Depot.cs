using Perforce.P4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace p4api.net.unit.test
{

    /// <summary>
    ///This is a test class for RepositoryTest and is intended
    ///to contain RepositoryTest Unit Tests
    ///</summary>
    public partial class RepositoryTest
    {
        private static string readonlyClientSpec =
@"# A Perforce Client Specification.
#
#  Client:      The client name.
#  Update:      The date this specification was last modified.
#  Access:      The date this client was last used in any way.
#  Owner:       The user who created this client.
#  Host:        If set, restricts access to the named host.
#  Description: A short description of the client (optional).
#  Root:        The base directory of the client workspace.
#  AltRoots:    Up to two alternate client workspace roots.
#  Options:     Client options:
#                      [no]allwrite [no]clobber [no]compress
#                      [un]locked [no]modtime [no]rmdir
#  SubmitOptions:
#                      submitunchanged/submitunchanged+reopen
#                      revertunchanged/revertunchanged+reopen
#                      leaveunchanged/leaveunchanged+reopen
#  LineEnd:     Text file line endings on client: local/unix/mac/win/share.
#  View:        Lines to map depot files into the client workspace.
#
# Use 'p4 help client' to see more about client views and options.

Client:	XP1_usr

Update:	2010/11/29 15:30:32

Access:	2010/11/23 08:26:17

Owner:	XP1

Host:	XPPro001

Description:
	Created by xp1.

Root:	c:\XP1_dev

AltRoots:
	c:\XP1_dev_A1
	c:\XP1_dev_A2

Options:	noallwrite noclobber nocompress unlocked nomodtime normdir noaltsync

SubmitOptions:	submitunchanged

LineEnd:	local

Type:   readonly

View:
	//depot/dev/xp1/... //XP1_usr/depot/dev/xp1/...
";

        private static string partitionedClientSpec =
@"# A Perforce Client Specification.
#
#  Client:      The client name.
#  Update:      The date this specification was last modified.
#  Access:      The date this client was last used in any way.
#  Owner:       The user who created this client.
#  Host:        If set, restricts access to the named host.
#  Description: A short description of the client (optional).
#  Root:        The base directory of the client workspace.
#  AltRoots:    Up to two alternate client workspace roots.
#  Options:     Client options:
#                      [no]allwrite [no]clobber [no]compress
#                      [un]locked [no]modtime [no]rmdir
#  SubmitOptions:
#                      submitunchanged/submitunchanged+reopen
#                      revertunchanged/revertunchanged+reopen
#                      leaveunchanged/leaveunchanged+reopen
#  LineEnd:     Text file line endings on client: local/unix/mac/win/share.
#  View:        Lines to map depot files into the client workspace.
#
# Use 'p4 help client' to see more about client views and options.

Client:	XP1_usr

Update:	2010/11/29 15:30:32

Access:	2010/11/23 08:26:17

Owner:	XP1

Host:	XPPro001

Description:
	Created by xp1.

Root:	c:\XP1_dev

AltRoots:
	c:\XP1_dev_A1
	c:\XP1_dev_A2

Options:	noallwrite noclobber nocompress unlocked nomodtime normdir noaltsync

SubmitOptions:	submitunchanged

LineEnd:	local

Type:   partitioned

View:
	//depot/dev/xp1/... //XP1_usr/depot/dev/xp1/...
";

        /// <summary>
        ///A test for CreateDepot
        ///</summary>
        [TestMethod()]
        public void CreateDepotTest()
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

                        Depot d = new Depot();
                        d.Id = "NewDepot";
                        d.Description = "created by perforce";
                        d.Owner = "admin";
                        d.Type = DepotType.Stream;
                        //TODO StreamDepth
                        d.Map = "NewDepot/...";
                        d.StreamDepth = "//NewDepot/1";

                        Depot newDepot = rep.CreateDepot(d, null);
                        Assert.IsNotNull(newDepot);
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
        ///A test for CreateDepot with an invalid path
        ///</summary>
        [TestMethod()]
        public void CreateInvalidPathDepotTest()
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

                        Depot d = new Depot();
                        d.Id = "NewDepot";
                        d.Description = "created by perforce";
                        d.Owner = "admin";
                        d.Type = DepotType.Stream;
                        d.Map = "123";

                        try
                        {
                            Depot newDepot = rep.CreateDepot(d, null);
                        }
                        catch (P4Exception e)
                        {
                            Assert.AreEqual(822153266, e.ErrorCode, "Error in depot specification. "
                            + "Map entry '123' must have only 1 wildcard which must be a trailing '/...' or '\\...'.");
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
        }


        /// <summary>
        ///A test for CreateDepot attempting to add another spec depot
        ///</summary>
        [TestMethod()]
        public void CreateExtraSpecDepotTest()
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

                        Depot d = new Depot();
                        d.Id = "NewDepot";
                        d.Description = "created by perforce";
                        d.Owner = "admin";
                        d.Type = DepotType.Spec;//.Local;
                        d.Map = "123";
                        d.StreamDepth = "//NewDepot/1";

                        try
                        {
                            Depot newDepot = rep.CreateDepot(d, null);
                        }
                        catch (P4Exception e)
                        {
                            Assert.AreEqual(839064437, e.ErrorCode, e.Message);
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
        }


        /// <summary>
        ///A test for DeleteDepot
        ///</summary>
        [TestMethod()]
        public void DeleteDepotTest()
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

                        Depot d = new Depot();
                        d.Id = "NewDepot";
                        d.Description = "created by perforce";
                        d.Owner = "admin";
                        d.Type = DepotType.Local;
                        d.Map = "NewDepot/...";
                        d.StreamDepth = "//NewDepot/1";

                        Depot newDepot = rep.CreateDepot(d, null);

                        Assert.IsNotNull(newDepot);

                        rep.DeleteDepot(newDepot, null);

                        IList<Depot> dlist = rep.GetDepots();

                        Assert.IsFalse(dlist.Contains(newDepot));

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
        ///A test for DeleteDepot for a depot with files in it
        ///</summary>
        [TestMethod()]
        public void DeleteDepotWithFilesTest()
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

                        String depotName = "flow";

                        Depot depot = rep.GetDepot(depotName, null);

                        Assert.IsNotNull(depot);
                        try
                        {
                            rep.DeleteDepot(depot, null);
                        }
                        catch (P4Exception e)
                        {
                            Assert.AreEqual(822417475, e.ErrorCode,
                                "Depot flow isn't empty. To delete a depot, all file revisions must be removed "
                                + "and all lazy copy references from other depots must be severed. Use 'p4 obliterate'"
                                + "or 'p4 snap' to break file linkages from other depots, then clear this depot with "
                                + "'p4 obliteror 'p4 snap' to break file linkages from other depots, then clear this depot "
                                + "with 'p4 obliterate', then retry the deletion.");
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
        }


        /// <summary>
        ///A test for GetDepot
        ///</summary>
        [TestMethod()]
        public void GetDepotTest()
        {
            string uri = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            string targetDepot = "flow";

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

                        Depot d = rep.GetDepot(targetDepot, null);

                        Assert.IsNotNull(d);
                        Assert.AreEqual(targetDepot, d.Id);
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
        ///A test for GetDepots
        ///</summary>
        [TestMethod()]
        public void GetDepotsTest()
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

                        IList<Depot> dlist = rep.GetDepots();

                        Assert.IsTrue(dlist[0].Id.Equals("depot"));
                        Assert.IsTrue(dlist[1].Map.Equals("flow/..."));
                        Assert.IsTrue(dlist[2].Type.Equals(DepotType.Stream));

                        string expected = Utilities.FixLineFeeds(
                            "Depot For 'Rocket' project\n\nEVENTS/new_stream_events/events0100_create_depots.pl-Event_001-perforce-CREATE_DEPOTS-Creating depots...\n");
                        string actual = Utilities.FixLineFeeds(dlist[3].Description);
                        Assert.AreEqual(expected, actual);

                        //DateTime modified = new DateTime(2010, 10, 19, 10, 40, 3);
                        //Assert.AreEqual(modified, dlist[0].Modified);
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
        ///A test for CheckDepotTypesA
        ///</summary>
        [TestMethod()]
        public void CheckDepotTypesTestA()
        {
            CheckDepotTypesTest(Utilities.CheckpointType.A);
        }

        /// <summary>
        ///A test for CheckDepotTypesU
        ///</summary>
        [TestMethod()]
        public void CheckDepotTypesTestU()
        {
            CheckDepotTypesTest(Utilities.CheckpointType.U);
        }

        /// <summary>
        ///A test for CheckDepotTypes
        ///</summary>
        public void CheckDepotTypesTest(Utilities.CheckpointType cptype)
        {
            string uri = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

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

                    Depot d = new Depot();
                    d.Id = "LocalDepot";
                    d.Description = "created by perforce";
                    d.Owner = "admin";
                    d.Type = DepotType.Local;
                    d.Map = "LocalDepot/...";
                    d.StreamDepth = "//StreamDepot/1";

                    rep.CreateDepot(d);

                    d = new Depot();
                    d.Id = "RemoteDepot";
                    d.Description = "created by perforce";
                    d.Owner = "admin";
                    d.Type = DepotType.Remote;
                    d.Map = "RemoteDepot/...";
                    d.StreamDepth = "//StreamDepot/1";
                    d.Address = new ServerAddress(con.getP4Server().Port);

                    rep.CreateDepot(d);

                    d = new Depot();
                    d.Id = "StreamDepot";
                    d.Description = "created by perforce";
                    d.Owner = "admin";
                    d.Type = DepotType.Stream;
                    d.Map = "StreamDepot/...";
                    d.StreamDepth = "//StreamDepot/1";

                    rep.CreateDepot(d);

                    d = new Depot();
                    d.Id = "ArchiveDepot";
                    d.Description = "created by perforce";
                    d.Owner = "admin";
                    d.Type = DepotType.Archive;
                    d.Map = "ArchiveDepot/...";
                    d.StreamDepth = "//StreamDepot/1";

                    rep.CreateDepot(d);

                    d = new Depot();
                    d.Id = "UnloadDepot";
                    d.Description = "created by perforce";
                    d.Owner = "admin";
                    d.Type = DepotType.Unload;
                    d.Map = "UnloadDepot/...";
                    d.StreamDepth = "//StreamDepot/1";

                    rep.CreateDepot(d);

                    d = new Depot();
                    d.Id = "TangentDepot";
                    d.Description = "created by perforce";
                    d.Owner = "admin";
                    d.Type = DepotType.Tangent;
                    d.Map = "TangentDepot/...";
                    d.StreamDepth = "//StreamDepot/1";

                    rep.CreateDepot(d);

                    d = new Depot();
                    d.Id = "GraphDepot";
                    d.Description = "created by perforce";
                    d.Owner = "admin";
                    d.Type = DepotType.Graph;
                    d.Map = "GraphDepot/...";
                    d.StreamDepth = "//StreamDepot/1";

                    rep.CreateDepot(d);

                    IList<Depot> dlist = rep.GetDepots();

                    Assert.IsTrue(dlist[0].Type.Equals(DepotType.Archive));
                    Assert.IsTrue(dlist[1].Type.Equals(DepotType.Local));
                    Assert.IsTrue(dlist[2].Type.Equals(DepotType.Stream));
                    Assert.IsTrue(dlist[3].Type.Equals(DepotType.Graph));
                    Assert.IsTrue(dlist[4].Type.Equals(DepotType.Local));
                    Assert.IsTrue(dlist[5].Type.Equals(DepotType.Stream));
                    Assert.IsTrue(dlist[6].Type.Equals(DepotType.Remote));
                    Assert.IsTrue(dlist[7].Type.Equals(DepotType.Stream));
                    Assert.IsTrue(dlist[8].Type.Equals(DepotType.Spec));
                    Assert.IsTrue(dlist[9].Type.Equals(DepotType.Stream));
                    Assert.IsTrue(dlist[10].Type.Equals(DepotType.Tangent));
                    Assert.IsTrue(dlist[11].Type.Equals(DepotType.Stream));
                    Assert.IsTrue(dlist[12].Type.Equals(DepotType.Unload));

                    Options opts = new Options();
                    opts["-t"] = "extension";
                    dlist = rep.GetDepots(opts);

                    Assert.IsTrue(dlist[0].Type.Equals(DepotType.Extension));
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
        [DataRow("admin_space", "admin_space_renamed", "ValidClientNames", DisplayName = "Rename Client/Workspace - Valid Client Names")]
        [DataRow("admin_space", "admin_space2", "ToClientNameExists", DisplayName = "Rename Client/Workspace - To Client Name Exists")]
        [DataRow("admin_space_old", "admin_space2", "InvliadFromClient", DisplayName = "Rename Client/Workspace - From Client Name Doesn't Exists")]
        [DataRow("admin_space", "admin_space", "RenameToSelf", DisplayName = "Rename Client/Workspace - From and To Client names are exact same")]
        public void RenameClientTest(string oldClientName, string newClientName, string testcaseScenario)
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
                    p4d = Utilities.DeployP4TestServer(TestDir, 7, cptype);
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

                        string commandResult = string.Empty;

                        try
                        {
                            commandResult = rep.RenameClient(oldClientName, newClientName);
                        }
                        catch (Exception ex)
                        {
                            commandResult = ex.Message.Trim();
                        }

                        switch (testcaseScenario)
                        {
                            case "ValidClientNames":
                                Assert.AreEqual(string.Format("Client {0} renamed to {1}.", oldClientName, newClientName), commandResult);
                                Client newClient = rep.GetClient(newClientName);
                                Assert.IsNotNull(newClient);
                                Assert.IsTrue(newClient.Name.Equals(newClientName));
                                break;
                            case "ToClientNameExists":
                                Assert.AreEqual(string.Format("Client {0} already exists.", newClientName), commandResult);
                                break;
                            case "InvliadFromClient":
                                Assert.AreEqual(string.Format("Client {0} doesn't exist.", oldClientName), commandResult);
                                Client oldClient = rep.GetClient(oldClientName);
                                break;
                            case "RenameToSelf":
                                Assert.AreEqual(string.Format("Client {0} cannot be renamed to itself.", oldClientName), commandResult);
                                break;

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
        }

        [TestMethod()]
        [DataRow("readonly", DisplayName = "Rename readonly client")]
        [DataRow("partitioned", DisplayName = "Rename partitioned client")]
        public void RenameReadOnlyPartitionedClientTest(string inputClientType)
        {
            string uri = configuration.ServerPort;
            string pass = string.Empty;

            for (int i = 0; i < 1; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                Process p4d = null;
                Repository rep = null;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, 7, cptype);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    Server server = new Server(new ServerAddress(uri));
                    rep = new Repository(server);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = "admin";

                        bool connected = con.Connect(null);


                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        string[] args = { "set", "client.readonly.dir=part-db-have" };

                        bool result = P4Configure(con, args);

                        con.getP4Server().Reconnect();

                        Client client = new Client();

                        client.Parse(readonlyClientSpec);
                        ClientType clientType;

                        Enum.TryParse(inputClientType, out clientType);

                        Options options = new Options
                        {
                            ["-T"] = inputClientType
                        };

                        Client newGuy = rep.CreateClient(client, options);

                        con.Client = newGuy;

                        string commandResult = string.Empty;

                        try
                        {
                            commandResult = rep.RenameClient(newGuy.Name, "client_renamed");
                        }
                        catch (Exception ex)
                        {
                            commandResult = ex.Message.Trim();
                        }

                        Assert.AreEqual("Partitioned or readonly clients cannot be renamed.", commandResult);

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

        [TestMethod()]
        public void RenameClientWithServerConfigurableClientTest()
        {
            string uri = configuration.ServerPort;
            string pass = string.Empty;

            for (int i = 0; i < 1; i++) // run once for ascii, once for unicode
            {
                var cptype = (Utilities.CheckpointType)i;
                Process p4d = null;
                Repository rep = null;
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, 7, cptype);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    Server server = new Server(new ServerAddress(uri));
                    rep = new Repository(server);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = "admin";

                        bool connected = con.Connect(null);

                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        string[] args = { "set", "run.renameclient.allow=0"};

                        bool result = P4Configure(con, args);

                        con.getP4Server().Reconnect();

                        string commandResult = string.Empty;

                        try
                        {
                            commandResult = rep.RenameClient("admin_space", "client_renamed");
                        }
                        catch (Exception ex)
                        {
                            commandResult = ex.Message.Trim();
                        }

                        Assert.AreEqual("Client renames are not allowed.", commandResult);

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

        private bool P4Configure(Connection con, string[] args)
        {
            string oldUser = con.UserName;
            con.UserName = "admin";
            con.Login("");

            using (var cmd = new P4Command(con, "configure", true, args))
            {
                var cmdr = cmd.Run(new Options());
                con.UserName = oldUser;
                return cmdr.Success;
            }
        }
    }
}
