using System;
using System.IO;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using Perforce.P4;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using File = System.IO.File;
using System.Runtime.InteropServices;

namespace p4api.net.unit.test
{
    // This class handles the unit test configuration as stored in appsettings.json
    // Three values for each OS (directory, p4d, tar) and one common (port)
    class UnitTestConfiguration
    {
        enum Platform
        {
            Windows,
            Linux,
            Osx
        }

        Platform current_platform;

        Architecture current_architecture;

        public PlatformID CurrentPlatform { get; set; }
        public string WindowsTestDirectory { get; set; }
        public string WindowsP4dPath { get; set; }
        public string WindowsTarPath { get; set; }
        public string LinuxTestDirectory { get; set; }
        public string LinuxP4dPath { get; set; }
        public string LinuxArm64P4dPath { get; set; }
        public string LinuxTarPath { get; set; }
        public string OsxTestDirectory { get; set; }
        public string OsxP4dPath { get; set; }
        public string OsxTarPath { get; set; }
        public string ServerPort { get; set; }

        public string TestDirectory
        {
            get
            {
                if (current_platform == Platform.Osx)
                    return OsxTestDirectory;

                if (current_platform == Platform.Linux)
                    return LinuxTestDirectory;

                return WindowsTestDirectory;
            }
        }

        public string TestP4Enviro
        {
            get
            {
                return Path.Combine(TestDirectory, ".p4enviro.txt");
            }
        }

        public string TestP4Tickets
        {
            get
            {
                return Path.Combine(TestDirectory, ".p4tickets.txt");
            }
        }

        public string P4dPath
        {
            get
            {
                if (current_platform == Platform.Osx)
                    return OsxP4dPath;

                if (current_platform == Platform.Linux)
                {
                    if (current_architecture == Architecture.Arm64)
                    {
                        return LinuxArm64P4dPath;
                    }
                    else
                    {
                        return LinuxP4dPath;
                    }
                }

                return WindowsP4dPath;
            }
        }

        public string TarPath
        {
            get
            {
                if (current_platform == Platform.Osx)
                    return OsxTarPath;

                if (current_platform == Platform.Linux)
                    return LinuxTarPath;

                return WindowsTarPath;
            }
        }

        public UnitTestConfiguration()
        {
#if NET5_0_OR_GREATER
            string pdesc = RuntimeInformation.OSDescription;
            current_architecture = RuntimeInformation.OSArchitecture;

            if (pdesc.Contains("Darwin"))
                current_platform = Platform.Osx;
            else if (pdesc.Contains("Linux"))
                current_platform = Platform.Linux;
            else
            {
                current_platform = Platform.Windows;
            }
#else
            current_platform = Platform.Windows;
#endif
        }
    }

    class UnitTestSettings
    {
        public static IConfigurationRoot GetIConfigurationRoot()
        {
            return new ConfigurationBuilder()
                        .SetBasePath(Utilities.GetUnitTestDir())
                        .AddJsonFile("appsettings.json", optional: true)
                        .Build();
        }

        public static UnitTestConfiguration GetApplicationConfiguration()
        {
            var configuration = new UnitTestConfiguration();
            var iConfig = GetIConfigurationRoot();

            iConfig.GetSection("UnitTest").Bind(configuration);

            return configuration;
        }
    }



    public class Utilities
    {
        private static UnitTestConfiguration configuration;
        private static string testDir;

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static Stopwatch stopWatchAllTests = null;
        private static Stopwatch stopWatch = null;
        private static int testCount = 0;
        private static string[] settings = new string[] { "P4CLIENT", "P4CONFIG", "P4IGNORE", "P4PASSWD", "P4PORT", "P4TRUST", "P4TICKETS", "P4USER" };
        private static string[] settingValues;
        private static string[] envValues;
        private static string cwd;
        private static int[] preTestObjectCount;
        private static P4CallBacks.LogMessageDelegate logDelegate = null;
        private static LogFile.LogMessageDelgate logFileDelegate = null;
        private static long allocs = 0;
        private static long frees = 0;

        // strings initialized from configuration settings.
        private static string rubbishBin;
        private static string _p4d_cmd;
        private static string rsh_p4d_cmd;

