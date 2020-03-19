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
        /// <summary>
        ///A test for CreateStream
        ///</summary>
        [TestMethod()]
        public void CreateStreamTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, 8, unicode);
                    Server server = new Server(new ServerAddress(uri));
                
                    Repository rep = new Repository(server);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = user;
                        con.Client = new Client();
                        con.Client.Name = ws_client;

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        Stream s = new Stream();
                        string targetId = "//Rocket/rel1";
                        s.Id = targetId;
                        s.Type = StreamType.Release;
                        s.Options = new StreamOptionEnum(StreamOption.Locked | StreamOption.NoToParent);
                        s.Parent = new DepotPath("//Rocket/main");
                        s.Name = "Release1";
                        s.Paths = new ViewMap();
                        MapEntry p1 = new MapEntry(MapType.Import, new DepotPath("..."), null);
                        s.Paths.Add(p1);
                        MapEntry p2 = new MapEntry(MapType.Share, new DepotPath("core/gui/..."), null);
                        s.Paths.Add(p2);
                        s.OwnerName = "admin";
                        s.Description = "release stream for first release";
                        s.Ignored = new ViewMap();
                        MapEntry ig1 = new MapEntry(MapType.Include, new DepotPath(".tmp"), null);
                        s.Ignored.Add(ig1);
                        MapEntry ig2 = new MapEntry(MapType.Include, new DepotPath("/bmps/..."), null);
                        s.Ignored.Add(ig2);
                        MapEntry ig3 = new MapEntry(MapType.Include, new DepotPath("/test"), null);
                        s.Ignored.Add(ig3);
                        MapEntry ig4 = new MapEntry(MapType.Include, new DepotPath(".jpg"), null);
                        s.Ignored.Add(ig4);
                        s.Remapped = new ViewMap();
                        MapEntry re1 = new MapEntry(MapType.Include, new DepotPath("..."), new DepotPath("x/..."));
                        s.Remapped.Add(re1);
                        MapEntry re2 = new MapEntry(MapType.Include, new DepotPath("y/*"), new DepotPath("y/z/*"));
                        s.Remapped.Add(re2);
                        MapEntry re3 = new MapEntry(MapType.Include, new DepotPath("ab/..."), new DepotPath("a/..."));
                        s.Remapped.Add(re3);

                        Stream newStream = rep.CreateStream(s);

                        Assert.IsNotNull(newStream);
                        Assert.AreEqual(targetId, newStream.Id);

                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                }
                unicode = !unicode;
            }
        }


        /// <summary>
        ///A test for CreateStream using an invalid parent
        ///</summary>
        [TestMethod()]
        public void CreateStreamInvalidParentTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, 8, unicode);
                    Server server = new Server(new ServerAddress(uri));
               
                    Repository rep = new Repository(server);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = user;
                        con.Client = new Client();
                        con.Client.Name = ws_client;

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        Stream s = new Stream();
                        string targetId = "//Rocket/rel2";
                        s.Id = targetId;
                        s.Type = StreamType.Release;
                        s.Options = new StreamOptionEnum(StreamOption.NoFromParent);
                        s.Name = "Release2";
                        s.Paths = new ViewMap();
                        MapEntry p1 = new MapEntry(MapType.Import, new DepotPath("..."), null);
                        s.Paths.Add(p1);
                        MapEntry p2 = new MapEntry(MapType.Share, new DepotPath("core/gui/..."), null);
                        s.Paths.Add(p2);
                        s.OwnerName = "admin";
                        s.Description = "release stream for first release";

                        try
                        {
                            Stream newStream = rep.CreateStream(s,
                                new StreamCmdOptions(StreamCmdFlags.None, "//Rocket/xxx", StreamType.Release.ToString()));
                        }
                        catch (P4Exception e)
                        {
                            Assert.AreEqual(822153261, e.ErrorCode,
                                "Error in stream specification.\nStream '//Rocket/xxx' doesn't exist.\n");
                        }
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                }
                unicode = !unicode;
            }
        }



        /// <summary>
        ///A test for CreateStream for a non-mainline stream without a parent
        ///</summary>
        [TestMethod()]
        public void CreateParentlessNonMainlineStreamTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, 8, unicode);
                    Server server = new Server(new ServerAddress(uri));
               
                    Repository rep = new Repository(server);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = user;
                        con.Client = new Client();
                        con.Client.Name = ws_client;

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        // parentless non-mainline stream
                        Stream s1 = new Stream();
                        string targetId1 = "//Rocket/rel2";
                        s1.Id = targetId1;
                        s1.Type = StreamType.Release;
                        s1.Options = new StreamOptionEnum(StreamOption.Locked | StreamOption.NoToParent);
                        s1.Name = "Release1";
                        s1.Paths = new ViewMap();
                        MapEntry p1 = new MapEntry(MapType.Import, new DepotPath("..."), null);
                        s1.Paths.Add(p1);
                        MapEntry p2 = new MapEntry(MapType.Share, new DepotPath("core/gui/..."), null);
                        s1.Paths.Add(p2);

                        s1.OwnerName = "admin";
                        s1.Description = "release stream for first release";
                        s1.Ignored = new ViewMap();

                        try
                        {
                            Stream newStream1 = rep.CreateStream(s1);
                        }
                        catch (P4Exception e)
                        {
                            Assert.AreEqual(822153261, e.ErrorCode,
                                ("Error in stream specification.\nMissing required field 'Parent'.\n"));
                        }
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                }
                unicode = !unicode;
            }
        }



        /// <summary>
        ///A test for CreateTaskStream
        ///</summary>
        [TestMethod()]
        public void CreateTaskStreamTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, 8, unicode);
                    Server server = new Server(new ServerAddress(uri));
                
                    Repository rep = new Repository(server);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = user;
                        con.Client = new Client();
                        con.Client.Name = ws_client;

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        Stream s = new Stream();
                        string targetId = "//Rocket/rel1";
                        s.Id = targetId;
                        s.Type = StreamType.Task;
                        s.Options = new StreamOptionEnum(StreamOption.Locked | StreamOption.NoToParent);
                        s.Parent = new DepotPath("//Rocket/main");
                        s.Name = "Release1";
                        s.Paths = new ViewMap();
                        MapEntry p1 = new MapEntry(MapType.Import, new DepotPath("..."), null);
                        s.Paths.Add(p1);
                        MapEntry p2 = new MapEntry(MapType.Share, new DepotPath("core/gui/..."), null);
                        s.Paths.Add(p2);
                        s.OwnerName = "admin";
                        s.Description = "release stream for first release";
                        s.Ignored = new ViewMap();
                        MapEntry ig1 = new MapEntry(MapType.Include, new DepotPath(".tmp"), null);
                        s.Ignored.Add(ig1);
                        MapEntry ig2 = new MapEntry(MapType.Include, new DepotPath("/bmps/..."), null);
                        s.Ignored.Add(ig2);
                        MapEntry ig3 = new MapEntry(MapType.Include, new DepotPath("/test"), null);
                        s.Ignored.Add(ig3);
                        MapEntry ig4 = new MapEntry(MapType.Include, new DepotPath(".jpg"), null);
                        s.Ignored.Add(ig4);
                        s.Remapped = new ViewMap();
                        MapEntry re1 = new MapEntry(MapType.Include, new DepotPath("..."), new DepotPath("x/..."));
                        s.Remapped.Add(re1);
                        MapEntry re2 = new MapEntry(MapType.Include, new DepotPath("y/*"), new DepotPath("y/z/*"));
                        s.Remapped.Add(re2);
                        MapEntry re3 = new MapEntry(MapType.Include, new DepotPath("ab/..."), new DepotPath("a/..."));
                        s.Remapped.Add(re3);

                        Stream newStream = rep.CreateStream(s);

                        Assert.IsNotNull(newStream);
                        Assert.AreEqual(targetId, newStream.Id);
                        Assert.AreEqual(newStream.Type, StreamType.Task);
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                }
                unicode = !unicode;
            }
        }

        /// <summary>
        ///A test for CreateNewStream
        ///</summary>
        [TestMethod()]
        public void CreateNewStreamTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, 8, unicode);
                    Server server = new Server(new ServerAddress(uri));
               
                    Repository rep = new Repository(server);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = user;
                        con.Client = new Client();
                        con.Client.Name = ws_client;

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        Stream newStream = rep.GetStream("//Rocket/newMainlineStream");
                        newStream.Type = StreamType.Mainline;
                        newStream = rep.CreateStream(newStream);
                        Stream fetchStream = rep.GetStream("//Rocket/newMainlineStream");
                        Assert.IsTrue(fetchStream.Type == StreamType.Mainline);

                        // test method with all stream types

                        // main type                       
                        Stream main  = new Stream();
                        string mainTargetId = "//Rocket/mainlinetest";
                        main.Id = mainTargetId;
                        main.Type = StreamType.Mainline;
                        main.Parent = new DepotPath("none");
                        main.Options = new StreamOptionEnum(StreamOption.NoFromParent | StreamOption.NoToParent);
                        main.Options = new StreamOptionEnum(StreamOption.None);
                        main.Name = "mainlinetest";
                        main.Paths = new ViewMap();
                        MapEntry p1 = new MapEntry(MapType.Import, new DepotPath("..."), null);
                        main.Paths.Add(p1);
                        MapEntry p2 = new MapEntry(MapType.Share, new DepotPath("core/gui/..."), null);
                        main.Paths.Add(p2);
                        main.OwnerName = "admin";
 
                        Stream mainline = rep.CreateStream(main, null);
                        fetchStream = rep.GetStream("//Rocket/newMainlineStream");
                        Assert.IsTrue(fetchStream.Type == StreamType.Mainline);

                        // task type
                        Stream task = new Stream();
                        string taskTargetId = "//Rocket/tasktest";
                        task.Id = taskTargetId;
                        task.Type = StreamType.Task;
                        task.Parent = new DepotPath("none");
                        task.Options = new StreamOptionEnum(StreamOption.None);
                        task.Name = "tasktest";
                        task.Paths = new ViewMap();
                        MapEntry taskp1 = new MapEntry(MapType.Share, new DepotPath("..."), null);
                        task.Paths.Add(taskp1);
                        task.OwnerName = "admin";

                        Stream task1 = rep.CreateStream(task, null);
                        fetchStream = rep.GetStream("//Rocket/tasktest");
                        Assert.IsTrue(fetchStream.Type == StreamType.Task);

                        // virtual type
                        Stream virt = new Stream();
                        string virtualTargetId = "//Rocket/virtualtest";
                        virt.Id = virtualTargetId;
                        virt.Type = StreamType.Virtual;
                        virt.Parent = new DepotPath("//Rocket/mainlinetest");
                        virt.Name = "virtualtest";
                        //TODO: find out if default values need to be set for virtual stream
                        virt.Options = new StreamOptionEnum(StreamOption.NoToParent | StreamOption.NoFromParent);
                        virt.Paths = new ViewMap();
                        MapEntry virtp1 = new MapEntry(MapType.Share, new DepotPath("..."), null);
                        virt.Paths.Add(virtp1);
                        virt.OwnerName = "admin";

                        Stream virt1 = rep.CreateStream(virt, null);
                        fetchStream = rep.GetStream("//Rocket/virtualtest");
                        Assert.IsTrue(fetchStream.Type == StreamType.Virtual);

                        // release type
                        Stream release = new Stream();
                        string releaseTargetId = "//Rocket/releasetest";
                        release.Id = releaseTargetId;
                        release.Type = StreamType.Release;
                        release.Parent = new DepotPath("//Rocket/mainlinetest");
                        release.Name = "releasetest";
                        release.Options = new StreamOptionEnum(StreamOption.NoFromParent);
                        release.Paths = new ViewMap();
                        MapEntry releasep1 = new MapEntry(MapType.Share, new DepotPath("..."), null);
                        release.Paths.Add(releasep1);
                        release.OwnerName = "admin";

                        Stream rel1 = rep.CreateStream(release, null);
                                                
                        fetchStream = rep.GetStream("//Rocket/releasetest");
                        Assert.IsTrue(fetchStream.Type == StreamType.Release);

                        // development type
                        Stream dev = new Stream();
                        string developmentTargetId = "//Rocket/devtest";
                        dev.Id = developmentTargetId;
                        dev.Type = StreamType.Development;
                        dev.Parent = new DepotPath("//Rocket/mainlinetest");
                        dev.Name = "releasetest";
                        dev.Options = new StreamOptionEnum(StreamOption.None);
                        dev.Paths = new ViewMap();
                        MapEntry devp1 = new MapEntry(MapType.Share, new DepotPath("..."), null);
                        dev.Paths.Add(devp1);
                        dev.OwnerName = "admin";

                        Stream dev1 = rep.CreateStream(dev, null);

                        fetchStream = rep.GetStream(developmentTargetId);
                        Assert.IsTrue(fetchStream.Type == StreamType.Development);
                       
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                }
                unicode = !unicode;
            }
        }

        /// <summary>
        ///A test for UpdateStream
        ///</summary>
        [TestMethod()]
        public void UpdateStreamTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, 8, unicode);
                    Server server = new Server(new ServerAddress(uri));
                
                    Repository rep = new Repository(server);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = user;
                        con.Client = new Client();
                        con.Client.Name = ws_client;

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        Stream streamToUpdate = rep.GetStream("//Rocket/GUI");
                        streamToUpdate.Options |= StreamOption.Locked;
                        streamToUpdate = rep.UpdateStream(streamToUpdate);

                        // now try to update the locked stream
                        streamToUpdate = rep.GetStream("//Rocket/GUI");
                        streamToUpdate.Description = "edited";
                        try
                        {
                            streamToUpdate = rep.UpdateStream(streamToUpdate);
                        }
                        catch (P4Exception ex)
                        {
                            // check the error message
                            Assert.AreEqual(838867441, ex.ErrorCode, "//Rocket/GUI' is owned by 'harold'.");
                        }

                        // use -f as admin
                        streamToUpdate = rep.GetStream("//Rocket/GUI");
                        streamToUpdate.Description = "edited";
                        string parent = streamToUpdate.Parent.ToString();
                        string type = streamToUpdate.Type.ToString();

                        streamToUpdate = rep.UpdateStream(streamToUpdate, 
                            new StreamCmdOptions(StreamCmdFlags.Force, parent, type ));

                        // confirm the edit
                        streamToUpdate = rep.GetStream("//Rocket/GUI");
                        Assert.IsTrue(streamToUpdate.Description.Contains("edited"));
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                }
                unicode = !unicode;
            }
        }

        /// <summary>
        ///A test for DeleteStream
        ///</summary>
        [TestMethod()]
        public void DeleteStreamTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, 8, unicode);
                    Server server = new Server(new ServerAddress(uri));
               
                    Repository rep = new Repository(server);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = user;
                        con.Client = new Client();
                        con.Client.Name = ws_client;

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        Stream s = new Stream();
                        string targetId = "//Rocket/rel1";
                        s.Id = targetId;
                        s.Type = StreamType.Release;
                        s.Options = new StreamOptionEnum(StreamOption.Locked | StreamOption.NoToParent);
                        s.Parent = new DepotPath("//Rocket/main");
                        s.Name = "Release1";
						s.Paths = new ViewMap();
						MapEntry p1 = new MapEntry(MapType.Import, new DepotPath("..."), null);
                        s.Paths.Add(p1);
						MapEntry p2 = new MapEntry(MapType.Share, new DepotPath("core/gui/..."), null);
                        s.Paths.Add(p2);
                        s.OwnerName = "admin";
                        s.Description = "release stream for first release";
						s.Ignored = new ViewMap();
						MapEntry ig1 = new MapEntry(MapType.Include, new DepotPath(".tmp"), null);
                        s.Ignored.Add(ig1);
						MapEntry ig2 = new MapEntry(MapType.Include, new DepotPath("/bmps/..."), null);
                        s.Ignored.Add(ig2);
						MapEntry ig3 = new MapEntry(MapType.Include, new DepotPath("/test"), null);
                        s.Ignored.Add(ig3);
						MapEntry ig4 = new MapEntry(MapType.Include, new DepotPath(".jpg"), null);
                        s.Ignored.Add(ig4);
                        s.Remapped = new ViewMap();
						MapEntry re1 = new MapEntry(MapType.Include, new DepotPath("..."), new DepotPath("x/..."));
                        s.Remapped.Add(re1);
						MapEntry re2 = new MapEntry(MapType.Include, new DepotPath("y/*"), new DepotPath("y/z/*"));
                        s.Remapped.Add(re2);
						MapEntry re3 = new MapEntry(MapType.Include, new DepotPath("ab/..."), new DepotPath("a/..."));
                        s.Remapped.Add(re3);

                        Stream newStream = rep.CreateStream(s);

                        Assert.IsNotNull(newStream);

						IList<Stream> slist = rep.GetStreams(new Options(StreamsCmdFlags.None, null, null, "//Rocket/rel1", -1));

						Assert.AreEqual(slist.Count, 35);

                        StreamCmdOptions opts = new StreamCmdOptions(StreamCmdFlags.Force,
                            null, null);
                        rep.DeleteStream(newStream, opts);

						slist = rep.GetStreams(new Options(StreamsCmdFlags.None, null, null, "//Rocket/rel1", -1));

						Assert.AreEqual(slist.Count, 34);

                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                }
                unicode = !unicode;
            }
        }


        /// <summary>
        ///A test for DeleteStream where the stream has children
        ///</summary>
        [TestMethod()]
        public void DeleteStreamWithChildTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";
            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, 8, unicode);
                    Server server = new Server(new ServerAddress(uri));
               
                    Repository rep = new Repository(server);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = user;
                        con.Client = new Client();
                        con.Client.Name = ws_client;

                        bool connected = con.Connect(null);

                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);                       

                        Stream newStream = rep.GetStream("//Rocket/GUI");

                        Assert.IsNotNull(newStream);

                        try
                        {
                            rep.DeleteStream(newStream, null);
                        }
                        catch (P4Exception e)
                        {
                            Assert.AreEqual(e.ErrorCode, 822417988, "Stream '//Rocket/GUI' has child streams; cannot delete until they are removed.\n");
                        }                      
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                }
                unicode = !unicode;
            }
        }


        /// <summary>
        ///A test for DeleteStream where the stream has clients
        ///</summary>
        [TestMethod()]
        public void DeleteStreamWithClientsTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, 8, unicode);
                    Server server = new Server(new ServerAddress(uri));
                
                    Repository rep = new Repository(server);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = user;
                        con.Client = new Client();
                        con.Client.Name = ws_client;

                        bool connected = con.Connect(null);

                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        Stream newStream = rep.GetStream("//flow/D7");

                        Assert.IsNotNull(newStream);

                        try
                        {
                            rep.DeleteStream(newStream, null);
                        }
                        catch (P4Exception e)
                        {
                            Assert.AreEqual(e.ErrorCode, 822417989, "Stream '//flow/D7' has active clients; cannot delete until they are removed.\n");
                        }
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                }
                unicode = !unicode;
            }
        }

      

        /// <summary>
        ///A test for DeleteStream with a locked stream where the user is not the stream owner
        ///</summary>
        [TestMethod()]
        public void DeleteStreamLockedTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "bernard";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, 8, unicode);
                    Server server = new Server(new ServerAddress(uri));
               
                    Repository rep = new Repository(server);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = user;                      

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        //create an ownersubmit /locked stream as user bernard
                        Stream s = new Stream();
                        string targetId = "//Rocket/rel2";
                        s.Id = targetId;
                        s.Type = StreamType.Release;
                        s.Options = new StreamOptionEnum(StreamOption.Locked | StreamOption.NoToParent | StreamOption.OwnerSubmit);
                        s.Parent = new DepotPath("//Rocket/main");
                        s.Name = "Release2";
                        s.Paths = new ViewMap();
						MapEntry p1 = new MapEntry(MapType.Import, new DepotPath("..."), null);
                        s.Paths.Add(p1);
						MapEntry p2 = new MapEntry(MapType.Share, new DepotPath("core/gui/..."), null);
                        s.Paths.Add(p2);
                        s.OwnerName = "bernard";
                        s.Description = "release stream for first release";

                        Stream rel2 = rep.CreateStream(s);
                        rep.UpdateStream(rel2);

                        //switch to admin and delete stream with -f 
                        con.UserName = "admin";
                        con.Client = new Client();
                        con.Client.Name = ws_client;

                        bool connected2 = con.Connect(null);
                        Assert.IsTrue(connected2);

                        StreamCmdOptions opts = new StreamCmdOptions(StreamCmdFlags.Force,
                                                    null, null); 
                        rep.DeleteStream(rel2, opts);                    
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                }
                unicode = !unicode;
            }
        }

        /// <summary>
        ///A test for GetStreamjob072025
        ///</summary>
        [TestMethod()]
        public void GetStreamTestjob072025A()
        {
            GetStreamTestjob072025(false);
        }
        /// <summary>
        ///A test for GetStreamjob072025
        ///</summary>
        [TestMethod()]
        public void GetStreamTestjob072025U()
        {
            GetStreamTestjob072025(true);
        }
        /// <summary>
        ///A test for GetStreamjob072025
        ///</summary>
        public void GetStreamTestjob072025(bool unicode)
        {
            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            try
            {
                p4d = Utilities.DeployP4TestServer(TestDir, 8, unicode);
                Server server = new Server(new ServerAddress(uri));

                Repository rep = new Repository(server);

                using (Connection con = rep.Connection)
                {
                    con.UserName = user;
                    con.Client = new Client();
                    con.Client.Name = ws_client;

                    bool connected = con.Connect(null);
                    Assert.IsTrue(connected);

                    Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                    string targetStream = "//Rocket/GUI";

                    Stream s = rep.GetStream(targetStream);

                    Assert.IsNotNull(s);

                    Assert.AreEqual(targetStream, s.Id);

                    // get a stream spec for a stream that doesn't exist
                    string targetStream1 = "//Rocket/GUI2";
                    Stream s1 = rep.GetStream(targetStream1,
                        new StreamCmdOptions(StreamCmdFlags.None, "//Rocket/MAIN", StreamType.Development.ToString()));
                    Assert.IsNotNull(s1);
                    Assert.AreEqual(targetStream1, s1.Id);
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
            }
        }

        /// <summary>
        ///A test for GetStream
        ///</summary>
        [TestMethod()]
        public void GetStreamTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, 8, unicode);
                    Server server = new Server(new ServerAddress(uri));
                
                    Repository rep = new Repository(server);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = user;
                        con.Client = new Client();
                        con.Client.Name = ws_client;

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        string targetStream = "//Rocket/GUI";

                        Stream s = rep.GetStream(targetStream);

                        Assert.IsNotNull(s);

                        Assert.AreEqual(targetStream, s.Id);

                        // get a stream spec for a stream that doesn't exist
                        string targetStream1 = "//Rocket/GUI2";
                        Stream s1 = rep.GetStream(targetStream1, "//Rocket/MAIN",
                            new StreamCmdOptions(StreamCmdFlags.None, "//Rocket/MAIN", StreamType.Development.ToString()));
                        Assert.IsNotNull(s1);
                        Assert.AreEqual(targetStream1, s1.Id);
                    
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                }
            }
        }

        /// <summary>
        ///A test for GetStreamWithSpacesViewMap
        ///</summary>
        [TestMethod()]
        public void GetStreamWithSpacesViewMapTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, 13, unicode);
                    Server server = new Server(new ServerAddress(uri));
               
                    Repository rep = new Repository(server);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = user;
                        con.Client = new Client();
                        con.Client.Name = ws_client;

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        Stream s = new Stream();
                        s.Id = "//Rocket/SPACE";
                        s.Name = "SPACE";
                        s.OwnerName = "admin";
                        s.Description = "stream with spaces in viewmap";
                        s.Type = StreamType.Development;
                        PathSpec thisParent = new DepotPath("//Rocket/MAIN");
                        s.Parent = thisParent;
                        s.Options = StreamOption.None;
                        List<string> map = new List<string>();
                        map.Add("import \"core/gui/...\" \"//Rocket/there are spaces/core/gui/...\"");
                        ViewMap spacesMap = new ViewMap(map);
                        s.Paths = spacesMap;

                        s = rep.CreateStream(s);


                        string targetStream = "//Rocket/SPACE";

                        s = rep.GetStream(targetStream);

                        Assert.IsNotNull(s);

                        Assert.AreEqual(targetStream, s.Id);
                        Assert.AreEqual(s.Type, StreamType.Development);
                        Assert.AreEqual(s.Paths[0].Type, MapType.Import);
                        Assert.IsTrue(s.Paths[0].Right.Path.Contains("there are spaces"));
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                }
            }
        }

        /// <summary>
        ///A test for GetStreams
        ///</summary>
        [TestMethod()]
        public void GetStreamsTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, 8, unicode);
                    Server server = new Server(new ServerAddress(uri));
                
                    Repository rep = new Repository(server);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = user;
                        con.Client = new Client();
                        con.Client.Name = ws_client;

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        IList<Stream> s = rep.GetStreams(new Options(StreamsCmdFlags.None,
                            "Parent=//flow/mainline & Type=development", null, "//...", 3));


                        Assert.IsNotNull(s);
                        Assert.AreEqual(3, s.Count);
                        Assert.AreEqual("D2", s[1].Name);
						//DateTime time = new DateTime(2011, 6, 24, 16, 23, 49);
						//if (unicode)
						//{
						//    time = new DateTime(2011, 6, 27, 15, 4, 21);
						//}
						//Assert.AreEqual(s[0].Accessed,time);
						//Assert.AreEqual(s[0].Updated, time);
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                }
                unicode = !unicode;
            }
        }

        /// <summary>
        ///A test for GetStreamsCheckDescription
        ///</summary>
        [TestMethod()]
        public void GetStreamsCheckDescriptionTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, 8, unicode);
                    Server server = new Server(new ServerAddress(uri));
                
                    Repository rep = new Repository(server);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = user;
                        con.Client = new Client();
                        con.Client.Name = ws_client;

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        IList<Stream> s = rep.GetStreams(new Options(StreamsCmdFlags.None,
                            null, null, "//flow/D1", 3));


                        Assert.IsNotNull(s);
                        Assert.AreEqual(3, s.Count);
                        Assert.AreEqual("D2", s[1].Name);
                        Assert.IsTrue(s[0].Description.Contains("Introduces two new paths not in the parent, but shared to children"));

                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                }
                unicode = !unicode;
            }
        }

        /// <summary>
        ///A test for GetStreamMetaData
        ///</summary>
        [TestMethod()]
        public void GetStreamMetaDataTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                try
                {
                    p4d = Utilities.DeployP4TestServer(TestDir, 13, unicode);
                    Server server = new Server(new ServerAddress(uri));
                
                    Repository rep = new Repository(server);

                    using (Connection con = rep.Connection)
                    {
                        con.UserName = user;
                        con.Client = new Client();
                        con.Client.Name = ws_client;

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        Stream s = rep.GetStream("//Rocket/GUI",null,null);
                        StreamMetaData smd = rep.GetStreamMetaData(s, null);
                        Assert.IsNotNull(smd);
                        Assert.AreEqual(smd.Stream, new DepotPath("//Rocket/GUI"));
                        Assert.AreEqual(smd.Parent, new DepotPath("//Rocket/MAIN"));
                        Assert.IsTrue(smd.ChangeFlowsToParent);
                        Assert.IsTrue(smd.ChangeFlowsFromParent);
                        Assert.IsFalse(smd.FirmerThanParent);
                        Assert.AreEqual(smd.Type, StreamType.Development);
                        Assert.AreEqual(smd.ParentType, StreamType.Mainline);
                        Assert.AreEqual(smd.IntegToParentHow, StreamMetaData.IntegAction.Copy);

                        // test the -r flag
                        Stream s1 = rep.GetStream("//Rocket/GUI", null, null);
                        StreamMetaData smd1 = rep.GetStreamMetaData(s, new Options(GetStreamMetaDataCmdFlags.Reverse));
                        Assert.IsNotNull(smd1);
                        Assert.AreEqual(smd1.Stream, new DepotPath("//Rocket/GUI"));
                        Assert.AreEqual(smd1.Parent, new DepotPath("//Rocket/MAIN"));
                        Assert.IsTrue(smd1.ChangeFlowsToParent);
                        Assert.IsTrue(smd1.ChangeFlowsFromParent);
                        Assert.IsFalse(smd1.FirmerThanParent);
                        Assert.AreEqual(smd1.Type, StreamType.Development);
                        Assert.AreEqual(smd1.ParentType, StreamType.Mainline);
                        Assert.AreEqual(smd1.IntegFromParentHow, StreamMetaData.IntegAction.Merge);

                        // test the -a flag
                        Stream s2 = rep.GetStream("//Rocket/GUI", null, null);
                        StreamMetaData smd2 = rep.GetStreamMetaData(s, new Options(GetStreamMetaDataCmdFlags.All));
                        Assert.IsNotNull(smd2);
                        Assert.AreEqual(smd2.Stream, new DepotPath("//Rocket/GUI"));
                        Assert.AreEqual(smd2.Parent, new DepotPath("//Rocket/MAIN"));
                        Assert.IsTrue(smd2.ChangeFlowsToParent);
                        Assert.IsTrue(smd2.ChangeFlowsFromParent);
                        Assert.IsFalse(smd2.FirmerThanParent);
                        Assert.AreEqual(smd2.Type, StreamType.Development);
                        Assert.AreEqual(smd2.ParentType, StreamType.Mainline);
                        Assert.AreEqual(smd2.IntegFromParentHow, StreamMetaData.IntegAction.Merge);
                        Assert.AreEqual(smd2.FromResult, "cache");
                        Assert.AreEqual(smd2.ToResult, "cache");

                        // TODO: add tests with different flags (-c and -s)
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                }
                unicode = !unicode;
            }
        }

        [TestMethod()]
        ///A test for GetStreamImportSubmittableU
        ///</summary>
        public void GetStreamImportSubmittableU()
        {
            GetStreamImportSubmittable(true);
        }

        [TestMethod()]
        ///A test for GetStreamImportSubmittableA
        ///</summary>
        public void GetStreamImportSubmittableA()
        {
            GetStreamImportSubmittable(false);
        }
        
        public void GetStreamImportSubmittable(bool unicode)
        {
            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            Server server = new Server(new ServerAddress(uri));
            try
            {
                p4d = Utilities.DeployP4TestServer(TestDir, 13, unicode);
                Repository rep = new Repository(server);

                using (Connection con = rep.Connection)
                {
                    con.UserName = user;
                    con.Client = new Client();
                    con.Client.Name = ws_client;

                    bool connected = con.Connect(null);
                    Assert.IsTrue(connected);

                    Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                    Stream s = rep.GetStream("//Rocket/MAIN");
                    s.Paths.Add("import+ TestData/... //depot/TestData/...");
                    rep.UpdateStream(s);

                    s = rep.GetStream("//Rocket/MAIN");
                    Assert.AreEqual(s.Paths[1].Type, MapType.ImportSubmittable);
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
            }
        }

        [TestMethod()]
        ///A test for GetStreamImportSubmittableU
        ///</summary>
        public void GetStreamMappingLeadingPlusMinusU()
        {
            GetStreamMappingLeadingPlusMinus(true);
        }

        [TestMethod()]
        ///A test for GetStreamImportSubmittableA
        ///</summary>
        public void GetStreamMappingLeadingPlusMinusA()
        {
            GetStreamMappingLeadingPlusMinus(false);
        }

        public void GetStreamMappingLeadingPlusMinus(bool unicode)
        {
            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;

            Server server = new Server(new ServerAddress(uri));
            try
            {
                p4d = Utilities.DeployP4TestServer(TestDir, 13, unicode);
                Repository rep = new Repository(server);

                using (Connection con = rep.Connection)
                {
                    con.UserName = user;
                    con.Client = new Client();
                    con.Client.Name = ws_client;

                    bool connected = con.Connect(null);
                    Assert.IsTrue(connected);

                    Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                    Stream s = rep.GetStream("//Rocket/MAIN");
                    s.Paths.Add("share --Special.txt");
                    s.Paths.Add("share ++Special.txt");
                    rep.UpdateStream(s);

                    s = rep.GetStream("//Rocket/MAIN");
                    Assert.AreEqual(s.Paths[1].Left.Path, "--Special.txt");
                    Assert.AreEqual(s.Paths[2].Left.Path, "++Special.txt");
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
            }
        }
    }
}
