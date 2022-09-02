using Perforce.P4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using NLog;

namespace p4api.net.unit.test
{
    /// <summary>
    ///This is a test class for VersionSpecTest and is intended
    ///to contain all VersionSpecTest Unit Tests
    ///</summary>
	[TestClass()]
#if NET462
    [DeploymentItem("x64", "x64")]
    [DeploymentItem("x86", "x86")]
#endif
	public class VersionSpecTest
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


        /// <summary>
        ///A test for VersionSpec
        ///</summary>
        [TestMethod()]
		public void VersionSpecTest1()
		{
            // VersionRange tests
			VersionSpec left = new VersionRange(1,2);
			VersionSpec rightpos = new VersionRange(1, 2);
			VersionSpec rightneg1 = new VersionRange(1, 4);
			VersionSpec rightneg2 = new Revision(1);
			VersionSpec rightnull = null;

			Assert.IsTrue(left.Equals(rightpos));
			Assert.IsFalse(left.Equals(rightneg1));
			Assert.IsFalse(left.Equals(rightneg2));
			Assert.IsFalse(left.Equals(rightnull));

            string serialized = left.ToString();
            VersionSpec vrange = new VersionRange(serialized);
            string sout = vrange.ToString();
            Assert.AreEqual(serialized, sout);

            // HeadRevision tests
			left = new HeadRevision();
			rightpos = new HeadRevision();
			rightneg1 = new HaveRevision();
			rightnull = null;

			Assert.IsTrue(left.Equals(rightpos));
			Assert.IsFalse(left.Equals(rightneg1));
			Assert.IsFalse(left.Equals(rightnull));

            serialized = left.ToString();
            HeadRevision headrev = new HeadRevision(serialized);
            sout = headrev.ToString();
            Assert.AreEqual(serialized, sout);

            // HaveRevision tests
			left = new HaveRevision();
			rightpos = new HaveRevision();
			rightneg1 = new HeadRevision();
			rightnull = null;

			Assert.IsTrue(left.Equals(rightpos));
			Assert.IsFalse(left.Equals(rightneg1));
			Assert.IsFalse(left.Equals(rightnull));

            serialized = left.ToString();
            HaveRevision haverev = new HaveRevision(serialized);
            sout = haverev.ToString();
            Assert.AreEqual(serialized, sout);

            // NoneRevision tests
			left = new NoneRevision();
			rightpos = new NoneRevision();
			rightneg1 = new HaveRevision();
			rightnull = null;

			Assert.IsTrue(left.Equals(rightpos));
			Assert.IsFalse(left.Equals(rightneg1));
			Assert.IsFalse(left.Equals(rightnull));

            serialized = left.ToString();
            NoneRevision nrev = new NoneRevision(serialized);
            sout = nrev.ToString();
            Assert.AreEqual(serialized, sout);

            // Revision tests
			left = new Revision(1);
			rightpos = new Revision(1);
			rightneg1 = new Revision(3);
			rightneg2 = new VersionRange(1, 4);
			rightnull = null;

			Assert.IsTrue(left.Equals(rightpos));
			Assert.IsFalse(left.Equals(rightneg1));
			Assert.IsFalse(left.Equals(rightneg2));
			Assert.IsFalse(left.Equals(rightnull));

            serialized = left.ToString();
            Revision newrev = new Revision(serialized);
            sout = newrev.ToString();
            Assert.AreEqual(serialized, sout);

            // DateTimeVersion tests
			left = new DateTimeVersion(DateTime.MinValue);
			rightpos = new DateTimeVersion(DateTime.MinValue);
			rightneg1 = new DateTimeVersion(DateTime.MaxValue);
			rightneg2 = new Revision(3);
			rightnull = null;

			Assert.IsTrue(left.Equals(rightpos));
			Assert.IsFalse(left.Equals(rightneg1));
			Assert.IsFalse(left.Equals(rightneg2));
			Assert.IsFalse(left.Equals(rightnull));

            serialized = left.ToString();
            DateTimeVersion dtver = new DateTimeVersion(serialized);
            sout = dtver.ToString();
            Assert.AreEqual(serialized, sout);

            // LabelNameVersion tests
			left = new LabelNameVersion("label_name");
			rightpos = new LabelNameVersion("label_name");
			rightneg1 = new LabelNameVersion("wrong_label_name");
			rightneg2 = new Revision(3);
			rightnull = null;

			Assert.IsTrue(left.Equals(rightpos));
			Assert.IsFalse(left.Equals(rightneg1));
			Assert.IsFalse(left.Equals(rightneg2));
			Assert.IsFalse(left.Equals(rightnull));

            serialized = left.ToString();
            LabelNameVersion lver = new LabelNameVersion(serialized, true);
            sout = lver.ToString();
            Assert.AreEqual(serialized, sout);

            // ChangelistIdVersion tests
			left = new ChangelistIdVersion(44444);
			rightpos = new ChangelistIdVersion(44444);
			rightneg1 = new ChangelistIdVersion(88888);
			rightneg2 = new Revision(3);
			rightnull = null;

			Assert.IsTrue(left.Equals(rightpos));
			Assert.IsFalse(left.Equals(rightneg1));
			Assert.IsFalse(left.Equals(rightneg2));
			Assert.IsFalse(left.Equals(rightnull));

            serialized = left.ToString();
            ChangelistIdVersion clver = new ChangelistIdVersion(serialized);
            sout = clver.ToString();
            Assert.AreEqual(serialized, sout);

            // ClientNameVersion tests
			left = new ClientNameVersion("client_name");
			rightpos = new ClientNameVersion("client_name");
			rightneg1 = new ClientNameVersion("wrong_client_name");
			rightneg2 = new Revision(3);
			rightnull = null;

			Assert.IsTrue(left.Equals(rightpos));
			Assert.IsFalse(left.Equals(rightneg1));
			Assert.IsFalse(left.Equals(rightneg2));
			Assert.IsFalse(left.Equals(rightnull));

            serialized = left.ToString();
            ClientNameVersion cnver = new ClientNameVersion(serialized, true);
            sout = cnver.ToString();
            Assert.AreEqual(serialized, sout);

            // ActionVersion tests
            left = new ActionVersion("add");
            rightpos = new ActionVersion("add");
            rightneg1 = new ActionVersion("branch");
			rightneg2 = new Revision(3);
			rightnull = null;

			Assert.IsTrue(left.Equals(rightpos));
			Assert.IsFalse(left.Equals(rightneg1));
			Assert.IsFalse(left.Equals(rightneg2));
			Assert.IsFalse(left.Equals(rightnull));

            serialized = left.ToString();
            ActionVersion aver = new ActionVersion(serialized, true);
            sout = aver.ToString();
            Assert.AreEqual(serialized, sout);

            // ShelvedInChangelistIdVersion tests
            left = new ShelvedInChangelistIdVersion("@=42");
            rightpos = new ShelvedInChangelistIdVersion("@=42");
            rightneg1 = new ShelvedInChangelistIdVersion("@=100");
            rightneg2 = new Revision(3);
            rightnull = null;

            Assert.IsTrue(left.Equals(rightpos));
            Assert.IsFalse(left.Equals(rightneg1));
            Assert.IsFalse(left.Equals(rightneg2));
            Assert.IsFalse(left.Equals(rightnull));

            serialized = left.ToString();
            ShelvedInChangelistIdVersion shver = new ShelvedInChangelistIdVersion(serialized);
            sout = shver.ToString();
            Assert.AreEqual(serialized, sout);
		}
	}
}
