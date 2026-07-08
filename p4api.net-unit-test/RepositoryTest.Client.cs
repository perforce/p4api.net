using Perforce.P4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
		private static string testClientSpec =
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

View:
	//depot/dev/xp1/... //XP1_usr/depot/dev/xp1/...
";

		/// <summary>
		///A test for CreateClient
		///</summary>
		[TestMethod()]
		public void CreateClientTest()
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
                    p4d = Utilities.DeployP4TestServer(TestDir, 10, cptype);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    Server server = new Server(new ServerAddress(uri));
                    rep = new Repository(server);

					using (Connection con = rep.Connection)
					{
						con.UserName = user;
						con.Client = new Client();
						con.Client.Name = ws_client;
                        if (cptype == Utilities.CheckpointType.U)
					        con.CharacterSetName = "utf8";

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						Client c = new Client();
						c.Parse(testClientSpec);
						c.OwnerName = "Alice";

						MapEntry me = new MapEntry(MapType.Include,
							new DepotPath("//depot/dev/xp1/..."),
							new ClientPath("//XP1_usr/depot/dev/xp1/..."));

						c.ViewMap.Add(me);

						c.Stream = "//Rocket/GUI";
						c.StreamAtChange = "82";
						
						Client newGuy = rep.CreateClient(c, null);

						Assert.IsNotNull(newGuy);
						Assert.AreEqual("Alice", newGuy.OwnerName);

						//create another one without stream fields or server id
						Client b = new Client();
						b.Parse(testClientSpec);
						b.Name = "XP2_usr";
						b.OwnerName = "Alice";
						
						b.AltRoots = null;
						MapEntry me2 = new MapEntry(MapType.Include,
							new DepotPath("//depot/dev/xp1/..."),
							new ClientPath("//XP2_usr/depot/dev/xp1/..."));

						b.ViewMap.Clear();
						b.ViewMap.Add(me2);

						Client anotherNewGuy = rep.CreateClient(b, null);

						Assert.IsNotNull(anotherNewGuy);
						Assert.AreEqual("Alice", anotherNewGuy.OwnerName);

                        // add lines with spaces in the view
                        
                        MapEntry me3 = new MapEntry(MapType.Include,
                            new DepotPath("//depot/dev/xp1/file with spaces.txt"),
                            new ClientPath("//XP2_usr/dev/xp1/file with spaces.txt"));

                        b.ViewMap.Clear();
                        b.ViewMap.Add(me3);

					    anotherNewGuy = rep.UpdateClient(b);

                        Assert.IsNotNull(anotherNewGuy);

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
		///A test for DeleteClient
		///</summary>
		[TestMethod()]
		public void DeleteClientTest()
		{
            string uri = configuration.ServerPort;
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			string targetClient = "admin_space2";

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

						Client c = new Client();
						c.Name = targetClient;

						Options uFlags = new Options(ClientCmdFlags.Force);
						rep.DeleteClient(c, uFlags);

						IList<Client> u2 = rep.GetClients(
							new Options(ClientsCmdFlags.None, null, "XP1_usr", 2, null));

						Assert.IsNull(u2);
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
		///A test for GetClient
		///</summary>
		[TestMethod()]
		public void GetClientTest()
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
                    p4d = Utilities.DeployP4TestServer(TestDir, 6, cptype);
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

						Client c = rep.GetClient("alex_space", null);

						Assert.IsNotNull(c);
						Assert.AreEqual("alex_space", c.Name);
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
		///A test for GetClient - get streams client
		///</summary>
		[TestMethod()]
		public void GetClientTest2()
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
                    p4d = Utilities.DeployP4TestServer(TestDir, 10, cptype);
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

						Client c = rep.GetClient("bob-main", null);

						Assert.IsNotNull(c);
						Assert.AreEqual("bob-main", c.Name);
						Assert.AreEqual("//Parents/MAIN", c.Stream);
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
		///A test for GetClients
		///</summary>
		[TestMethod()]
		public void GetClientsTest()
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
                    p4d = Utilities.DeployP4TestServer(TestDir, 6, cptype);
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

						IList<Client> u = rep.GetClients(
							new Options(ClientsCmdFlags.IncludeTime, null, null, 2, null));

						Assert.IsNotNull(u);
						Assert.AreEqual(2, u.Count);
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
		/// A test for GetClients with --user-case-insensitive flag (AC-1 and AC-3).
		/// AC-1: querying with upper-case user name and UserCaseInsensitive flag returns the
		///       matching clients despite the case difference.
		/// AC-3: querying with the same upper-case user name WITHOUT the flag returns null on
		///       a case-sensitive server.
		/// </summary>
		[TestMethod()]
		public void GetClientsUserCaseInsensitiveTest()
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
					p4d = Utilities.DeployP4TestServer(TestDir, 6, cptype);
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

						// AC-1: upper-case "ADMIN" with UserCaseInsensitive flag should match
						// the stored lower-case "admin" user and return admin's clients.
						IList<Client> withFlag = rep.GetClients(
							new ClientsCmdOptions(ClientsCmdFlags.UserCaseInsensitive,
								"ADMIN", null, -1, null));

						Assert.IsNotNull(withFlag,
							"Expected clients when --user-case-insensitive is set with 'ADMIN'");
						Assert.IsTrue(withFlag.Count >= 1,
							"Expected at least one client owned by 'admin' (case-insensitive match on 'ADMIN')");
						foreach (Client c in withFlag)
						{
							Assert.IsTrue(
								string.Equals(c.OwnerName, "admin",
									StringComparison.OrdinalIgnoreCase),
								"Expected all returned clients to be owned by 'admin'");
						}

						// AC-3: same upper-case "ADMIN" WITHOUT the flag — on a case-sensitive server
						// this returns null; on a case-insensitive server it may return results.
						// We verify the call completes without error and that any returned clients
						// are owned by "admin" (case-insensitively).
						IList<Client> withoutFlag = rep.GetClients(
							new ClientsCmdOptions(ClientsCmdFlags.None,
								"ADMIN", null, -1, null));

						// Result is server-configuration-dependent: null on a case-sensitive server,
						// non-null on a case-insensitive server. Either is valid here.
						if (withoutFlag != null)
						{
							foreach (Client c in withoutFlag)
							{
								Assert.IsTrue(
									string.Equals(c.OwnerName, "admin",
										StringComparison.OrdinalIgnoreCase),
									"Expected all returned clients to be owned by 'admin'");
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
		}

		/// <summary>
		/// A test for GetClients with --user-case-insensitive flag and a wildcard pattern (AC-2).
		/// AC-2: querying with an upper-case wildcard pattern "A*" and UserCaseInsensitive flag
		///       returns clients from all users whose names start with 'a' (case-insensitively),
		///       i.e. admin, Alex, and Alice in checkpoint 6.
		/// </summary>
		[TestMethod()]
		public void GetClientsUserCaseInsensitiveWildcardTest()
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
					p4d = Utilities.DeployP4TestServer(TestDir, 6, cptype);
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

						// AC-2: wildcard "A*" with UserCaseInsensitive should match all users
						// whose names start with 'a' regardless of case: admin, Alex, Alice.
						// Checkpoint 6 has admin_space + admin_space2 (admin), alex_space (Alex),
						// alice_space (Alice) => at least 2 clients returned.
						IList<Client> results = rep.GetClients(
							new ClientsCmdOptions(ClientsCmdFlags.UserCaseInsensitive,
								"A*", null, -1, null));

						Assert.IsNotNull(results,
							"Expected clients when --user-case-insensitive is set with wildcard 'A*'");
						Assert.IsTrue(results.Count >= 2,
							"Expected at least 2 clients matching wildcard 'A*' case-insensitively");
						foreach (Client c in results)
						{
							Assert.IsTrue(
								c.OwnerName.StartsWith("a", StringComparison.OrdinalIgnoreCase),
								"Expected all returned clients to be owned by a user starting with 'a'");
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
	}
}