        // Three checkpoints types are available.
        // They are associated with the tar files a.tar u.tar and s3.tar ( kept in ../testDataDir )
        public enum CheckpointType
        {
            A = 0,  // Standard Server
            U = 1,  // Unicode Server
            S3 = 2   // Security level 3
        }

        static Utilities()
        {
            configuration = UnitTestSettings.GetApplicationConfiguration();
            testDir = configuration.TestDirectory;
            rubbishBin = testDir + "_rubbish-bin";
            _p4d_cmd = "-p " + configuration.ServerPort + " -Id {1} -r {0} -L p4d.log";
            rsh_p4d_cmd = "rsh:" + configuration.P4dPath + " -r {0} -L p4d.log -vserver=3 -i";
        }

        // how much logging information we see from the bridge
        // big numbers are noisier, 4 is DEBUG
        private static int bridge_log_level = 4;

        private static void LogFunction(int logLevel, string file, int line, string message)
        {
            if (logLevel <= bridge_log_level)
            {
                logger.Debug($"{file}({line}): {message}");
            }
        }

        private static void LogFileFunction(int logLevel, string src, string message)
        {
            if (logLevel <= bridge_log_level)
            {
                logger.Debug($"{src}: {message}");
            }
        }

        private static void SaveP4Prefs()
        {
            settingValues = new string[settings.Length];
            envValues = new string[settings.Length];
            for (int i = 0; i < settings.Length; i++)
            {
                settingValues[i] = P4Server.Get(settings[i]);
                // also clear the value
                P4Server.Set(settings[i], "");
            }
        }

        private static void RestoreP4Prefs()
        {
            P4Bridge.ReloadEnviro();

            for (int i = 0; i < settings.Length; i++)
            {
                P4Server.Set(settings[i], settingValues[i]);
            }
        }

        public static void LogTestStart(TestContext testContext)
        {
            // use non-default locations for ticket file and p4enviro file
            //    (protect the build and test environment from corruption)
            Environment.SetEnvironmentVariable("P4TICKETS", configuration.TestP4Tickets);
            Environment.SetEnvironmentVariable("P4ENVIRO", configuration.TestP4Enviro);

                // save and restore the p4config env variable and the cwd
            SaveP4Prefs();
            allocs = P4Debugging.GetStringAllocs();
            frees = P4Debugging.GetStringReleases();

            cwd = System.IO.Directory.GetCurrentDirectory();

            logDelegate = new P4CallBacks.LogMessageDelegate(LogFunction);
            logFileDelegate = new LogFile.LogMessageDelgate(LogFileFunction);

            LogFile.SetLoggingFunction(logFileDelegate);

            // turn up the bridge debugging
            P4Debugging.SetBridgeLogFunction(logDelegate);

            // track allocated objects
            preTestObjectCount = new int[P4Debugging.GetAllocObjectCount()];

            for (int i = 0; i < preTestObjectCount.Length; i++)
            {
                preTestObjectCount[i] = P4Debugging.GetAllocObject(i);
            }

            // reset the p4d_cmd variable to the default
            p4d_cmd = _p4d_cmd;

            testCount++;
            logger.Info("====== TestName: {0}", testContext.TestName);
            stopWatch = Stopwatch.StartNew();
            if (stopWatchAllTests == null)
            {
                stopWatchAllTests = Stopwatch.StartNew();
            }
        }

        public static void LogTestFinish(TestContext testContext)
        {
            P4Debugging.SetBridgeLogFunction(null);   // unlink the log function

            System.IO.Directory.SetCurrentDirectory(cwd);
            RestoreP4Prefs();

            logger.Info("------ {0}: {1}, {2} ms", testContext.TestName, testContext.CurrentTestOutcome, stopWatch.ElapsedMilliseconds);

            int iExtraObjects = 0;
            for (int i = 0; i < P4Debugging.GetAllocObjectCount(); i++)
            {
                int postTest = P4Debugging.GetAllocObject(i);
                if (preTestObjectCount[i] != postTest)
                {
                    iExtraObjects += postTest - preTestObjectCount[i];
                    logger.Info(string.Format("<<<<*** Item count for {0} mismatch: {1}/{2}",
                        P4Debugging.GetAllocObjectName(i), preTestObjectCount[i], postTest));
                }
            }

            long postAllocs = P4Debugging.GetStringAllocs();
            long postFrees = P4Debugging.GetStringReleases();

            if (postAllocs - allocs != postFrees - frees)
            {
                logger.Info(string.Format("<<<<*** String alloc mismatch: {0}/{1}", postAllocs - allocs, postFrees - frees));
                Assert.AreEqual(postAllocs - allocs, postFrees - frees);
            }

            Assert.AreEqual(0, iExtraObjects);
        }

