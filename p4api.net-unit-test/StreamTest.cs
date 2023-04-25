using Perforce.P4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using NLog;

namespace p4api.net.unit.test
{
    /// <summary>
    ///This is a test class for StreamTest and is intended
    ///to contain all StreamTest Unit Tests
    ///</summary>
    [TestClass()]
#if NET462
    [DeploymentItem("x64", "x64")]
    [DeploymentItem("x86", "x86")]
#endif
    public class StreamTest
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void SetupTest()
        {
            Utilities.LogTestStart(TestContext);
        }
        [TestCleanup]
        public void CleanupTest()
        {
            Utilities.LogTestFinish(TestContext);
        }

        static string id = "//projectX/dev";
        static DateTime updated = new DateTime(2011, 04, 10);
        static DateTime accessed = new DateTime(2011, 04, 10);
        static string ownername = "John Smith";
        static string name = "ProjectX development";
        static PathSpec parent = new DepotPath("//projectX/main");
        static PathSpec baseparent = null;
        static StreamType type = StreamType.Development;
        static string description = "development stream for experimental work on projectX";
        static string firmerthanparent = "n/a";
        static string changeflowstoparent = "false";
        static string changeflowsfromparent = "false";
        static StreamOption options = new StreamOptionEnum(StreamOption.OwnerSubmit | StreamOption.Locked | StreamOption.NoToParent | StreamOption.NoFromParent);
        static ViewMap components = null;
        static MapEntry component1 = new MapEntry(MapType.Readonly, new DepotPath("dirB"), new DepotPath("//streamsDepot/streamB"));
        static MapEntry component2 = new MapEntry(MapType.Writeall, new DepotPath("dirC"), new DepotPath("//streamsDepot/streamC"));
        static MapEntry component3 = new MapEntry(MapType.WriteImportPlus, new DepotPath("dirD"), new DepotPath("//streamsDepot/streamD"));
        static ViewMap paths = null;
        static MapEntry path = new MapEntry(MapType.Share, new DepotPath("//projectX/main"), null);
        static ViewMap remapped = null;
        static MapEntry remap = new MapEntry(MapType.Include, new DepotPath("//projectX/main"), null);
        static ViewMap ignored = null;
        static ViewMap view = null;
        static ParentView parentView = ParentView.Inherit;
        static MapEntry ig = new MapEntry(MapType.Include, new DepotPath("//projectX/extra"), null);
        static FormSpec spec = new FormSpec(new List<SpecField>(), new Dictionary<string, string>(), new List<string>(), new List<string>(), new Dictionary<string, string>(),
            new Dictionary<string, string>(), null, null, "here are the comments");
        static Dictionary<string, object> customfields = new Dictionary<string, object>()
        {
            {"testField1", "test value1"},
            {"testField2", "test value2"},
            {"testField3", "test value3"},
            {"testField4", "test value4"},
            {"listField1", new List<string>{"multiline1","multiline2","multiline3"} },
            {"testField5", "test value5"},
        };
        static ViewMap changeview = null;


        static Stream target = null;

