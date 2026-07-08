using Perforce.P4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using NLog;

namespace p4api.net.unit.test
{
    /// <summary>
    ///This is a test class for GroupTest and is intended
    ///to contain all GroupTest Unit Tests
    ///</summary>
    [TestClass()]
    public class GroupTest
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

        private static String GroupSpec =
                "Group:\tTest" + Environment.NewLine + Environment.NewLine
                + "MaxResults:\t100" + Environment.NewLine + Environment.NewLine
                + "MaxScanRows:\t10" + Environment.NewLine + Environment.NewLine
                + "MaxLockTime:\t20" + Environment.NewLine + Environment.NewLine
                + "MaxOpenFiles:\t50" + Environment.NewLine + Environment.NewLine
                + "Timeout:\t30" + Environment.NewLine + Environment.NewLine
                + "IdleTimeout:\t35" + Environment.NewLine + Environment.NewLine
                + "PasswordTimeout:\t40" + Environment.NewLine + Environment.NewLine
                + "Subgroups:" + Environment.NewLine
                + "\tSG1" + Environment.NewLine
                + "\tSG2" + Environment.NewLine + Environment.NewLine
                + "Owners:" + Environment.NewLine
                + "\tAlpha" + Environment.NewLine
                + "\tBeta" + Environment.NewLine + Environment.NewLine
                + "Users:" + Environment.NewLine
                + "\tAlice" + Environment.NewLine
                + "\tBob" + Environment.NewLine + Environment.NewLine
                + "Description:\tTest Description" + Environment.NewLine + Environment.NewLine
                + "MaxMemory:\t60" + Environment.NewLine + Environment.NewLine;

        // Timeout fields set to "unlimited" (-1 sentinel)
        private static String GroupSpecUnlimited =
                "Group:\tTest" + Environment.NewLine + Environment.NewLine
                + "MaxResults:\t100" + Environment.NewLine + Environment.NewLine
                + "MaxScanRows:\t10" + Environment.NewLine + Environment.NewLine
                + "MaxLockTime:\t20" + Environment.NewLine + Environment.NewLine
                + "MaxOpenFiles:\t50" + Environment.NewLine + Environment.NewLine
                + "Timeout:\tunlimited" + Environment.NewLine + Environment.NewLine
                + "IdleTimeout:\tunlimited" + Environment.NewLine + Environment.NewLine
                + "PasswordTimeout:\tunlimited" + Environment.NewLine + Environment.NewLine
                + "Subgroups:" + Environment.NewLine
                + "\tSG1" + Environment.NewLine
                + "\tSG2" + Environment.NewLine + Environment.NewLine
                + "Owners:" + Environment.NewLine
                + "\tAlpha" + Environment.NewLine
                + "\tBeta" + Environment.NewLine + Environment.NewLine
                + "Users:" + Environment.NewLine
                + "\tAlice" + Environment.NewLine
                + "\tBob" + Environment.NewLine + Environment.NewLine
                + "Description:\tTest Description" + Environment.NewLine + Environment.NewLine
                + "MaxMemory:\t60" + Environment.NewLine + Environment.NewLine;

        // Timeout fields set to "unset" (0 sentinel) for parse testing
        private static String GroupSpecUnset =
                "Group:\tTest" + Environment.NewLine + Environment.NewLine
                + "MaxResults:\t100" + Environment.NewLine + Environment.NewLine
                + "MaxScanRows:\t10" + Environment.NewLine + Environment.NewLine
                + "MaxLockTime:\t20" + Environment.NewLine + Environment.NewLine
                + "MaxOpenFiles:\t50" + Environment.NewLine + Environment.NewLine
                + "Timeout:\tunset" + Environment.NewLine + Environment.NewLine
                + "IdleTimeout:\tunset" + Environment.NewLine + Environment.NewLine
                + "PasswordTimeout:\tunset" + Environment.NewLine + Environment.NewLine
                + "Subgroups:" + Environment.NewLine
                + "\tSG1" + Environment.NewLine
                + "\tSG2" + Environment.NewLine + Environment.NewLine
                + "Owners:" + Environment.NewLine
                + "\tAlpha" + Environment.NewLine
                + "\tBeta" + Environment.NewLine + Environment.NewLine
                + "Users:" + Environment.NewLine
                + "\tAlice" + Environment.NewLine
                + "\tBob" + Environment.NewLine + Environment.NewLine
                + "Description:\tTest Description" + Environment.NewLine + Environment.NewLine
                + "MaxMemory:\t60" + Environment.NewLine + Environment.NewLine;