        public static void LogAllTestsFinish()
        {
            logger.Info("@@@@@@ Time for tests: {0} - {1} s", testCount, stopWatchAllTests.Elapsed);
        }


        // Delete recursively a directory, even if the files are read only,
        // If the directory doesn't exist, that is OK too.
        public static void DeleteDirectory(string path, bool recurse = false)
        {
            if (path == null)
                return;

            //    if (! recurse)
            //        logger.Info("DeleteDirectory {0}", path);  // only log the topmost delete path

            if (!Directory.Exists(path))
                return;

            try
            {
                foreach (string folder in Directory.GetDirectories(path))
                {
                    DeleteDirectory(folder, true);
                }

                foreach (string file in Directory.GetFiles(path))
                {
                    var pPath = Path.Combine(path, file);
                    System.IO.File.SetAttributes(pPath, FileAttributes.Normal);
                    System.IO.File.Delete(file);
                }

                Directory.Delete(path);
            }
            catch (Exception ex)
            {
                logger.Info("DeleteDirectory {0} Failed: {1}", path, ex.Message);
            }
        }

        // Convert all line feeds and CRLF to OS NewLine format
        public static string FixLineFeeds(string src)
        {
            return Regex.Replace(src, @"\r\n?|\n", System.Environment.NewLine);
        }

        static string p4d_cmd = null;

        static string restore_cmd = "-C1 -r {0} -jr checkpoint.{1}";
        static string upgrade_cmd = "-C1 -r {0} -xu";
        static string generate_key_cmd = string.Empty;

        public static Process DeploySSLP4TestServer(string path, CheckpointType cptype = CheckpointType.A)
        {
            // Create the ssldir
            string ssldir = Path.Combine(path, "ssldir");
            if (!CreateDir(ssldir, 5))
                return null;
#if (_LINUX || _OSX)
            // in Linux and OSX the SSLDIR requires a specific set of permissions
            string cmd = $"chmod -R 700 {ssldir}";
            using (Process proc = Process.Start("/bin/bash", $"-c \"{cmd}\""))
           {
                proc.WaitForExit();
                if (proc.ExitCode != 0)
                {
                    logger.Error("Unable to change ssldir permissions");
                    return null;
                }
            }
#endif
            // If there are left over files in ssldir, delete them
            foreach (string file in Directory.GetFiles(ssldir))
            {
                var pPath = Path.Combine(ssldir, file);
                System.IO.File.SetAttributes(pPath, FileAttributes.Normal);
                System.IO.File.Delete(file);
            }
            System.Environment.SetEnvironmentVariable("P4SSLDIR", ssldir);
            string test = System.Environment.GetEnvironmentVariable("P4SSLDIR");
            p4d_cmd = "-p ssl:" + configuration.ServerPort + " -Id UnitTestServer -r {0} -L p4d.log";
            generate_key_cmd = "-Gc";
            return DeployP4TestServer(path, 1, cptype);
        }

        public static Process DeployIPv6P4TestServer(string path, string tcp, CheckpointType cptype)
        {
            string[] parts = configuration.ServerPort.Split(':');
            p4d_cmd = "-p " + tcp + ":::1:" + parts[1] + " -Id UnitTestServer -r {0} -L p4d.log";
            return DeployP4TestServer(path, 1, cptype);
        }

        public static Process DeployP4TestServer(string path, CheckpointType cptype, string testName = "")
        {
            return DeployP4TestServer(path, 1, cptype, testName);
        }

        public static string TestServerRoot(string baseRoot, CheckpointType cptype)
        {
            return Path.Combine(baseRoot, cptype.ToString().ToLower(), "server");
        }