        static void setTarget()
        {
            components = new ViewMap();
            components.Add(component1);
            components.Add(component2);
            components.Add(component3);

            paths = new ViewMap();
            paths.Add(path);
            remapped = new ViewMap();
            remapped.Add(remap);
            ignored = new ViewMap();
            ignored.Add(ig);
            target = new Stream(id, updated, accessed,
            ownername, name, parent, baseparent, type, description,
            options, firmerthanparent, changeflowstoparent,
            changeflowsfromparent, components, paths, remapped, ignored, view,
            changeview, spec, customfields, parentView);
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
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for Accessed
        ///</summary>
        [TestMethod()]
        public void AccessedTest()
        {
            DateTime expected = new DateTime(2011, 03, 01);
            setTarget();
            Assert.AreEqual(target.Accessed, new DateTime(2011, 04, 10));
            target.Accessed = expected;
            DateTime actual;
            actual = target.Accessed;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void ParseTest()
        {
            string streamForm = "Stream:\t{0}\r\n" + Environment.NewLine + Environment.NewLine
                + "Update:\t{1}" + Environment.NewLine + Environment.NewLine
                + "Access:\t{2}" + Environment.NewLine + Environment.NewLine
                + "Owner:\t{3}" + Environment.NewLine + Environment.NewLine
                + "Name:\t{4}" + Environment.NewLine + Environment.NewLine
                + "Parent:\t{5}" + Environment.NewLine + Environment.NewLine
                + "Type:\t{6}" + Environment.NewLine + Environment.NewLine
                + "Description:" + Environment.NewLine
                + "\t{7}" + Environment.NewLine + Environment.NewLine
                + "Options:\t{8}" + Environment.NewLine + Environment.NewLine
                + "Components:" + Environment.NewLine
                + "\t{9}" + Environment.NewLine + Environment.NewLine
                + "\t{10}" + Environment.NewLine + Environment.NewLine
                + "\t{11}" + Environment.NewLine + Environment.NewLine
                + "Paths:" + Environment.NewLine
                + "\t{12}" + Environment.NewLine
                + "\t{13}" + Environment.NewLine + Environment.NewLine
                + "Remapped:" + Environment.NewLine
                + "\t{14}" + Environment.NewLine + Environment.NewLine
                + "Ignored:" + Environment.NewLine
                + "\t{15}" + Environment.NewLine + Environment.NewLine
                + "ParentView:" + Environment.NewLine
                + "\t{16}" + Environment.NewLine;

            String streamData = String.Format(streamForm, "//User/test",
                "", "", "user1", "test", "//User/user1_stream", "development",
                "created by user1", "allsubmit unlocked toparent fromparent",
                "readonly dirB //streamsDepot/streamB",
                "writeall dirC //streamsDepot/streamC",
                "writeimport+ dirD //streamsDepot/streamD",
                "share ... ## In-line comment", "## This is new line comment", "share/... remapped/... #3rd part comment", "Rocket/GUI/core/gui/res/...", "noinherit");

            ViewMap Paths = new ViewMap() { new MapEntry(MapType.Share, new ClientPath("..."), new ClientPath("")) };
            Stream stream = new Stream();
            stream.Parse(streamData);
            Assert.AreEqual(stream.Description, "created by user1");
            Assert.AreEqual(stream.OwnerName, "user1");

            // Assert Components
            Assert.AreEqual(stream.Components[0].Type, MapType.Readonly);
            Assert.AreEqual(stream.Components[0].Left.ToString(), "dirB");
            Assert.AreEqual(stream.Components[0].Right.ToString(), "//streamsDepot/streamB");

            Assert.AreEqual(stream.Components[1].Type, MapType.Writeall);
            Assert.AreEqual(stream.Components[1].Left.ToString(), "dirC");
            Assert.AreEqual(stream.Components[1].Right.ToString(), "//streamsDepot/streamC");

            Assert.AreEqual(stream.Components[2].Type, MapType.WriteImportPlus);
            Assert.AreEqual(stream.Components[2].Left.ToString(), "dirD");
            Assert.AreEqual(stream.Components[2].Right.ToString(), "//streamsDepot/streamD");


            // Assert Paths
            Assert.AreEqual(stream.Paths[0].Left.ToString(), "...");
            Assert.AreEqual(stream.Paths[0].Comment.ToString(), "## In-line comment");
            Assert.AreEqual(stream.Paths[1].Comment.ToString(), "## This is new line comment");

            Assert.AreEqual(stream.Id, "//User/test");
            Assert.AreEqual(stream.Name, "test");
            Assert.AreEqual(stream.ParentView.ToString(), "NoInherit");
        }


        /// <summary>
        ///A test for Description
        ///</summary>
        [TestMethod()]
        public void DescriptionTest()
        {
            string expected = "wrong string";
            setTarget();
            Assert.AreEqual(target.Description, "development stream for experimental work on projectX");
            target.Description = expected;
            string actual;
            actual = target.Description;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Ignored
        ///</summary>
        [TestMethod()]
        public void IgnoredTest()
        {
            MapEntry fs = new MapEntry(MapType.Include, new DepotPath("//projectX/extra"), null);
            setTarget();
            ViewMap actual;
            actual = target.Ignored;
            Assert.IsTrue(actual.Contains(fs));
        }

        /// <summary>
        ///A test for Name
        ///</summary>
        [TestMethod()]
        public void NameTest()
        {
            string expected = "wrong string";
            setTarget();
            Assert.AreEqual(target.Name, "ProjectX development");
            target.Name = expected;
            string actual;
            actual = target.Name;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Options
        ///</summary>
        [TestMethod()]
        public void OptionsTest()
        {
            setTarget();
            StreamOption actual;
            actual = target.Options;
            Assert.IsTrue(actual.HasFlag(StreamOption.OwnerSubmit));
            Assert.IsTrue(actual.HasFlag(StreamOption.Locked));
            Assert.IsTrue(actual.HasFlag(StreamOption.NoToParent));
            Assert.IsTrue(actual.HasFlag(StreamOption.NoFromParent));
        }

        /// <summary>
        ///A test for OwnerName
        ///</summary>
        [TestMethod()]
        public void OwnerNameTest()
        {
            string expected = "wrong string";
            setTarget();
            Assert.AreEqual(target.OwnerName, "John Smith");
            target.OwnerName = expected;
            string actual;
            actual = target.OwnerName;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Parent
        ///</summary>
        [TestMethod()]
        public void ParentTest()
        {
            PathSpec expected = new DepotPath("//Wrong/main");
            setTarget();
            Assert.AreEqual(target.Parent, new DepotPath("//projectX/main"));
            target.Parent = expected;
            PathSpec actual;
            actual = target.Parent;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
		///A test for Components
		///</summary>
		[TestMethod()]
        public void ComponentsTest()
        {
            setTarget();
            ViewMap actual = actual = target.Components;
            MapEntry sp1 = new MapEntry(MapType.Readonly, new DepotPath("dirB"), new DepotPath("//streamsDepot/streamB"));
            MapEntry sp2 = new MapEntry(MapType.Writeall, new DepotPath("dirC"), new DepotPath("//streamsDepot/streamC"));
            MapEntry sp3 = new MapEntry(MapType.WriteImportPlus, new DepotPath("dirD"), new DepotPath("//streamsDepot/streamD"));

            Assert.IsTrue(actual.Contains(sp1));
            Assert.IsTrue(actual.Contains(sp2));
            Assert.IsTrue(actual.Contains(sp3));
        }

        /// <summary>
        ///A test for Paths
        ///</summary>
        [TestMethod()]
        public void PathsTest()
        {
            MapEntry sp = new MapEntry(MapType.Share, new DepotPath("//projectX/main"), null);
            setTarget();
            ViewMap actual;
            actual = target.Paths;
            Assert.IsTrue(actual.Contains(sp));
        }

        /// <summary>
        ///A test for Remapped
        ///</summary>
        [TestMethod()]
        public void RemappedTest()
        {
            MapEntry re = new MapEntry(MapType.Include, new DepotPath("//projectX/main"), null);
            setTarget();
            ViewMap actual;
            actual = target.Remapped;
            Assert.IsTrue(actual.Contains(re));
        }

        /// <summary>
        ///A test for Spec
        ///</summary>
        [TestMethod()]
        public void SpecTest()
        {
            setTarget();
            string actual = target.Spec.Comments;
            string expected = "here are the comments";
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for StreamId
        ///</summary>
        [TestMethod()]
        public void StreamIdTest()
        {
            string expected = "//projectX/dev";
            string actual;
            setTarget();
            actual = target.Id;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Type
        ///</summary>
        [TestMethod()]
        public void TypeTest()
        {
            StreamType expected = StreamType.Development;
            StreamType actual;
            setTarget();
            actual = target.Type;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Updated
        ///</summary>
        [TestMethod()]
        public void UpdatedTest()
        {
            DateTime expected = new DateTime(2011, 03, 01);
            setTarget();
            Assert.AreEqual(target.Updated, new DateTime(2011, 04, 10));
            target.Updated = expected;
            DateTime actual;
            actual = target.Updated;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test fot to String method
        ///</summary>
        [TestMethod()]
        public void ToStringTest()
        {
            string expected = "Stream:\t//projectX/dev" + Environment.NewLine + Environment.NewLine
                              + "Update:\t2011/04/10 00:00:00" + Environment.NewLine + Environment.NewLine
                              + "Access:\t2011/04/10 00:00:00" + Environment.NewLine + Environment.NewLine
                              + "Owner:\tJohn Smith" + Environment.NewLine + Environment.NewLine
                              + "Name:\tProjectX development" + Environment.NewLine + Environment.NewLine
                              + "Parent:\t//projectX/main" + Environment.NewLine + Environment.NewLine
                              + "Type:\tdevelopment" + Environment.NewLine + Environment.NewLine
                              + "Description:" + Environment.NewLine + "\tdevelopment stream for experimental work on projectX" + Environment.NewLine + Environment.NewLine
                              + "Options:\townersubmit locked notoparent nofromparent mergedown" + Environment.NewLine + Environment.NewLine
                              + "ParentView:\tinherit" + Environment.NewLine + Environment.NewLine
                              + "Components:" + Environment.NewLine + "\treadonly dirB //streamsDepot/streamB" + Environment.NewLine
                                + "\twriteall dirC //streamsDepot/streamC" + Environment.NewLine
                                + "\twriteimport+ dirD //streamsDepot/streamD" + Environment.NewLine + Environment.NewLine
                              + "Paths:" + Environment.NewLine + "\tshare //projectX/main" + Environment.NewLine + Environment.NewLine
                              + "Remapped:" + Environment.NewLine + "\t//projectX/main" + Environment.NewLine + Environment.NewLine
                              + "Ignored:" + Environment.NewLine + "\t//projectX/extra" + Environment.NewLine + Environment.NewLine
                              + "testField1:\ttest value1" + Environment.NewLine + Environment.NewLine
                              + "testField2:\ttest value2" + Environment.NewLine + Environment.NewLine
                              + "testField3:\ttest value3" + Environment.NewLine + Environment.NewLine
                              + "testField4:\ttest value4" + Environment.NewLine + Environment.NewLine
                              + "listField1:" + Environment.NewLine + "\tmultiline1" + Environment.NewLine
                                                                    + "\tmultiline2" + Environment.NewLine
                                                                    + "\tmultiline3" + Environment.NewLine + Environment.NewLine
                              + "testField5:\ttest value5" + Environment.NewLine;
            setTarget();
            string actual = target.ToString();
            Assert.AreEqual(expected, actual);
        }
    }
}