        // Max* fields set to "unlimited" (-1 sentinel), timeout fields positive
        private static String GroupSpecMaxUnlimited =
                "Group:\tTest" + Environment.NewLine + Environment.NewLine
                + "MaxResults:\tunlimited" + Environment.NewLine + Environment.NewLine
                + "MaxScanRows:\tunlimited" + Environment.NewLine + Environment.NewLine
                + "MaxLockTime:\tunlimited" + Environment.NewLine + Environment.NewLine
                + "MaxOpenFiles:\tunlimited" + Environment.NewLine + Environment.NewLine
                + "Timeout:\t30" + Environment.NewLine + Environment.NewLine
                + "IdleTimeout:\t35" + Environment.NewLine + Environment.NewLine
                + "PasswordTimeout:\t40" + Environment.NewLine + Environment.NewLine
                + "Subgroups:" + Environment.NewLine
                + "\tSG1" + Environment.NewLine
                + "\tSG2" + Environment.NewLine + Environment.NewLine
                + "Owners:" + Environment.NewLine
                + "\tAlpha" + Environment.NewLine
                + "\tBeta" + Environment.NewLine + Environment.NewLine
                + "Users:" + Environment.NewLine
                + "\tAlice" + Environment.NewLine
                + "\tBob" + Environment.NewLine + Environment.NewLine
                + "Description:\tTest Description" + Environment.NewLine + Environment.NewLine
                + "MaxMemory:\tunlimited" + Environment.NewLine + Environment.NewLine;

        // Max* fields set to "unset" (0 sentinel) for parse testing
        private static String GroupSpecMaxUnset =
                "Group:\tTest" + Environment.NewLine + Environment.NewLine
                + "MaxResults:\tunset" + Environment.NewLine + Environment.NewLine
                + "MaxScanRows:\tunset" + Environment.NewLine + Environment.NewLine
                + "MaxLockTime:\tunset" + Environment.NewLine + Environment.NewLine
                + "MaxOpenFiles:\tunset" + Environment.NewLine + Environment.NewLine
                + "Timeout:\t30" + Environment.NewLine + Environment.NewLine
                + "IdleTimeout:\t35" + Environment.NewLine + Environment.NewLine
                + "PasswordTimeout:\t40" + Environment.NewLine + Environment.NewLine
                + "Subgroups:" + Environment.NewLine
                + "\tSG1" + Environment.NewLine
                + "\tSG2" + Environment.NewLine + Environment.NewLine
                + "Owners:" + Environment.NewLine
                + "\tAlpha" + Environment.NewLine
                + "\tBeta" + Environment.NewLine + Environment.NewLine
                + "Users:" + Environment.NewLine
                + "\tAlice" + Environment.NewLine
                + "\tBob" + Environment.NewLine + Environment.NewLine
                + "Description:\tTest Description" + Environment.NewLine + Environment.NewLine
                + "MaxMemory:\tunset" + Environment.NewLine + Environment.NewLine;