        public static string TestRshServerPort(string baseRoot, CheckpointType cptype)
        {
            var root = Path.Combine(baseRoot, cptype.ToString().ToLower(), "server");
            return string.Format(rsh_p4d_cmd, root);
        }

        public static string TestClientRoot(string baseRoot, CheckpointType cptype)
        {
            return Path.Combine(baseRoot, cptype.ToString().ToLower(), "clients");
        }

        // Most clients have platform specific paths embedded in them, so we fix them up before we use them
        //   in the runtime operating system.
        // This also runs "p4 verify -v" on non NT systems, to fix the archive files. (which are in NT format)
        //  And then optionally syncs the client workspace.
        public static void SetClientRoot(Repository rep, string TestDir, CheckpointType cptype, string ws_client, bool dosync = true)
        {
            using (Connection con = rep.Connection)
            {
                con.UserName = "admin";

                con.Connect(null);
                con.getP4Server().SetTicketFile(configuration.TestP4Tickets);  // sometimes the bridge doesn't pick up the P4TICKETS environment variable

                Credential cred = con.Login("Password");

                Client c = rep.GetClient(ws_client, null);
                c.Root = Path.Combine(TestClientRoot(TestDir, cptype), ws_client);
                rep.UpdateClient(c);

                // On non NT systems, the archive files may have BAD checksums, so fix them using "p4 verify -v"
                if (configuration.CurrentPlatform != PlatformID.Win32NT)
                {
                    P4CommandResult results;

                    // ignore exceptions, p4 verify -v will always return errors!
                    ErrorSeverity sev = P4Exception.MinThrowLevel;
                    P4Exception.MinThrowLevel = ErrorSeverity.E_NOEXC;

                    string[] args = { "-v", "-q", "//..." };
                    using (P4Command cmd = new P4Command(rep, "verify", false, args))
                    {
                        try
                        {
                            results = cmd.Run();
                        }
                        catch (P4Exception ex)
                        {
                            logger.Info("'p4 verify -v -q //...' Threw Exception: {0}", ex.Message);
                        }
                    }

                    P4Exception.MinThrowLevel = sev;
                }

                con.Client = c;

                if (dosync)
                {
                    // Now sync all the files in the workspace
                    var syncOpts = new SyncFilesCmdOptions(SyncFilesCmdFlags.Force, 0);
                    con.Client.SyncFiles(syncOpts, new FileSpec(new DepotPath("//...")));
                }

                rep.Connection.Server.SetState(ServerState.Unknown);
            }
        }


        public static Process DeployP4TestServer(string path, int checkpointRev,
            CheckpointType cptype, string testName = "")
        {
            string tarFile = cptype.ToString().ToLower() + ".tar";

            return DeployP4TestServer(path, checkpointRev, tarFile, testName);
        }

        public static Process DeployP4TestServer(string path, int checkpointRev, string tarFile,
             string testName = "")
        {
            return DeployP4TestServer(path, checkpointRev, tarFile, null, testName);
        }

        public static string GetThisFilePath([CallerFilePath] string path = null)
        {
            return path;
        }

        public static string GetUnitTestDir()
        {
            return Path.GetDirectoryName(GetThisFilePath());
        }

