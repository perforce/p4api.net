using Perforce.P4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using NLog;
using System.Globalization;

namespace p4api.net.unit.test
{
	/// <summary>
	///This is a test class for RepositoryTest and is intended
	///to contain RepositoryTest Unit Tests
	///</summary>
	public partial class RepositoryTest
	{
		/// <summary>
		///A test for CreateChangelist
		///</summary>
		[TestMethod()]
		public void CreateChangelistTest()
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

						Changelist c = new Changelist();
						c.Description = "New changelist for unit test";

						Options uFlags = new Options(ChangeCmdFlags.IncludeJobs);
						Changelist newGuy = rep.CreateChangelist(c, null);

						Assert.IsNotNull(newGuy);
						Assert.AreNotEqual(-1, newGuy.Id);

						Assert.AreEqual("admin", newGuy.OwnerName);
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
        ///A test for NewChangelist
        ///</summary>
        [TestMethod()]
        public void NewChangelistTest()
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

                        Changelist c = rep.NewChangelist();

                        c.Description = "adding a description";
                        Changelist newOne = rep.SaveChangelist(c, null);

                        Assert.IsNotNull(newOne);

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
		///A test for UpdateChangelist
		///</summary>
		[TestMethod()]
		public void UpdateChangelistTest()
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

						Changelist c = rep.GetChangelist(5);
						c.Description = "new desc\r\nline 2\nline 3";
						rep.UpdateChangelist(c);

						Changelist d = rep.GetChangelist(5);
                        Assert.AreEqual(d.Description, $"new desc{Environment.NewLine}line 2{Environment.NewLine}line 3");
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
        ///A test for UpdateChangelist with another user's pending changelist
        ///</summary>
        [TestMethod()]
        public void UpdateChangelistOtherUserTest()
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
                    p4d = Utilities.DeployP4TestServer(TestDir, 13, cptype);
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
                        int changeID = 120;
                        if (cptype == Utilities.CheckpointType.U)
                        {
                            changeID = 87;
                        }
                        Changelist c = new Changelist();
                        try
                        {
                            c = rep.GetChangelist(changeID);
                            c.Description = "new desc\r\nline 2\nline 3";
                            rep.UpdateChangelist(c);
                        }
                        catch (P4Exception ex)
                        {
                            Assert.AreEqual(838932948, ex.ErrorCode, ("can only be updated by user harold"));
                        }

                        Options opts = new Options();
                        opts["-f"] = null;

                        c = rep.GetChangelist(changeID);
                        c.Description = "new desc\r\nline 2\nline 3";
                        rep.UpdateChangelist(c,opts);

                        Changelist d = rep.GetChangelist(changeID);
                        Assert.AreEqual(d.Description, $"new desc{Environment.NewLine}line 2{Environment.NewLine}line 3");
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
        ///A test for UpdateChangelistWithMultilineDescription
        ///</summary>
        [TestMethod()]
        public void UpdateChangelistWithMultilineDescTest()
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

                        Changelist c = rep.GetChangelist(5);
                        c.Description = "new desc";
                        rep.UpdateChangelist(c);

                        Changelist d = rep.GetChangelist(5);
                        Assert.AreEqual(d.Description, "new desc");
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
        ///A test for SubmitShelvedFromChangelist
        ///</summary>
        [TestMethod()]
        public void SubmitShelvedFromChangelist()
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
                    p4d = Utilities.DeployP4TestServer(TestDir, 13, cptype);
                    Assert.IsNotNull(p4d, "Setup Failure");

                    Server server = new Server(new ServerAddress(uri));
                    rep = new Repository(server);

                    Utilities.SetClientRoot(rep, TestDir, cptype, ws_client, false);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = user;
                        con.Client = new Client();
                        con.Client.Name = ws_client;

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);
                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        Changelist change = new Changelist(5, true);
                        change.initialize(con);

                        // shelve the files in changelist 5
                        con.Client.ShelveFiles(new ShelveFilesCmdOptions(ShelveFilesCmdFlags.None,
                            null,change.Id));

                        // revert the checked out file that was shelved
                        FileSpec file = new FileSpec(new DepotPath("//..."), null, null, null);
                        con.Client.RevertFiles(new RevertCmdOptions(RevertFilesCmdFlags.None, change.Id),
                            file);
                        
                        // submit the shelved file
                        SubmitCmdOptions submitOptions = new 
                            SubmitCmdOptions(Perforce.P4.SubmitFilesCmdFlags.SubmitShelved,
                            5, null, null, null);
                        change.Submit(submitOptions);