        /// <summary>
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void ParseTest()
        {
            Group target = new Group();
            string spec = GroupSpec; 
            bool expected = true; 
            bool actual;
            actual = target.Parse(spec);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(target.TimeOut, 30);
            Assert.AreEqual(target.OwnerNames[0], "Alpha");

            Assert.AreEqual(target.Id, "Test");
            Assert.AreEqual(target.Description, "Test Description");
            Assert.AreEqual(target.MaxResults, 100);
            Assert.AreEqual(target.MaxScanRows, 10);
            Assert.AreEqual(target.MaxLockTime, 20);
            Assert.AreEqual(target.TimeOut, 30);
            Assert.AreEqual(target.IdleTimeout, 35);
            Assert.AreEqual(target.PasswordTimeout, 40);
            Assert.AreEqual(target.MaxOpenFiles, 50);
            Assert.AreEqual(target.MaxMemory, 60);
            Assert.AreEqual(target.OwnerNames[0], "Alpha");
            Assert.AreEqual(target.OwnerNames[1], "Beta");
            Assert.AreEqual(target.UserNames[0], "Alice");
            Assert.AreEqual(target.UserNames[1], "Bob");
            Assert.AreEqual(target.SubGroups[0], "SG1");
            Assert.AreEqual(target.SubGroups[1], "SG2");
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [TestMethod()]
        public void ToStringTest()
        {
            Group target = new Group();
            target.Id = "Test";
            target.Description = "Test Description";
            target.MaxResults = 100;
            target.MaxScanRows = 10;
            target.MaxLockTime = 20;
            target.TimeOut = 30;
            target.IdleTimeout = 35;
            target.PasswordTimeout = 40;
            target.MaxOpenFiles = 50;
            target.MaxMemory = 60;
            target.OwnerNames = new List<string>() { "Alpha", "Beta" };
            target.UserNames = new List<string>() { "Alice", "Bob" };
            target.SubGroups = new List<string>() { "SG1", "SG2" };

            string expected = GroupSpec.Trim(); // TODO: Initialize to an appropriate value
            string actual;
            actual = target.ToString().Trim();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for FromGroupCmdTaggedOutput
        ///</summary>
        [TestMethod()]
        public void FromGroupCmdTaggedOutputTest()
        {
            Group target = new Group();
            TaggedObject objectInfo = new TaggedObject();

            objectInfo["Group"] = "Test";
            objectInfo["Description"] = "Test Description";
            objectInfo["MaxResults"] = "100";
            objectInfo["MaxScanRows"] = "10";
            objectInfo["MaxLockTime"] = "20";
            objectInfo["Timeout"] = "30";
            objectInfo["IdleTimeout"] = "35";
            objectInfo["PasswordTimeout"] = "40";
            objectInfo["MaxOpenFiles"] = "50";
            objectInfo["MaxMemory"] = "60";
            objectInfo["Users0"] = "Alice";
            objectInfo["Users1"] = "Bob";
            objectInfo["Owners0"] = "Alpha";
            objectInfo["Owners1"] = "Beta";
            objectInfo["Subgroups0"] = "SG1";
            objectInfo["Subgroups1"] = "SG2";

            target.FromGroupCmdTaggedOutput(objectInfo);

            Assert.AreEqual("Test", target.Id);
            Assert.AreEqual("Test Description", target.Description);
            Assert.AreEqual(100, target.MaxResults);
            Assert.AreEqual(10, target.MaxScanRows);
            Assert.AreEqual(20, target.MaxLockTime);
            Assert.AreEqual(50, target.MaxOpenFiles);
            Assert.AreEqual(60, target.MaxMemory);
            Assert.AreEqual(30, target.TimeOut);
            Assert.AreEqual(35, target.IdleTimeout);
            Assert.AreEqual(40, target.PasswordTimeout);
            Assert.AreEqual("Alpha", target.OwnerNames[0]);
            Assert.AreEqual("Beta", target.OwnerNames[1]);
            Assert.AreEqual("Alice", target.UserNames[0]);
            Assert.AreEqual("Bob", target.UserNames[1]);
            Assert.AreEqual("SG1", target.SubGroups[0]);
            Assert.AreEqual("SG2", target.SubGroups[1]);
        }

        /// <summary>
        /// Verifies that "unlimited" tagged output maps to -1 for timeout fields.
        /// </summary>
        [TestMethod()]
        public void FromGroupCmdTaggedOutputUnlimitedTest()
        {
            Group target = new Group();
            TaggedObject objectInfo = new TaggedObject();

            objectInfo["Group"] = "Test";
            objectInfo["Timeout"] = "unlimited";
            objectInfo["IdleTimeout"] = "unlimited";
            objectInfo["PasswordTimeout"] = "unlimited";

            target.FromGroupCmdTaggedOutput(objectInfo);

            Assert.AreEqual(-1, target.TimeOut);
            Assert.AreEqual(-1, target.IdleTimeout);
            Assert.AreEqual(-1, target.PasswordTimeout);
        }

        /// <summary>
        /// Verifies that "unset" tagged output maps to 0 for timeout fields.
        /// </summary>
        [TestMethod()]
        public void FromGroupCmdTaggedOutputUnsetTest()
        {
            Group target = new Group();
            TaggedObject objectInfo = new TaggedObject();

            objectInfo["Group"] = "Test";
            objectInfo["Timeout"] = "unset";
            objectInfo["IdleTimeout"] = "unset";
            objectInfo["PasswordTimeout"] = "unset";

            target.FromGroupCmdTaggedOutput(objectInfo);

            Assert.AreEqual(0, target.TimeOut);
            Assert.AreEqual(0, target.IdleTimeout);
            Assert.AreEqual(0, target.PasswordTimeout);
        }

        /// <summary>
        /// Verifies that a spec with "unlimited" values parses timeout fields to -1.
        /// </summary>
        [TestMethod()]
        public void ParseTestUnlimitedTimeouts()
        {
            Group target = new Group();
            bool actual = target.Parse(GroupSpecUnlimited);
            Assert.IsTrue(actual);
            Assert.AreEqual(-1, target.TimeOut);
            Assert.AreEqual(-1, target.IdleTimeout);
            Assert.AreEqual(-1, target.PasswordTimeout);
        }

        /// <summary>
        /// Verifies that a spec with "unset" values parses timeout fields to 0.
        /// </summary>
        [TestMethod()]
        public void ParseTestUnsetTimeouts()
        {
            Group target = new Group();
            bool actual = target.Parse(GroupSpecUnset);
            Assert.IsTrue(actual);
            Assert.AreEqual(0, target.TimeOut);
            Assert.AreEqual(0, target.IdleTimeout);
            Assert.AreEqual(0, target.PasswordTimeout);
        }

        /// <summary>
        /// Verifies that timeout fields set to -1 (unlimited) are serialized as "unlimited".
        /// </summary>
        [TestMethod()]
        public void ToStringTestUnlimitedTimeouts()
        {
            Group target = new Group();
            target.Id = "Test";
            target.Description = "Test Description";
            target.MaxResults = 100;
            target.MaxScanRows = 10;
            target.MaxLockTime = 20;
            target.TimeOut = -1;
            target.IdleTimeout = -1;
            target.PasswordTimeout = -1;
            target.MaxOpenFiles = 50;
            target.MaxMemory = 60;
            target.OwnerNames = new List<string>() { "Alpha", "Beta" };
            target.UserNames = new List<string>() { "Alice", "Bob" };
            target.SubGroups = new List<string>() { "SG1", "SG2" };

            string expected = GroupSpecUnlimited.Trim();
            string actual = target.ToString().Trim();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Verifies that "unlimited" tagged output maps to -1 for the five Max* fields.
        /// </summary>
        [TestMethod()]
        public void FromGroupCmdTaggedOutputUnlimitedMaxFieldsTest()
        {
            Group target = new Group();
            TaggedObject objectInfo = new TaggedObject();

            objectInfo["Group"] = "Test";
            objectInfo["MaxResults"] = "unlimited";
            objectInfo["MaxScanRows"] = "unlimited";
            objectInfo["MaxLockTime"] = "unlimited";
            objectInfo["MaxOpenFiles"] = "unlimited";
            objectInfo["MaxMemory"] = "unlimited";

            target.FromGroupCmdTaggedOutput(objectInfo);

            Assert.AreEqual(-1, target.MaxResults);
            Assert.AreEqual(-1, target.MaxScanRows);
            Assert.AreEqual(-1, target.MaxLockTime);
            Assert.AreEqual(-1, target.MaxOpenFiles);
            Assert.AreEqual(-1, target.MaxMemory);
        }

        /// <summary>
        /// Verifies that "unset" tagged output maps to 0 for the five Max* fields.
        /// </summary>
        [TestMethod()]
        public void FromGroupCmdTaggedOutputUnsetMaxFieldsTest()
        {
            Group target = new Group();
            TaggedObject objectInfo = new TaggedObject();

            objectInfo["Group"] = "Test";
            objectInfo["MaxResults"] = "unset";
            objectInfo["MaxScanRows"] = "unset";
            objectInfo["MaxLockTime"] = "unset";
            objectInfo["MaxOpenFiles"] = "unset";
            objectInfo["MaxMemory"] = "unset";

            target.FromGroupCmdTaggedOutput(objectInfo);

            Assert.AreEqual(0, target.MaxResults);
            Assert.AreEqual(0, target.MaxScanRows);
            Assert.AreEqual(0, target.MaxLockTime);
            Assert.AreEqual(0, target.MaxOpenFiles);
            Assert.AreEqual(0, target.MaxMemory);
        }

        /// <summary>
        /// Verifies that a spec with "unlimited" Max* values parses all five fields to -1.
        /// </summary>
        [TestMethod()]
        public void ParseTestUnlimitedMaxFields()
        {
            Group target = new Group();
            bool actual = target.Parse(GroupSpecMaxUnlimited);
            Assert.IsTrue(actual);
            Assert.AreEqual(-1, target.MaxResults);
            Assert.AreEqual(-1, target.MaxScanRows);
            Assert.AreEqual(-1, target.MaxLockTime);
            Assert.AreEqual(-1, target.MaxOpenFiles);
            Assert.AreEqual(-1, target.MaxMemory);
        }

        /// <summary>
        /// Verifies that a spec with "unset" Max* values parses all five fields to 0.
        /// </summary>
        [TestMethod()]
        public void ParseTestUnsetMaxFields()
        {
            Group target = new Group();
            bool actual = target.Parse(GroupSpecMaxUnset);
            Assert.IsTrue(actual);
            Assert.AreEqual(0, target.MaxResults);
            Assert.AreEqual(0, target.MaxScanRows);
            Assert.AreEqual(0, target.MaxLockTime);
            Assert.AreEqual(0, target.MaxOpenFiles);
            Assert.AreEqual(0, target.MaxMemory);
        }

        /// <summary>
        /// Verifies that Max* fields set to -1 (unlimited) are serialized as "unlimited" in the spec.
        /// </summary>
        [TestMethod()]
        public void ToStringTestUnlimitedMaxFields()
        {
            Group target = new Group();
            target.Id = "Test";
            target.Description = "Test Description";
            target.MaxResults = -1;
            target.MaxScanRows = -1;
            target.MaxLockTime = -1;
            target.MaxOpenFiles = -1;
            target.MaxMemory = -1;
            target.TimeOut = 30;
            target.IdleTimeout = 35;
            target.PasswordTimeout = 40;
            target.OwnerNames = new List<string>() { "Alpha", "Beta" };
            target.UserNames = new List<string>() { "Alice", "Bob" };
            target.SubGroups = new List<string>() { "SG1", "SG2" };

            string expected = GroupSpecMaxUnlimited.Trim();
            string actual = target.ToString().Trim();
            Assert.AreEqual(expected, actual);
        }
    }
}