        // if the server directory is not already populated, we need to extract the tarFile
        // and copy it's contents to testServerRoot
        public static bool PopulateServer(string testServerRoot, string tarFile)
        {
            //logger.Info("PopulateServer {0} {1}", testServerRoot, tarFile);
            if (!Directory.Exists(rubbishBin))
            {
                Directory.CreateDirectory(rubbishBin);
            }
            string mypath = GetThisFilePath();

            string TestDataDir = "";

            string pDir = Path.GetDirectoryName(mypath);  // Search for "p4api.net/testDataDir"
            do
            {
                if (Path.GetFileName(pDir) == "p4api.net")
                {
                    string tdir = Path.Combine(pDir, "testDataDir");
                    if (Directory.Exists(tdir))
                    {
                        TestDataDir = tdir;
                        break;
                    }
                }
                pDir = Path.GetDirectoryName(pDir);
            }
            while
                (pDir != null);

            var tarBase = Path.GetFileNameWithoutExtension(tarFile); // a or u
            var untarredContentsDir = Path.Combine(rubbishBin, tarBase);

            string unitTestTar = Path.Combine(TestDataDir, tarFile);
            string targetTestTar = Path.Combine(untarredContentsDir, tarFile);

            // check for the tar file target directory
            if (!Directory.Exists(untarredContentsDir))
            {
                Utilities.CreateDir(untarredContentsDir, 5);
            }

            if (!File.Exists(targetTestTar))
            {
                logger.Info("PopulateServer: copy {0} to {1}", unitTestTar, targetTestTar);
                if (!CopyFile(unitTestTar, targetTestTar))
                    return false;
            }

            // untar the file
            ProcessStartInfo si;
            FileInfo fi = new FileInfo(targetTestTar);
            fi.IsReadOnly = false;

            Process Untarrer = new Process();

            // unpack the tar ball
            si = new ProcessStartInfo(configuration.TarPath);
            si.WorkingDirectory = untarredContentsDir;
#if _LINUX
            si.Arguments = String.Format("--warning=no-unknown-keyword -xf {0}", tarFile);
#else
            si.Arguments = string.Format("-xf {0}", tarFile);
#endif
            logger.Info("PopulateServer: in {0}, {1} {2}", untarredContentsDir, si.FileName, si.Arguments);

            try
            {
                Untarrer.StartInfo = si;
                Untarrer.Start();
                Untarrer.WaitForExit();
            }
            catch (Exception ex)
            {
                logger.Info("PopulateServer: {0}. Extraction of tar file failed\n", ex.Message);
                return false;
            }

            // Copy untarred directory tree to test directory
            if (!CopyDirTree(untarredContentsDir, testServerRoot))
                return false;

            return true;
        }