                        P4CommandResult last = rep.Connection.LastResults;
                        Assert.IsTrue(last.Success);
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
        ///A test for UpdateSubmittedChangelist
        ///</summary>
        [TestMethod()]
        public void UpdateSubmittedChangelistTest()
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

                        Changelist c = rep.GetChangelist(12);
                        c.Description += "\n\tModified!";
                        rep.UpdateSubmittedChangelist(c,null);

                        Changelist d = rep.GetChangelist(12);
                        Assert.IsTrue(d.Description.Contains("Modified!"));

                        // on the non-unicode server edit the description
                        // of Alex's changelist 8 as an admin
                        if (cptype != Utilities.CheckpointType.U)
                        {
                            c = rep.GetChangelist(8);
                            c.Description += "\n\tModified!";
                            Options opts = new Options();
                            opts["-f"] = null;
                            rep.UpdateSubmittedChangelist(c, opts);

                            d = rep.GetChangelist(8);
                            Assert.IsTrue(d.Description.Contains("Modified!"));
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
		///A test for DeleteChangelist
		///</summary>
		[TestMethod()]
		public void DeleteChangelistTest()
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

						Changelist c = new Changelist();
						c.Description = "New changelist for unit test";

						Changelist newGuy = rep.CreateChangelist(c, null);

						rep.DeleteChangelist(newGuy, null);

						Changelist deadGuy = null;
						try
						{
							deadGuy = rep.GetChangelist(newGuy.Id);
						}
						catch { }

						Assert.IsNull(deadGuy);
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
        ///A test for GetChangelistjob080718
        ///</summary>
        [TestMethod()]
        public void GetChangelistTestjob080718()
        {
            string uri = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;
            Repository rep = null;
            var cptype = Utilities.CheckpointType.A;
                try
                {
                p4d = Utilities.DeployP4TestServer(TestDir, 17, cptype, TestContext.TestName);
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

                    Changelist c = rep.GetChangelist(156, null);

                    IList<FileSpec> fs = new List<FileSpec>();
                    fs.Add(c.Files[0]);

                    IList<FileMetaData> fmd = rep.GetFileMetaData(fs, null);

                    IList<FileIntegrationRecord> i = rep.GetSubmittedIntegrations(fs ,null);
                    Assert.IsNotNull(i);
                    Assert.AreEqual(i[0].ToFile.DepotPath, c.Files[0].DepotPath);
                    Assert.AreNotEqual(i[0].FromFile.DepotPath, c.Files[0].DepotPath);
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
        ///A test for GetChangelist
        ///</summary>
        [TestMethod()]
		public void GetChangelistTest()
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

						Changelist c = rep.GetChangelist(5, null);

						Assert.IsNotNull(c);
						Assert.AreEqual("admin", c.OwnerName);
						Assert.AreEqual(c.Files.Count, 1);
						Assert.AreEqual(c.Jobs.Count, 1);
						Assert.IsTrue(c.Files[0].DepotPath.Path.Contains("//depot/MyCode/NewFile.txt"));

                        if (cptype == Utilities.CheckpointType.A)
						{
							c = rep.GetChangelist(4, null);
							Assert.AreEqual("admin", c.OwnerName);
							Assert.AreEqual(c.Files.Count, 2);
							Assert.AreEqual(c.Jobs.Count, 1);
							Assert.IsTrue(c.Files[0].DepotPath.Path.Contains("//depot/TheirCode/ReadMe.txt"));
							Assert.AreEqual(c.Files[0].Digest, "C7DECE3DB80A73F3F53AF4BCF6AC0576");
							Assert.AreEqual(c.Files[0].FileSize, 30);
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
        ///A test for GetChangelistWithUTCConversion
        ///</summary>
        [TestMethod()]
        public void GetChangelistWithUTCConversionTest()
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

                    Utilities.SetClientRoot(rep, TestDir, cptype, ws_client, false);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = user;
                        con.Client = new Client();
                        con.Client.Name = ws_client;

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);
                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        int submitted;
                        using (P4Command cmd = new P4Command(rep, "change", true, "5"))
                        {
                        Options opts = new Options();
                        opts["-o"] = null;
                        Changelist fromChangeCommand = new Changelist();

                        P4CommandResult results = cmd.Run(opts);
                        if (results.Success)
                        {
                            fromChangeCommand.initialize(rep.Connection);
                            fromChangeCommand.FromChangeCmdTaggedOutput((results.TaggedOutput[0]),
                                server.Metadata.DateTimeOffset, FormBase.DSTMismatch(server.Metadata));
                        }

                        SubmitResults sr = fromChangeCommand.Submit(null);
                        
                            submitted = 17;
                            if (cptype == Utilities.CheckpointType.U)
                        {
                            submitted = 13;
                        }
                        }

                        using (P4Command cmd1 = new P4Command(rep, "change", true, submitted.ToString()))
                        {
                            Options opts = new Options();
                        opts["-o"] = null;

                            Changelist fromChangeCommand = new Changelist();

                            P4CommandResult results = cmd1.Run(opts);
                        if (results.Success)
                        {
                            fromChangeCommand.initialize(rep.Connection);
                            fromChangeCommand.FromChangeCmdTaggedOutput((results.TaggedOutput[0]),
                                server.Metadata.DateTimeOffset, FormBase.DSTMismatch(server.Metadata));
                        }
                        
                        Changelist fromDescribeCommand = rep.GetChangelist(submitted, null);

                        Assert.AreEqual(fromDescribeCommand.ModifiedDate, fromChangeCommand.ModifiedDate);
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
        ///A test for GetChangelistWithUTCConversionNoTZDetails
        ///</summary>
        [TestMethod()]
        public void GetChangelistWithUTCConversionNoTZDetailsTest()
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

                        using (P4Command cmd = new P4Command(rep, "describe", true, "5"))
                        {
                        Changelist fromDescribeCommand = new Changelist();

                        P4CommandResult results = cmd.Run(null);
                        if (results.Success)
                        {
                            fromDescribeCommand.FromChangeCmdTaggedOutput((results.TaggedOutput[0]),
                                string.Empty, FormBase.DSTMismatch(server.Metadata));
                        }

                        DateTime unconverted = fromDescribeCommand.ModifiedDate;

                        fromDescribeCommand = new Changelist();
                        results = cmd.Run(null);
                        if (results.Success)
                        {
                            fromDescribeCommand.FromChangeCmdTaggedOutput((results.TaggedOutput[0]),
                                "-200", FormBase.DSTMismatch(server.Metadata));
                        }

                        Assert.AreEqual(unconverted.AddHours(-2), fromDescribeCommand.ModifiedDate);

                        fromDescribeCommand = new Changelist();
                        results = cmd.Run(null);
                        if (results.Success)
                        {
                            fromDescribeCommand.FromChangeCmdTaggedOutput((results.TaggedOutput[0]),
                                "+200", FormBase.DSTMismatch(server.Metadata));
                        }

                        Assert.AreEqual(unconverted.AddHours(2), fromDescribeCommand.ModifiedDate);

                        fromDescribeCommand = new Changelist();
                        results = cmd.Run(null);
                        if (results.Success)
                        {
                            fromDescribeCommand.FromChangeCmdTaggedOutput((results.TaggedOutput[0]),
                                "200", FormBase.DSTMismatch(server.Metadata));
                        }

                        Assert.AreEqual(unconverted.AddHours(2), fromDescribeCommand.ModifiedDate);
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
		///A test for GetChangelists
		///</summary>
		[TestMethod()]
		public void GetChangelistsTest()
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

						IList<Changelist> u = rep.GetChangelists(
                            new Options(ChangesCmdFlags.LongDescription, "admin_space", 10, ChangeListStatus.Pending, null));

						
						Assert.IsNotNull(u);
						Assert.AreEqual(3, u.Count);
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

        ///A test for GetChangelistsjob094473
        ///</summary>
        [TestMethod()]
        public void GetChangelistsTestjob094473A()
        {
            GetChangelistsTestjob094473(Utilities.CheckpointType.A);
        }

        ///A test for GetChangelistsjob094473
        ///</summary>
        [TestMethod()]
        public void GetChangelistsTestjob094473U()
        {
            GetChangelistsTestjob094473(Utilities.CheckpointType.U);
        }

        ///A test for GetChangelistsjob094473
        ///</summary>
        public void GetChangelistsTestjob094473(Utilities.CheckpointType cptype)
        {
            string uri = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";


            Process p4d = null;
            Repository rep = null;
            try
            {
                p4d = Utilities.DeployP4TestServer(TestDir, 13, cptype);
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

                    ChangesCmdOptions opts = new ChangesCmdOptions(ChangesCmdFlags.None,null,
                        0,ChangeListStatus.Submitted,null);
                    IList<Changelist> changes = rep.GetChangelists(opts, null);

                    // ascii 122 unicode 126
                    Assert.IsNotNull(changes);
                    if (cptype == Utilities.CheckpointType.U)
                    {
                        Assert.AreEqual(126, changes.Count);
                    }
                    else
                    {
                        Assert.AreEqual(122, changes.Count);
                    }

                    // only get changes @ or above 100
                    opts = new ChangesCmdOptions(ChangesCmdFlags.None, null,
                        0, ChangeListStatus.Submitted, null, 100);
                    // ascii 39 (151->100) unicode 51 (150->100)
                    changes = rep.GetChangelists(opts, null);
                    Assert.IsNotNull(changes);
                    if (cptype == Utilities.CheckpointType.U)
                    {
                        Assert.AreEqual(51, changes.Count);
                        Assert.AreEqual(150, changes[0].Id);
                    }
                    else
                    {
                        Assert.AreEqual(39, changes.Count);
                        Assert.AreEqual(151, changes[0].Id);
                    }

                    // only get changes @ or above 100 and reverse the order
                    opts = new ChangesCmdOptions(ChangesCmdFlags.ReverseOrder, null,
                        0, ChangeListStatus.Submitted, null, 100);
                    // ascii 39 (100->151) unicode 51 (100->150)
                    changes = rep.GetChangelists(opts, null);
                    Assert.IsNotNull(changes);
                    if (cptype == Utilities.CheckpointType.U)
                    {
                        Assert.AreEqual(51, changes.Count);
                        Assert.AreEqual(100, changes[0].Id);
                    }
                    else
                    {
                        Assert.AreEqual(39, changes.Count);
                        Assert.AreEqual(100, changes[0].Id);
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
        ///A test for GetChangelistsjob097882U
        ///</summary>
        public void GetChangelistsTestjob097882U()
        {
            GetChangelistsTestjob097882(Utilities.CheckpointType.U);
        }

        [TestMethod()]
        ///A test for GetChangelistsjob097882A
        ///</summary>
        public void GetChangelistsTestjob097882A()
        {
            GetChangelistsTestjob097882(Utilities.CheckpointType.A);
        }

        ///A test for GetChangelistsjob097882
        ///</summary>
        public void GetChangelistsTestjob097882(Utilities.CheckpointType cptype)
        {
            string uri = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;
            Repository rep = null;
            try
            {
                p4d = Utilities.DeployP4TestServer(TestDir, 13, cptype);
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

                    ChangesCmdOptions opts = new ChangesCmdOptions(ChangesCmdFlags.None, null,
                        0, ChangeListStatus.Submitted, null);
                    FileSpec fs = new FileSpec();
                    fs.DepotPath = new DepotPath("//depot/...");

                    DateTime dts = new DateTime(2011, 5, 23, 1, 19, 17);
                    DateTime dte = new DateTime(2011, 5, 25, 1, 19, 17);

                    //// Creates and initializes a CultureInfo to use - as the date separator.
                    //// This tests job097882: date range filtering must work even when the
                    //// current culture uses dashes instead of slashes as date separators.
                    //// Wrapped in try/catch because CultureInfo("en-US") requires the ICU
                    //// library, which is absent in invariant-globalization environments.
                    try
                    {
                        CultureInfo myCI = new CultureInfo("en-US", false);

                        //// Clones myCI and modifies the DTFI and NFI instances associated with the clone.
                        CultureInfo myCIclone = (CultureInfo)myCI.Clone();
                        myCIclone.DateTimeFormat.DateSeparator = "-";

                        System.Threading.Thread.CurrentThread.CurrentCulture = myCIclone;

                        // confirm that the dates have dashes as seperators
                        Assert.IsTrue(dts.ToString().Contains("5-23-2011"));
                        Assert.IsTrue(dte.ToString().Contains("5-25-2011"));
                    }
                    catch (CultureNotFoundException)
                    {
                        // ICU library not available (invariant globalization mode);
                        // skip culture-specific format assertions and proceed with core test.
                    }

                    DateTimeVersion start = new DateTimeVersion(dts);
                    DateTimeVersion end = new DateTimeVersion(dte);

                    fs.Version = new VersionRange(start, end);
                    IList<Changelist> changes = rep.GetChangelists(opts, fs);
                    Assert.AreEqual(changes.Count, 4);
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
        ///A test for GetChangelists with a wildcard -c client pattern (AC-1)
        ///</summary>
        [TestMethod()]
        public void GetChangelistsClientWildcardA()
        {
            GetChangelistsClientWildcard(Utilities.CheckpointType.A);
        }

        /// <summary>
        ///A test for GetChangelists with a wildcard -c client pattern (AC-1)
        ///</summary>
        [TestMethod()]
        public void GetChangelistsClientWildcardU()
        {
            GetChangelistsClientWildcard(Utilities.CheckpointType.U);
        }

        /// <summary>
        ///Helper: verifies that a wildcard -c client pattern filters changelists
        ///to only those from matching clients (AC-1).
        ///</summary>
        public void GetChangelistsClientWildcard(Utilities.CheckpointType cptype)
        {
            string uri = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

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

                    // "admin*" should match the admin_space client and return its 3 pending CLs
                    ChangesCmdOptions opts = new ChangesCmdOptions(ChangesCmdFlags.None,
                        "admin*", 0, ChangeListStatus.Pending, null);
                    IList<Changelist> changes = rep.GetChangelists(opts, null);

                    Assert.IsNotNull(changes);
                    Assert.AreEqual(3, changes.Count);
                    foreach (Changelist cl in changes)
                    {
                        Assert.IsTrue(cl.ClientId.StartsWith("admin"),
                            string.Format("ClientId '{0}' does not start with 'admin'", cl.ClientId));
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
        ///A test for GetChangelists with an exclusion -c client pattern (AC-2)
        ///</summary>
        [TestMethod()]
        public void GetChangelistsClientExclusionA()
        {
            GetChangelistsClientExclusion(Utilities.CheckpointType.A);
        }

        /// <summary>
        ///A test for GetChangelists with an exclusion -c client pattern (AC-2)
        ///</summary>
        [TestMethod()]
        public void GetChangelistsClientExclusionU()
        {
            GetChangelistsClientExclusion(Utilities.CheckpointType.U);
        }

        /// <summary>
        ///Helper: verifies that a hyphen-prefixed -c client pattern excludes matching
        ///clients so their changelists are not returned (AC-2).
        ///</summary>
        public void GetChangelistsClientExclusion(Utilities.CheckpointType cptype)
        {
            string uri = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

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

                    // "-admin_space" excludes the only client with pending CLs; result should be empty
                    ChangesCmdOptions opts = new ChangesCmdOptions(ChangesCmdFlags.None,
                        "-admin_space", 0, ChangeListStatus.Pending, null);
                    IList<Changelist> changes = rep.GetChangelists(opts, null);

                    Assert.IsTrue(changes == null || changes.Count == 0,
                        "Expected no pending changelists after excluding admin_space client");
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
        ///A test for GetChangelists with -E (--client-case-insensitive) flag (AC-3).
        ///Only run against an ASCII (case-sensitive) server to demonstrate the contrast.
        ///</summary>
        [TestMethod()]
        public void GetChangelistsClientCaseInsensitive()
        {
            string uri = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;
            Repository rep = null;
            try
            {
                // ASCII (case-sensitive) server only — unicode servers are case-insensitive by default
                p4d = Utilities.DeployP4TestServer(TestDir, 6, Utilities.CheckpointType.A);
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

                    // With -E flag: upper-case wildcard pattern should match case-insensitively and return 3 CLs
                    ChangesCmdOptions optsWithFlag = new ChangesCmdOptions(ChangesCmdFlags.ClientCaseInsensitive,
                        "ADMIN_SP*", 0, ChangeListStatus.Pending, null);
                    IList<Changelist> flagChanges = rep.GetChangelists(optsWithFlag, null);

                    Assert.IsNotNull(flagChanges);
                    Assert.AreEqual(3, flagChanges.Count);
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
        ///A test for GetChangelists with a wildcard -u user pattern (AC-4)
        ///</summary>
        [TestMethod()]
        public void GetChangelistsUserWildcardA()
        {
            GetChangelistsUserWildcard(Utilities.CheckpointType.A);
        }

        /// <summary>
        ///A test for GetChangelists with a wildcard -u user pattern (AC-4)
        ///</summary>
        [TestMethod()]
        public void GetChangelistsUserWildcardU()
        {
            GetChangelistsUserWildcard(Utilities.CheckpointType.U);
        }

        /// <summary>
        ///Helper: verifies that a wildcard -u user pattern filters changelists to
        ///only those owned by matching users (AC-4).
        ///</summary>
        public void GetChangelistsUserWildcard(Utilities.CheckpointType cptype)
        {
            string uri = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

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

                    // "adm*" should match the "admin" user and return the 3 pending CLs
                    ChangesCmdOptions opts = new ChangesCmdOptions(ChangesCmdFlags.None,
                        null, 0, ChangeListStatus.Pending, "adm*");
                    IList<Changelist> changes = rep.GetChangelists(opts, null);

                    Assert.IsNotNull(changes);
                    Assert.AreEqual(3, changes.Count);
                    foreach (Changelist cl in changes)
                    {
                        Assert.IsTrue(cl.OwnerName.StartsWith("adm"),
                            string.Format("OwnerName '{0}' does not start with 'adm'", cl.OwnerName));
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
        ///A test for GetChangelists with multiple semicolon-delimited -c client flags (AC-1).
        ///</summary>
        [TestMethod()]
        public void GetChangelistsMultipleClientsA()
        {
            GetChangelistsMultipleClients(Utilities.CheckpointType.A);
        }

        /// <summary>
        ///A test for GetChangelists with multiple semicolon-delimited -c client flags (AC-1).
        ///</summary>
        [TestMethod()]
        public void GetChangelistsMultipleClientsU()
        {
            GetChangelistsMultipleClients(Utilities.CheckpointType.U);
        }

        /// <summary>
        ///Helper: verifies that a semicolon-delimited clientName in ChangesCmdOptions expands
        ///to multiple -c flags and returns changelists from all specified clients (AC-1).
        ///</summary>
        public void GetChangelistsMultipleClients(Utilities.CheckpointType cptype)
        {
            string uri = configuration.ServerPort;
            string user = "admin";
            string ws_client = "admin_space";

            Process p4d = null;
            Repository rep = null;
            Repository rep2 = null;
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

                    // Create a second client spec "client_b" owned by admin
                    Client clientB = new Client();
                    clientB.Name = "client_b";
                    clientB.OwnerName = user;
                    clientB.Root = TestDir + "/client_b";
                    ViewMap vmB = new ViewMap();
                    vmB.Add("//depot/...", "//client_b/...", MapType.Include);
                    clientB.ViewMap = vmB;
                    rep.CreateClient(clientB, null);

                    // Connect as admin/client_b and create 2 pending changelists
                    Server server2 = new Server(new ServerAddress(uri));
                    rep2 = new Repository(server2);

                    using (Connection con2 = rep2.Connection)
                    {
                        con2.UserName = user;
                        con2.Client = new Client();
                        con2.Client.Name = "client_b";

                        bool connected2 = con2.Connect(null);
                        Assert.IsTrue(connected2);

                        Changelist clB1 = new Changelist();
                        clB1.Description = "Pending CL for client_b (1)";
                        rep2.CreateChangelist(clB1, null);

                        Changelist clB2 = new Changelist();
                        clB2.Description = "Pending CL for client_b (2)";
                        rep2.CreateChangelist(clB2, null);
                    }

                    // Multi-client filter: "admin_space;client_b" should return CLs from both clients
                    ChangesCmdOptions opts = new ChangesCmdOptions(ChangesCmdFlags.None,
                        "admin_space;client_b", 0, ChangeListStatus.Pending, null);
                    IList<Changelist> changes = rep.GetChangelists(opts, null);

                    Assert.IsNotNull(changes);
                    Assert.IsTrue(changes.Count >= 5,
                        string.Format("Expected at least 5 pending CLs (3 from admin_space + 2 from client_b), got {0}", changes.Count));
                    foreach (Changelist cl in changes)
                    {
                        Assert.IsTrue(cl.ClientId == "admin_space" || cl.ClientId == "client_b",
                            string.Format("ClientId '{0}' is not 'admin_space' or 'client_b'", cl.ClientId));
                    }

                    // Single-client filter: "admin_space" only should return exactly 3 CLs, none from client_b
                    ChangesCmdOptions optsOne = new ChangesCmdOptions(ChangesCmdFlags.None,
                        "admin_space", 0, ChangeListStatus.Pending, null);
                    IList<Changelist> changesOne = rep.GetChangelists(optsOne, null);

                    Assert.IsNotNull(changesOne);
                    Assert.AreEqual(3, changesOne.Count,
                        string.Format("Expected exactly 3 pending CLs for admin_space alone, got {0}", changesOne.Count));
                    foreach (Changelist cl in changesOne)
                    {
                        Assert.AreEqual("admin_space", cl.ClientId,
                            string.Format("Expected ClientId 'admin_space', got '{0}'", cl.ClientId));
                    }
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
                p4d?.Dispose();
                rep?.Dispose();
                rep2?.Dispose();
            }
        }

        /// <summary>
        ///A test for GetChangelists with multiple semicolon-delimited -u user flags (AC-2).
        ///</summary>
        [TestMethod()]
        public void GetChangelistsMultipleUsersA()
        {
            GetChangelistsMultipleUsers(Utilities.CheckpointType.A);
        }

        /// <summary>
        ///A test for GetChangelists with multiple semicolon-delimited -u user flags (AC-2).
        ///</summary>
        [TestMethod()]
        public void GetChangelistsMultipleUsersU()
        {
            GetChangelistsMultipleUsers(Utilities.CheckpointType.U);
        }

        /// <summary>
        ///Helper: verifies that a semicolon-delimited userName in ChangesCmdOptions expands
        ///to multiple -u flags and returns changelists from all specified users (AC-2).
        ///</summary>
        public void GetChangelistsMultipleUsers(Utilities.CheckpointType cptype)
        {
            string uri = configuration.ServerPort;
            string user = "admin";
            string ws_client = "admin_space";

            Process p4d = null;
            Repository rep = null;
            Repository rep2 = null;
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

                    // Create a new user "tester" (requires superuser force flag)
                    User tester = new User();
                    tester.Id = "tester";
                    tester.FullName = "Tester User";
                    tester.EmailAddress = "tester@p4test.com";
                    Options forceOpts = new Options();
                    forceOpts["-f"] = null;
                    rep.CreateUser(tester, forceOpts);

                    // Create a client workspace for tester
                    Client testerWs = new Client();
                    testerWs.Name = "tester_ws";
                    testerWs.OwnerName = "tester";
                    testerWs.Root = TestDir + "/tester_ws";
                    ViewMap vmT = new ViewMap();
                    vmT.Add("//depot/...", "//tester_ws/...", MapType.Include);
                    testerWs.ViewMap = vmT;
                    rep.CreateClient(testerWs, null);

                    // Connect as tester/tester_ws and create 1 pending changelist
                    Server server2 = new Server(new ServerAddress(uri));
                    rep2 = new Repository(server2);

                    using (Connection con2 = rep2.Connection)
                    {
                        con2.UserName = "tester";
                        con2.Client = new Client();
                        con2.Client.Name = "tester_ws";

                        bool connected2 = con2.Connect(null);
                        Assert.IsTrue(connected2);

                        Changelist clT1 = new Changelist();
                        clT1.Description = "Pending CL for tester";
                        rep2.CreateChangelist(clT1, null);
                    }

                    // Multi-user filter: "admin;tester" should return CLs from both users
                    ChangesCmdOptions opts = new ChangesCmdOptions(ChangesCmdFlags.None,
                        null, 0, ChangeListStatus.Pending, "admin;tester");
                    IList<Changelist> changes = rep.GetChangelists(opts, null);

                    Assert.IsNotNull(changes);
                    Assert.IsTrue(changes.Count >= 4,
                        string.Format("Expected at least 4 pending CLs (3 from admin + 1 from tester), got {0}", changes.Count));
                    foreach (Changelist cl in changes)
                    {
                        Assert.IsTrue(cl.OwnerName == "admin" || cl.OwnerName == "tester",
                            string.Format("OwnerName '{0}' is not 'admin' or 'tester'", cl.OwnerName));
                    }

                    // Single-user filter: "admin" only should return exactly 3 CLs, none from tester
                    ChangesCmdOptions optsOne = new ChangesCmdOptions(ChangesCmdFlags.None,
                        null, 0, ChangeListStatus.Pending, "admin");
                    IList<Changelist> changesOne = rep.GetChangelists(optsOne, null);

                    Assert.IsNotNull(changesOne);
                    Assert.AreEqual(3, changesOne.Count,
                        string.Format("Expected exactly 3 pending CLs for admin alone, got {0}", changesOne.Count));
                    foreach (Changelist cl in changesOne)
                    {
                        Assert.AreEqual("admin", cl.OwnerName,
                            string.Format("Expected OwnerName 'admin', got '{0}'", cl.OwnerName));
                    }
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
                p4d?.Dispose();
                rep?.Dispose();
                rep2?.Dispose();
            }
        }

        /// <summary>
        ///A test for GetChangelists with combined semicolon-delimited -c and -u flags (AC-3).
        ///</summary>
        [TestMethod()]
        public void GetChangelistsCombinedMultiFiltersA()
        {
            GetChangelistsCombinedMultiFilters(Utilities.CheckpointType.A);
        }

        /// <summary>
        ///A test for GetChangelists with combined semicolon-delimited -c and -u flags (AC-3).
        ///</summary>
        [TestMethod()]
        public void GetChangelistsCombinedMultiFiltersU()
        {
            GetChangelistsCombinedMultiFilters(Utilities.CheckpointType.U);
        }

        /// <summary>
        ///Helper: verifies that semicolon-delimited clientName combined with semicolon-delimited
        ///userName correctly applies both filter sets simultaneously (AC-3).
        ///</summary>
        public void GetChangelistsCombinedMultiFilters(Utilities.CheckpointType cptype)
        {
            string uri = configuration.ServerPort;
            string user = "admin";
            string ws_client = "admin_space";

            Process p4d = null;
            Repository rep = null;
            Repository rep2 = null;
            Repository rep3 = null;
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

                    // Create client_b as admin
                    Client clientB = new Client();
                    clientB.Name = "client_b";
                    clientB.OwnerName = user;
                    clientB.Root = TestDir + "/client_b";
                    ViewMap vmB = new ViewMap();
                    vmB.Add("//depot/...", "//client_b/...", MapType.Include);
                    clientB.ViewMap = vmB;
                    rep.CreateClient(clientB, null);

                    // Connect as admin/client_b and create 2 pending CLs
                    Server server2 = new Server(new ServerAddress(uri));
                    rep2 = new Repository(server2);

                    using (Connection con2 = rep2.Connection)
                    {
                        con2.UserName = user;
                        con2.Client = new Client();
                        con2.Client.Name = "client_b";

                        bool connected2 = con2.Connect(null);
                        Assert.IsTrue(connected2);

                        Changelist clB1 = new Changelist();
                        clB1.Description = "Pending CL for client_b (1)";
                        rep2.CreateChangelist(clB1, null);

                        Changelist clB2 = new Changelist();
                        clB2.Description = "Pending CL for client_b (2)";
                        rep2.CreateChangelist(clB2, null);
                    }

                    // Create user "tester" + client "tester_ws"
                    User tester = new User();
                    tester.Id = "tester";
                    tester.FullName = "Tester User";
                    tester.EmailAddress = "tester@p4test.com";
                    Options forceOpts = new Options();
                    forceOpts["-f"] = null;
                    rep.CreateUser(tester, forceOpts);

                    Client testerWs = new Client();
                    testerWs.Name = "tester_ws";
                    testerWs.OwnerName = "tester";
                    testerWs.Root = TestDir + "/tester_ws";
                    ViewMap vmT = new ViewMap();
                    vmT.Add("//depot/...", "//tester_ws/...", MapType.Include);
                    testerWs.ViewMap = vmT;
                    rep.CreateClient(testerWs, null);

                    // Connect as tester/tester_ws and create 1 pending CL
                    Server server3 = new Server(new ServerAddress(uri));
                    rep3 = new Repository(server3);

                    using (Connection con3 = rep3.Connection)
                    {
                        con3.UserName = "tester";
                        con3.Client = new Client();
                        con3.Client.Name = "tester_ws";

                        bool connected3 = con3.Connect(null);
                        Assert.IsTrue(connected3);

                        Changelist clT1 = new Changelist();
                        clT1.Description = "Pending CL for tester";
                        rep3.CreateChangelist(clT1, null);
                    }

                    // Combined matching filter: clients admin_space+client_b AND user admin
                    // Expects 5 CLs (3 from admin_space + 2 from client_b), all owned by admin
                    ChangesCmdOptions optsMatch = new ChangesCmdOptions(ChangesCmdFlags.None,
                        "admin_space;client_b", 0, ChangeListStatus.Pending, "admin");
                    IList<Changelist> changesMatch = rep.GetChangelists(optsMatch, null);

                    Assert.IsNotNull(changesMatch);
                    Assert.IsTrue(changesMatch.Count >= 5,
                        string.Format("Expected at least 5 CLs matching admin on admin_space/client_b, got {0}", changesMatch.Count));
                    foreach (Changelist cl in changesMatch)
                    {
                        Assert.AreEqual("admin", cl.OwnerName,
                            string.Format("Expected OwnerName 'admin', got '{0}'", cl.OwnerName));
                        Assert.IsTrue(cl.ClientId == "admin_space" || cl.ClientId == "client_b",
                            string.Format("ClientId '{0}' is not 'admin_space' or 'client_b'", cl.ClientId));
                    }

                    // Combined non-matching filter: clients admin_space+client_b AND user tester
                    // Expects 0 CLs because tester has no changelists on admin_space or client_b
                    ChangesCmdOptions optsNoMatch = new ChangesCmdOptions(ChangesCmdFlags.None,
                        "admin_space;client_b", 0, ChangeListStatus.Pending, "tester");
                    IList<Changelist> changesNoMatch = rep.GetChangelists(optsNoMatch, null);

                    Assert.IsTrue(changesNoMatch == null || changesNoMatch.Count == 0,
                        "Expected no changelists when filtering by tester user on admin_space/client_b clients");
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
                p4d?.Dispose();
                rep?.Dispose();
                rep2?.Dispose();
                rep3?.Dispose();
            }
        }
    }
}
