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
                    p4d?.Dispose();
                    rep?.Dispose();
                }
            }
        }


        /// <summary>
        ///A test for CreateStream using an invalid parent
        ///</summary>
        [TestMethod()]
        public void CreateStreamInvalidParentTest()
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
                        catch (System.ArgumentNullException e)
                        {
                            Assert.AreEqual(e.ParamName, "Parent");
                            Assert.IsTrue(e.Message.Contains("Value cannot be null"));
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
        ///A test for CreateStream for a non-mainline stream without a parent
        ///</summary>
        [TestMethod()]
        public void CreateParentlessNonMainlineStreamTest()
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
                        catch (ArgumentNullException e)
                        {
                            Assert.AreEqual(e.ParamName, "Parent");
                            Assert.IsTrue(e.Message.Contains("Value cannot be null"));
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
        ///A test for CreateTaskStream
        ///</summary>
        [TestMethod()]
        public void CreateTaskStreamTest()
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
                    p4d?.Dispose();
                    rep?.Dispose();
                }
            }
        }

        /// <summary>
        ///A test for CreateNewStream
        ///</summary>
        [TestMethod()]
        public void CreateNewStreamTest()
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
                        task.Parent = new DepotPath("//Rocket/mainlinetest");
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
                    p4d?.Dispose();
                    rep?.Dispose();
                }
            }
        }

        /// <summary>
        ///A test for UpdateStream
        ///</summary>
        [TestMethod()]
        public void UpdateStreamTest()
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
                    p4d?.Dispose();
                    rep?.Dispose();
                }
            }
        }

        /// <summary>
        ///A test for DeleteStream
        ///</summary>
        [TestMethod()]
        public void DeleteStreamTest()
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
                    p4d?.Dispose();
                    rep?.Dispose();
                }
            }
        }


        /// <summary>
        ///A test for DeleteStream where the stream has children
        ///</summary>
        [TestMethod()]
        public void DeleteStreamWithChildTest()
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
                    p4d?.Dispose();
                    rep?.Dispose();
                }
            }
        }


        /// <summary>
        ///A test for DeleteStream where the stream has clients
        ///</summary>
        [TestMethod()]
        public void DeleteStreamWithClientsTest()
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
                    p4d?.Dispose();
                    rep?.Dispose();
                }
            }
        }

      

        /// <summary>
        ///A test for DeleteStream with a locked stream where the user is not the stream owner
        ///</summary>
        [TestMethod()]
        public void DeleteStreamLockedTest()
        {
            string uri = configuration.ServerPort;
            string user = "bernard";
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
                    p4d?.Dispose();
                    rep?.Dispose();
                }
            }
        }
        
        /// <summary>
        ///A test for GetStreamjob072025 Ascii
        ///</summary>
        [TestMethod()]
        public void GetStreamTestjob072025A()
        {
            GetStreamTestjob072025(Utilities.CheckpointType.A);
        }
        
        /// <summary>
        ///A test for GetStreamjob072025 Unicode
        ///</summary>
        [TestMethod()]
        public void GetStreamTestjob072025U()
        {
            GetStreamTestjob072025(Utilities.CheckpointType.U);
        }
        
        /// <summary>
        ///A test for GetStreamjob072025
        ///</summary>
        public void GetStreamTestjob072025(Utilities.CheckpointType cptype)
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
                p4d?.Dispose();
                rep?.Dispose();
            }
        }

        /// <summary>
        ///A test for GetStream
        ///</summary>
        [TestMethod()]
        public void GetStreamTest()
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
                    p4d?.Dispose();
                    rep?.Dispose();
                }
            }
        }

        /// <summary>
        ///A test for GetStreamWithSpacesViewMap
        ///</summary>
        [TestMethod()]
        public void GetStreamWithSpacesViewMapTest()
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
                    p4d?.Dispose();
                    rep?.Dispose();
                }
            }
        }

        /// <summary>
        ///A test for GetStreams
        ///</summary>
        [TestMethod()]
        public void GetStreamsTest()
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

                        IList<Stream> s = rep.GetStreams(new Options(StreamsCmdFlags.None,
                            "Parent=//flow/mainline & Type=development", null, "//...", 3));


                        Assert.IsNotNull(s);
                        Assert.AreEqual(3, s.Count);
                        Assert.AreEqual("D2", s[1].Name);

                        string viewmatch = "//flow/mainline/...";
                        IList<Stream> s1 = rep.GetStreams(new Options(StreamsCmdFlags.None, null, null, null, 5, viewmatch));
                        Assert.IsNotNull(s1);
                        Assert.AreEqual("//flow/D1", s1[0].Id);
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
        ///A test for GetStreamsCheckDescription
        ///</summary>
        [TestMethod()]
        public void GetStreamsCheckDescriptionTest()
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
                    p4d?.Dispose();
                    rep?.Dispose();
                }
            }
        }

        /// <summary>
        ///A test for GetStreamMetaData
        ///</summary>
        [TestMethod()]
        public void GetStreamMetaDataTest()
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

                        Stream s = rep.GetStream("//Rocket/GUI",null);
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
                        Stream s1 = rep.GetStream("//Rocket/GUI", null);
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
                        Stream s2 = rep.GetStream("//Rocket/GUI", null);
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
                    p4d?.Dispose();
                    rep?.Dispose();
                }
            }
        }

        /// <summary>
        ///A test for GetStreamImportSubmittableU
        ///</summary>
        [TestMethod()]
        public void GetStreamImportSubmittableU()
        {
            GetStreamImportSubmittable(Utilities.CheckpointType.U);
        }

        /// <summary>
        ///A test for GetStreamImportSubmittableA
        ///</summary>
        [TestMethod()]
        public void GetStreamImportSubmittableA()
        {
            GetStreamImportSubmittable(Utilities.CheckpointType.A);
        }
        
        public void GetStreamImportSubmittable(Utilities.CheckpointType cptype)
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
                p4d?.Dispose();
                rep?.Dispose();
            }
        }

        /// <summary>
        ///A test for GetStreamImportSubmittableU
        ///</summary>
        [TestMethod()]
        public void GetStreamMappingLeadingPlusMinusU()
        {
            GetStreamMappingLeadingPlusMinus(Utilities.CheckpointType.U);
        }

        /// <summary>
        ///A test for GetStreamImportSubmittableA
        ///</summary>
        [TestMethod()]
        public void GetStreamMappingLeadingPlusMinusA()
        {
            GetStreamMappingLeadingPlusMinus(Utilities.CheckpointType.A);
        }

        public void GetStreamMappingLeadingPlusMinus(Utilities.CheckpointType cptype)
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
                p4d?.Dispose();
                rep?.Dispose();
            }
        }

        [TestMethod()]
        public void CreateCustomFieldStreamTest()
        {                       
            string uri = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;
            Repository rep = null;
            try
            {
                p4d = Utilities.DeployP4TestServer(TestDir, 21, Utilities.CheckpointType.A);
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

                    // Setting spec.custom=1
                    using (P4Command configureCmd = new P4Command(con, "configure", true, null))
                    {
                        Options opts = new Options();
                        opts["set"] = "spec.custom=1";
                        P4CommandResult setRes = configureCmd.Run(opts);
                    }

                    // Send custom spec to server
                    using (P4Command specStreamCmd = new P4Command(con, "spec", false, null))
                    {
                        Options specOpts = new Options();
                        specOpts["-i"] = "stream";
                        specStreamCmd.DataSet = CustomStreamSpec201;
                        specStreamCmd.Run(specOpts);
                    }

                    // Create a Stream spec
                    //Define values for stream properties
                    string expectedP1 = "...";
                    string expectedP2 = "aps/...";
                    string expectedP3 = "aps/...";
                    string expectedP4 = "remapped-aps/...";
                    string expectedP5 = "/flow/D1/a";
                    string testField1 = "TestValue1";
                    List<string> testField2 = new List<string> { "listValues1", "listValues2" };
                    int testField3 = 20;
                    Dictionary<string, object> customFields = new Dictionary<string, object>() 
                    {
                        { "TestField", testField1 },
                        { "NewField", testField2 },
                        { "DifferentField", testField3 }
                    };
                    ViewMap Paths = new ViewMap() 
                    {
                        { new MapEntry(MapType.Share, new DepotPath(expectedP1), null) },
                        { new MapEntry(MapType.Share, new DepotPath(expectedP2), null) }
                    };
                    ViewMap Remapped = new ViewMap() { new MapEntry(MapType.None, new DepotPath(expectedP3), new DepotPath(expectedP4)) };
                    ViewMap Ignored = new ViewMap() { new MapEntry(MapType.None, new DepotPath(expectedP5), null) };
                    StreamOptionEnum options =  new StreamOptionEnum(
                        StreamOption.Locked | 
                        StreamOption.NoToParent | 
                        StreamOption.OwnerSubmit |
                        StreamOption.MergeAny );
                    string description = "release stream for first release";
                    string ownerName = "admin";
                    DepotPath parent = new DepotPath("none");
                    string name = "StreamCustomFieldTest";
                    string id = "//Rocket/StreamCustomFieldTest";
                    StreamType type = StreamType.Mainline;

                    // Assign values to stream properties
                    Stream streamCustom = new Stream();
                    streamCustom.Id = id;
                    streamCustom.Type = type;
                    streamCustom.Options = options;
                    streamCustom.Parent = parent;
                    streamCustom.Name = name;
                    streamCustom.OwnerName = ownerName;
                    streamCustom.Description = description;
                    streamCustom.Paths = Paths;
                    streamCustom.Remapped = Remapped;
                    streamCustom.Ignored = Ignored;
                    streamCustom.CustomFields = customFields;
  
                    // push the stream back to the server
                    // This will test the toString method to convert object to server stream spec
                    Stream createdCustomStream = rep.CreateStream(streamCustom);

                    // Read back the stream properties and compare to initial values
                    Stream readStream = rep.GetStream(id);
                    Assert.IsNotNull(readStream);
                    Assert.AreEqual(readStream.Id, id);
                    Assert.AreEqual(readStream.Type, type);
                    Assert.AreEqual(readStream.Options.ToString(), "OwnerSubmit, Locked, NoToParent, MergeAny");
                    Assert.AreEqual(readStream.Parent, parent);
                    Assert.AreEqual(readStream.Name, name);                    
                    Assert.AreEqual(readStream.OwnerName, ownerName);
                    Assert.AreEqual(readStream.Description, description + Environment.NewLine);
                    Assert.AreEqual(readStream.Paths[1].Left.ToString(), expectedP2);
                    Assert.AreEqual(readStream.Remapped[0].Left.ToString(), expectedP3);
                    Assert.AreEqual(readStream.Remapped[0].Right.ToString(), expectedP4);
                    Assert.AreEqual(readStream.Ignored[0].Left.ToString(), expectedP5);
                    Assert.AreEqual(readStream.CustomFields["TestField"], testField1);
                    Assert.AreEqual(readStream.CustomFields["NewField"].ToString(), testField2.ToString());
                    Assert.AreEqual(readStream.CustomFields["DifferentField"], testField3.ToString());                    
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
        public void CreateSpecParentViewTest()
        {
            string uri = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;
            Repository rep = null;
            try
            {
                p4d = Utilities.DeployP4TestServer(TestDir, 21, Utilities.CheckpointType.A);
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


                    StreamOption options = new StreamOption();
                    string description = "ParentView spec development";
                    string ownerName = "admin";
                    DepotPath parent = new DepotPath("//Rocket/MAIN");
                    string name = "StreamParentView1";
                    string id = "//Rocket/StreamParentView1";
                    StreamType type = StreamType.Development;
                    ParentView parentView = ParentView.NoInherit;
                    ViewMap Paths = new ViewMap()
                    {
                        { new MapEntry(MapType.Share, new DepotPath("..."), null) }
                    };

                    // Assign values to stream properties
                    Stream streamParentView = new Stream();
                    streamParentView.Id = id;
                    streamParentView.Type = type;
                    streamParentView.Options = options;
                    streamParentView.Parent = parent;
                    streamParentView.Name = name;
                    streamParentView.OwnerName = ownerName;
                    streamParentView.Description = description;
                    streamParentView.Paths = Paths;
                    streamParentView.ParentView = parentView;

                    Stream createdCustomStream = rep.CreateStream(streamParentView);

                    // Read back the stream properties and compare to initial values
                    Stream readStream = rep.GetStream(id);
                    Assert.IsNotNull(readStream);
                    Assert.AreEqual(readStream.ParentView.ToString(), parentView.ToString());
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
        public void ViewCommentsTest()
        {
            string uri = configuration.ServerPort;
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            Process p4d = null;
            Repository rep = null;
            try
            {
                p4d = Utilities.DeployP4TestServer(TestDir, 21, Utilities.CheckpointType.A);
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


                    StreamOption options = new StreamOption();
                    string description = "View comments spec development";
                    string ownerName = "admin";
                    DepotPath parent = new DepotPath("//Rocket/MAIN");
                    string name = "StreamViewComments1";
                    string id = "//Rocket/StreamViewComments1";
                    StreamType type = StreamType.Development;
                    ViewMap Paths = new ViewMap()
                    {
                        { new MapEntry(MapType.Share, new DepotPath("..."), null, "## inline comment") },
                        { new MapEntry(MapType.None, null, null, "## newline comment") }
                    };

                    // Assign values to stream properties
                    Stream streamViewComments = new Stream();
                    streamViewComments.Id = id;
                    streamViewComments.Type = type;
                    streamViewComments.Options = options;
                    streamViewComments.Parent = parent;
                    streamViewComments.Name = name;
                    streamViewComments.OwnerName = ownerName;
                    streamViewComments.Description = description;
                    streamViewComments.Paths = Paths;

                    rep.CreateStream(streamViewComments);

                    // Read back the stream properties and compare to initial values
                    Stream readStream = rep.GetStream(id);
                    Assert.IsNotNull(readStream);
                    Assert.AreEqual("## inline comment",readStream.Paths[0].Comment.ToString());
                    Assert.AreEqual("## newline comment", readStream.Paths[1].Comment.ToString());
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
                p4d?.Dispose();
                rep?.Dispose();
            }
        }


        // Custom Spec
        private static String CustomStreamSpec201 = @"# A Perforce Spec Specification.
#
#  Updating this form can be dangerous!
#  To update the job spec, see 'p4 help jobspec' for proper directions.
#  To update the stream spec, see 'p4 help streamspec' for proper directions.
#  Otherwise, see 'p4 help spec'.

Fields:
        701 Stream word 64 key
        705 Update date 20 always
        706 Access date 20 always
        704 Owner word 32 optional
        703 Name line 32 required
        702 Parent word 64 required
        708 Type word 32 required
        709 Description text 128 optional
        707 Options line 64 optional
        710 Paths wlist 64 required
        711 Remapped wlist 64 optional
        712 Ignored wlist 64 optional
        713 View wlist 64 optional
        714 ChangeView llist 64 always
        715 ParentView word 0 required
        NNN TestField word 32 optional
        NNN NewField llist 32 optional
        NNN DifferentField word 32 optional

Words:
        Paths 2
        Remapped 2
        View 2

Formats:
        Update 0 L
        Access 0 L

Values:
        Options allsubmit/ownersubmit,unlocked/locked,toparent/notoparent,fromparent/nofromparent,mergedown/mergeany
        ParentView noinherit/inherit

Presets:
        ParentView inherit

Openable:
        Owner isolate
        Name isolate
        Parent isolate
        Type isolate
        Description isolate
        Options isolate
        ParentView isolate
        Paths propagate
        Remapped propagate
        Ignored propagate

Maxwords:
        Paths 3

Comments:
        # A Perforce Stream Specification.
        #
        #  Stream:       The stream field is unique and specifies the depot path.
        #  Update:       The date the specification was last changed.
        #  Access:       The date the specification was originally created.
        #  Owner:        The user who created this stream.
        #  Name:         A short title which may be updated.
        #  Parent:       The parent of this stream, or 'none' if Type is mainline.
        #  Type:         Type of stream provides clues for commands run
        #                between stream and parent.  Five types include 'mainline',
        #                'release', 'development' (default), 'virtual' and 'task'.
        #  Description:  A short description of the stream (optional).
        #  Options:      Stream Options:
        #                       allsubmit/ownersubmit [un]locked
        #                       [no]toparent [no]fromparent mergedown/mergeany
        #  Paths:        Identify paths in the stream and how they are to be
        #                generated in resulting clients of this stream.
        #                Path types are share/isolate/import/import+/exclude.
        #  Remapped:     Remap a stream path in the resulting client view.
        #  Ignored:      Ignore a stream path in the resulting client view.
        #
        # Use 'p4 help stream' to see more about stream specifications and command.";

        [TestMethod()]
        public void StreamCommandEdit()
        {
            string uri = configuration.ServerPort;
            string user = "admin";
            string ws_client = "admin_space";

            Process p4d = null;
            try
            {
                p4d = Utilities.DeployP4TestServer(TestDir, 21, (Utilities.CheckpointType)0);
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

                    StreamOption options = new StreamOption();
                    string description = "Stream commands - edit";
                    string ownerName = "admin";
                    string name = "streamcmds-edit";
                    string id = "//Rocket/streamcmds-edit";
                    StreamType type = StreamType.Mainline;
                    ViewMap Paths = new ViewMap()
                    {
                        { new MapEntry(MapType.Share, new DepotPath("..."), null) }
                    };

                    // Assign values to stream properties
                    Stream streamCommandsEdit = new Stream();
                    streamCommandsEdit.Id = id;
                    streamCommandsEdit.Type = type;
                    streamCommandsEdit.Parent = new DepotPath("none");
                    streamCommandsEdit.Options = options;
                    streamCommandsEdit.Name = name;
                    streamCommandsEdit.Name = name;
                    streamCommandsEdit.OwnerName = ownerName;
                    streamCommandsEdit.Description = description;
                    streamCommandsEdit.Paths = Paths;

                    rep.CreateStream(streamCommandsEdit);

                    Changelist c = rep.NewChangelist();
                    c.Description = "New changelist for stream commands edit";
                    c.Files = null;

                    Changelist editChange = rep.SaveChangelist(c, null);

                    con.Client.Stream = id;
                    editChange.Stream = id;
                    rep.UpdateClient(con.Client);

                    Options streamEditOptions = new Options(EditFilesCmdFlags.StreamEdit, editChange.Id, null);
                    con.Client.EditFiles(streamEditOptions);

                    rep.UpdateChangelist(editChange);
                    Options submitCmdOptions = new Options();
                    submitCmdOptions["-c"] = editChange.Id.ToString();
                    rep.Connection.Client.SubmitFiles(submitCmdOptions, null);

                    Changelist submittedChange = rep.GetChangelist(editChange.Id);

                    Assert.IsFalse(submittedChange.Pending);
                    Assert.AreEqual(id, submittedChange.Stream);
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
            }
        }

        [TestMethod()]
        public void StreamCommandRevert()
        {
            string uri = configuration.ServerPort;
            string user = "admin";
            string ws_client = "admin_space";

            Process p4d = null;
            try
            {
                p4d = Utilities.DeployP4TestServer(TestDir, 21, (Utilities.CheckpointType)0);
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

                    StreamOption options = new StreamOption();
                    string description = "Stream commands - revert";
                    string ownerName = "admin";
                    string name = "streamcmds-revert";
                    string id = "//Rocket/streamcmds-revert";
                    StreamType type = StreamType.Mainline;
                    ViewMap Paths = new ViewMap()
                    {
                        { new MapEntry(MapType.Share, new DepotPath("..."), null) }
                    };

                    // Assign values to stream properties
                    Stream streamCommandsRevert = new Stream();
                    streamCommandsRevert.Id = id;
                    streamCommandsRevert.Type = type;
                    streamCommandsRevert.Parent = new DepotPath("none");
                    streamCommandsRevert.Options = options;
                    streamCommandsRevert.Name = name;
                    streamCommandsRevert.Name = name;
                    streamCommandsRevert.OwnerName = ownerName;
                    streamCommandsRevert.Description = description;
                    streamCommandsRevert.Paths = Paths;

                    rep.CreateStream(streamCommandsRevert);

                    Changelist c = rep.NewChangelist();
                    c.Description = "New changelist for stream commands revert";
                    c.Files = null;

                    Changelist editChange = rep.SaveChangelist(c, null);

                    con.Client.Stream = id;
                    editChange.Stream = id;
                    rep.UpdateClient(con.Client);

                    Options streamEditOptions = new Options(EditFilesCmdFlags.StreamEdit, editChange.Id, null);
                    con.Client.EditFiles(streamEditOptions);

                    IList<File> opened = rep.GetOpenedFiles(null, new Options());
                    Assert.AreEqual(id, opened[0].Stream.ToString());

                    Options streamRevertOptions = new Options(RevertFilesCmdFlags.StreamRevert, editChange.Id);
                    con.Client.RevertFiles(streamRevertOptions);

                    // After revert the stream should no longer be in opened state
                    opened = rep.GetOpenedFiles(null, new Options());
                    Assert.AreNotEqual(id, opened[0].Stream.ToString());
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
            }
        }

        [TestMethod()]
        public void StreamCommandResolve()
        {
            string uri = configuration.ServerPort;
            string user1 = "admin";
            string ws_client1 = "admin_space";
            string user2 = "alex";
            string ws_client2 = "alex_space";

            Process p4d = null;
            try
            {
                p4d = Utilities.DeployP4TestServer(TestDir, 8, (Utilities.CheckpointType)0);
                Server server = new Server(new ServerAddress(uri));

                Repository rep = new Repository(server);

                using (Connection con = rep.Connection)
                {

                    con.UserName = user1;
                    con.Client = new Client();
                    con.Client.Name = ws_client1;

                    bool connected = con.Connect(null);
                    Assert.IsTrue(connected);
                    Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                    #region createStream
                    StreamOption options = new StreamOption();
                    string description = "Stream commands - resolve";
                    string ownerName = "admin";
                    string name = "streamcmds-resolve";
                    string id = "//Rocket/streamcmds-resolve";
                    StreamType type = StreamType.Mainline;
                    ViewMap Paths = new ViewMap()
                    {
                        { new MapEntry(MapType.Share, new DepotPath("..."), null) }
                    };

                    // Assign values to stream properties
                    Stream streamCommandsResolve = new Stream();
                    streamCommandsResolve.Id = id;
                    streamCommandsResolve.Type = type;
                    streamCommandsResolve.Parent = new DepotPath("none");
                    streamCommandsResolve.Options = options;
                    streamCommandsResolve.Name = name;
                    streamCommandsResolve.Name = name;
                    streamCommandsResolve.OwnerName = ownerName;
                    streamCommandsResolve.Description = description;
                    streamCommandsResolve.Paths = Paths;

                    rep.CreateStream(streamCommandsResolve);
                    #endregion createStream

                    #region openStreamClient1
                    Changelist c1 = rep.NewChangelist();
                    c1.Description = "Edit stream by client1";
                    c1.Files = null;

                    Changelist editChange1 = rep.SaveChangelist(c1, null);

                    con.Client.Stream = id;
                    editChange1.Stream = id;
                    rep.UpdateClient(con.Client);

                    Options streamEditOptions1 = new Options(EditFilesCmdFlags.StreamEdit, editChange1.Id, null);
                    // Opens the Stream for edit
                    con.Client.EditFiles(streamEditOptions1);
                    // Update the changelist with Stream field
                    rep.UpdateChangelist(editChange1);


                    con.Disconnect();
                    #endregion openStreamClient1

                    #region editStreamClient2

                    con.UserName = user2;
                    con.Client = new Client();
                    con.Client.Name = ws_client2;

                    connected = con.Connect(null);
                    Assert.IsTrue(connected);
                    Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                    Stream readStream = rep.GetStream(id);
                    readStream.Description = "Stream commands - resolve. Edited by client2";
                    rep.UpdateStream(readStream);
                    Stream readStream1 = rep.GetStream(id);
                    con.Disconnect();

                    #endregion editStreamClient2


                    #region resolveStreamClient1

                    con.UserName = user1;
                    con.Client = new Client();
                    con.Client.Name = ws_client1;

                    connected = con.Connect(null);
                    Assert.IsTrue(connected);
                    Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                    Options submitCmdOptions = new Options();
                    submitCmdOptions["-c"] = editChange1.Id.ToString();
                    // Resolve command when used with -So, cannot take changelist, it resolves "opened" stream,
                    // But we still Supply it here, as it needs changelist.
                    Options resolveOptions = new Options(ResolveFilesCmdFlags.ResolveStream, editChange1.Id);
                    try
                    {
                        con.Client.ResolveStream(resolveOptions);
                    }
                    catch (P4Exception e)
                    {
                        Assert.AreEqual("Stream needs resolve action: -ay/-at", e.Message);
                    }

                    resolveOptions = new Options(ResolveFilesCmdFlags.ResolveStream | ResolveFilesCmdFlags.AutomaticYoursMode, editChange1.Id);
                    Assert.IsTrue(con.Client.ResolveStream(resolveOptions));
                    rep.Connection.Client.SubmitFiles(submitCmdOptions, null);

                    Changelist submittedChange = rep.GetChangelist(editChange1.Id);
                    #endregion resolveStreamClient1
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
            }
        }

        [TestMethod()]
        public void StreamCommandStreamLog()
        {
            string uri = configuration.ServerPort;
            string user = "admin";
            string ws_client = "admin_space";

            Process p4d = null;
            try
            {
                p4d = Utilities.DeployP4TestServer(TestDir, 21, (Utilities.CheckpointType)0);
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

                    StreamOption options = new StreamOption();
                    string description = "Stream commands - streamLog";
                    string ownerName = "admin";
                    string name = "streamcmds-streamLog";
                    string id = "//Rocket/streamcmds-streamLog";
                    StreamType type = StreamType.Mainline;
                    ViewMap Paths = new ViewMap()
                    {
                        { new MapEntry(MapType.Share, new DepotPath("..."), null) }
                    };

                    // Assign values to stream properties
                    Stream streamCommandsStreamLog = new Stream();
                    streamCommandsStreamLog.Id = id;
                    streamCommandsStreamLog.Type = type;
                    streamCommandsStreamLog.Parent = new DepotPath("none");
                    streamCommandsStreamLog.Options = options;
                    streamCommandsStreamLog.Name = name;
                    streamCommandsStreamLog.Name = name;
                    streamCommandsStreamLog.OwnerName = ownerName;
                    streamCommandsStreamLog.Description = description;
                    streamCommandsStreamLog.Paths = Paths;

                    rep.CreateStream(streamCommandsStreamLog);

                    Changelist c = rep.NewChangelist();
                    c.Description = "New changelist for stream commands streamLog";
                    c.Files = null;

                    Changelist streamLogChange = rep.SaveChangelist(c, null);

                    con.Client.Stream = id;
                    streamLogChange.Stream = id;
                    rep.UpdateClient(con.Client);

                    Options streamStreamLogOptions = new Options(EditFilesCmdFlags.StreamEdit, streamLogChange.Id, null);
                    con.Client.EditFiles(streamStreamLogOptions);

                    rep.UpdateChangelist(streamLogChange);
                    Options submitCmdOptions = new Options();
                    submitCmdOptions["-c"] = streamLogChange.Id.ToString();
                    rep.Connection.Client.SubmitFiles(submitCmdOptions, null);

                    Changelist submittedChange = rep.GetChangelist(streamLogChange.Id);

                    Assert.IsFalse(submittedChange.Pending);
                    Assert.AreEqual(id, submittedChange.Stream);

                    string[] streams = { "//Rocket/streamcmds-streamLog", "//flow/D1", "//flow/D2" };

                    Dictionary<string, List<StreamLog>> streamLog = rep.GetStreamLog(streams, new Options());
                    Assert.IsNotNull(streamLog);
                    Assert.AreEqual("edit", streamLog["//Rocket/streamcmds-streamLog"][0].Action);
                }
            }
            finally
            {
                Utilities.RemoveTestServer(p4d, TestDir);
            }
        }
    }
}