        public static Process DeployP4TestServer(string testRoot, int checkpointRev,
            string tarFile, string P4DCmd, string testName)
        {
            try
            {
                var tarBase = Path.GetFileNameWithoutExtension(tarFile); // a, u, s3
                var untarredContentsDir = Path.Combine(rubbishBin, tarBase);

                var testServerRoot = Path.Combine(testRoot, tarBase, "server");
                var testClientsRoot = Path.Combine(testRoot, tarBase, "clients");

                // If the checkpoints are not in the server root, populate the directory
                if (!System.IO.File.Exists(Path.Combine(testServerRoot, "checkpoint.2")))
                {
                    if (!PopulateServer(testServerRoot, tarFile))
                        return null;
                }

                // Change directory
                Environment.CurrentDirectory = testServerRoot;
                string CurWDir = Environment.CurrentDirectory;

                string unitTestDir = AppDomain.CurrentDomain.BaseDirectory;
                //logger.Info("unitTestDir {0}", unitTestDir);

                if (unitTestDir.ToLower().StartsWith("file:\\"))
                {
                    // cut off the file:\\
                    var idx = unitTestDir.IndexOf("\\", StringComparison.Ordinal) + 1;
                    unitTestDir = unitTestDir.Substring(idx);
                }

                // Make sure no db.* files present
                // logger.Info("Removing db.* from {0}", testServerRoot);
                try
                {
                    foreach (string fname in Directory.GetFiles(testServerRoot, "db.*", SearchOption.TopDirectoryOnly))
                    {
                        System.IO.File.Delete(fname);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    logger.Error(ex.StackTrace);
                    return null;
                }

                // Always Reset client directories
                DeleteDirectory(testClientsRoot);
                CreateDir(testClientsRoot, 5);

                foreach (
                    string fname in Directory.GetDirectories(untarredContentsDir, "*space*", SearchOption.TopDirectoryOnly)
                )
                {
                    var dirOnly = Path.GetFileName(fname);
                    if (!CopyDirTree(fname, Path.Combine(testClientsRoot, dirOnly)))
                        return null;
                }

                ProcessStartInfo si;

                if (p4d_cmd.Contains("ssl:"))
                {
                    using (Process GenKeyandCert = new Process())
                    {
                        // generate private key and certificate
                        si = new ProcessStartInfo(configuration.P4dPath);
                        si.Arguments = generate_key_cmd;
                        si.WorkingDirectory = testServerRoot;
                        si.UseShellExecute = false;
                        si.CreateNoWindow = true;
                        si.RedirectStandardOutput = true;

                        var msg = si.Arguments;
                        logger.Info(msg);

                        GenKeyandCert.StartInfo = si;
                        GenKeyandCert.Start();
                        GenKeyandCert.WaitForExit();
                    }
                }

                // restore the checkpoint
                using (Process RestoreCheckPoint = new Process())
                {
                    si = new ProcessStartInfo(configuration.P4dPath);
                    si.Arguments = string.Format(restore_cmd, testServerRoot, checkpointRev);
                    si.WorkingDirectory = testServerRoot;
                    si.UseShellExecute = false;
                    si.RedirectStandardOutput = true;
                    si.RedirectStandardError = true;
                    si.CreateNoWindow = true;

                    // logger.Info("{0} {1}", si.FileName, si.Arguments);

                    RestoreCheckPoint.StartInfo = si;
                    RestoreCheckPoint.Start();
                    RestoreCheckPoint.WaitForExit();

                    if (RestoreCheckPoint.ExitCode != 0)
                    {
                        var errorStream = RestoreCheckPoint.StandardError;
                        logger.Error("Error restoring checkpoint.{0} {1}", checkpointRev, errorStream.ReadToEnd());
                        return null;
                    }
                }

                // upgrade the db tables
                using (Process UpgradeTables = new Process())
                {
                    si = new ProcessStartInfo(configuration.P4dPath);
                    si.Arguments = string.Format(upgrade_cmd, testServerRoot);
                    si.WorkingDirectory = testServerRoot;
                    si.UseShellExecute = false;
                    si.CreateNoWindow = true;
                    si.RedirectStandardOutput = true;

                    //logger.Info("{0} {1}", si.FileName, si.Arguments);

                    UpgradeTables.StartInfo = si;
                    UpgradeTables.Start();
                    UpgradeTables.WaitForExit();

                    if (UpgradeTables.ExitCode != 0)
                    {
                        logger.Error("Error upgrading server tables");
                        return null;
                    }
                }

                Process p4d = new Process();

                if (P4DCmd != null)
                {
                    string P4DCmdSrc = Path.Combine(unitTestDir, P4DCmd);
                    string P4DCmdTarget = Path.Combine(testServerRoot, P4DCmd);
                    System.IO.File.Copy(P4DCmdSrc, P4DCmdTarget);

                    // run the command to start p4d
                    si = new ProcessStartInfo(P4DCmdTarget);
                    si.Arguments = string.Format(testServerRoot);
                    si.WorkingDirectory = testServerRoot;
                    si.UseShellExecute = false;
                    si.CreateNoWindow = true;
                    si.RedirectStandardOutput = true;

                    // logger.Info("{0} {1}", si.FileName, si.Arguments);

                    p4d.StartInfo = si;
                    p4d.Start();
                }
                else
                {
                    //start p4d
                    si = new ProcessStartInfo(configuration.P4dPath);
                    if (string.IsNullOrEmpty(testName))
                        testName = "UnitTestServer";
                    si.Arguments = string.Format(p4d_cmd, testServerRoot, testName);
                    si.WorkingDirectory = testServerRoot;
                    si.UseShellExecute = false;
                    si.RedirectStandardOutput = true;
                    si.CreateNoWindow = true;

                    // logger.Info("{0} {1}", si.FileName, si.Arguments);

                    p4d.StartInfo = si;
                    p4d.Start();
                }
                Environment.CurrentDirectory = CurWDir;

                // Give p4d time to start up
                using (TcpClient client = new TcpClient())
                {
                    DateTime started = DateTime.UtcNow;
                    while (DateTime.UtcNow.Subtract(started).Milliseconds < 500)
                    {
                        try
                        {
                            string[] parts = configuration.ServerPort.Split(':');
                            int port = int.Parse(parts[1]);
                            client.Connect(parts[0], port);
                            if (client.Connected)
                                return p4d;
                        }
                        catch (FormatException ex)
                        {
                            logger.Error("Unparsable port number in configuration: {0} {1}",
                                configuration.ServerPort, ex.Message);
                        }
                        catch (SocketException)
                        {
                            // logger.Info("Ignoring Exception during startup: {0}", ex.Message);
                            // Ignore any exception while waiting for p4d to initialize
                        }
                    }
                }

                // that didn't work
                return p4d;

            }
            finally
            {
#if (_LINUX || _OSX)
                    // Linux tests are getting triggerred even before the p4d checkpoint is upgraded to the latest
                    // So wait for a bit
                    System.Threading.Thread.Sleep(2000);
#endif
            }
        }

        private static bool CopyDirTree(string srcDir, string targDir)
        {
            //logger.Info("Copying directory {0} to {1}", srcDir, targDir);
            // First Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(srcDir, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(srcDir, targDir));

            // Then copy all the files & Replaces any files with the same name
            foreach (string fname in Directory.GetFiles(srcDir, "*.*", SearchOption.AllDirectories))
            {
                var target = fname.Replace(srcDir, targDir);
                try
                {
                    System.IO.File.Copy(fname, target, true);
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                    return false;
                }
            }
            //logger.Info("Finished copying {0} to {1}", srcDir, targDir);
            return true;
        }

        private static bool CopyFile(string srcFile, string targetFile)
        {
            int retries = 3;
            int delay = 1000; // initial delay 1 second
            while (retries > 0)
            {
                try
                {
                    System.IO.File.Copy(srcFile, targetFile);
                    return true; //success
                }
                catch (Exception)
                {
                    System.Threading.Thread.Sleep(delay);
                    delay *= 2; // wait twice as long next time
                    retries--;
                }
            }
            logger.Info("Unable to copy {0} to {1}", srcFile, targetFile);
            return false;
        }

        public static bool CreateDir(string path, int retries)
        {
            while (retries > 0)
            {
                if (Directory.Exists(path))
                    return true;

                try
                {
                    Directory.CreateDirectory(path);
                    if (Directory.Exists(path))
                    {
                        return true;
                    }
                    retries--;
                    System.Threading.Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    retries--;
                    bool dirExists = Directory.Exists(path);
                    Trace.WriteLine(ex.Message);
                    if (dirExists)
                    {
                        return true;
                    }
                    System.Threading.Thread.Sleep(200);
                }
            }
            logger.Info("Unable to create directory {0}", path);
            return false;
        }


        private static void DelDir(string dirPath)
        {
            if (!Directory.Exists(dirPath))
                return;
            try
            {
                Directory.Delete(dirPath, true);
            }
            catch
            {
                try
                {
                    // delete failed, try to rename it
                    Directory.Move(dirPath, string.Format("{0}-{1}", dirPath, DateTime.Now.Ticks));
                }
                catch
                {
                    // rename failed, try to clobber it (can be slow so last resort)
                    Utilities.DeleteDirectory(dirPath);
                }
            }
        }

        public static void RemoveTestServer(Process p, string testRoot, bool resetDepot = false)
        {
            // logger.Info("RemoveTestServer");
            if (p != null)
            {
                if (!p.HasExited)
                    p.Kill();
                p.WaitForExit();
                // sleep for a bit to let the system clean up
                System.Threading.Thread.Sleep(100);
            }

            // some tests are changing archives.  resetDepot forces the test to regenerate the server P4ROOT next time
            if (resetDepot)
                DeleteDirectory(testRoot);

            MainTest.RememberToCleanup(rubbishBin);
            MainTest.RememberToCleanup(testRoot);
        }

        private static void MoveDir(string srcDir, string targDir)
        {
            //logger.Info("Moving {0} to {1}", srcDir, targDir);
            try
            {
                int retries = 60;
                while (Directory.Exists(srcDir) && retries > 0)
                {
                    try
                    {
                        // Try to rename it
                        Directory.Move(srcDir, targDir);
                        //must have worked
                        break;
                    }
                    catch
                    {
                        retries--;
                        System.Threading.Thread.Sleep(1000);
                        if (retries <= 0)
                        {
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Info("In DeployP4TestServer, Directory.Move failed: {0}", ex.Message);
                // rename failed, try to clobber it (can be slow so last resort)
                Utilities.DeleteDirectory(srcDir);
            }
        }
    }
}
