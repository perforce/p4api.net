// change false to true if you want to debug the threadp procs
// currently changes to 30 seconds to wait for a thread to exit 
// and 15 minutes to run the test (and debug), up from 1 second
// and 15 seconds
#if false && DEBUG 
#define SlowJoin
#endif

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Perforce.P4;
using NLog;

namespace p4api.net.unit.test
{
	[TestClass]
	public class P4ServerMultiThreadingTest
	{
        // When debugging often need to slow down time outs to keep from triggering unwanted thread aborts 
#if SlowJoin
		const int JoinTime = 30000;
#else
		const int JoinTime = 1000;
#endif
        private String TestDir = "";

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private UnitTestConfiguration configuration;
        public TestContext TestContext { get; set; }
		
        [TestInitialize]
        public void SetupTest()
        {
            configuration = UnitTestSettings.GetApplicationConfiguration();
            TestDir = configuration.TestDirectory;
            Utilities.LogTestStart(TestContext);
        }
        [TestCleanup]
        public void CleanupTest()
        {
            Utilities.LogTestFinish(TestContext);
        }

		int cmdCnt1 = 0;
		int cmdCnt2 = 0;
		int cmdCnt3 = 0;
		int cmdCnt4 = 0;
		int cmdCnt5 = 0;
		int cmdCnt6 = 0;

		bool run = true;

		TimeSpan delay = TimeSpan.FromMilliseconds(5);

        class RunThreadException
        {
            public RunThreadException(int threadNumber, Exception threadException)
            {
                ThreadNumber = threadNumber;
                ThreadException = threadException;
            }
            public int ThreadNumber { get; set; }
            public Exception ThreadException { get; set; }
        }
        List<RunThreadException> runThreadExeptions = new List<RunThreadException>();

        private void cmdThreadProc1(object obj)
		{
            CancellationToken token = (CancellationToken)obj;
			try
			{
                P4Server server = serverMT.getServer();

				while (run)
				{
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
					cmdCnt1++;
                    using (P4Command cmd1 = new P4Command(server, "fstat", false, "//depot/..."))
                    {
                        DateTime startedAt = DateTime.Now;

					WriteLine(string.Format("Thread 1 starting command: {0:X8}, at {1}",
                            cmd1.CommandId, startedAt.ToLongTimeString()));

					P4CommandResult result = cmd1.Run();

					WriteLine(string.Format("Thread 1 Finished command: {0:X8}, at {1}, run time {2} Milliseconds",
                            cmd1.CommandId, startedAt.ToLongTimeString(),
                            (DateTime.Now - startedAt).TotalMilliseconds));

					P4CommandResult lastResult = server.LastResults;

					if (!result.Success)
					{
                            string msg = string.Format("Thread 1, fstat failed:{0}",
                                (result.ErrorList != null && result.ErrorList.Count > 0)
                                    ? result.ErrorList[0].ErrorMessage
                                    : "<unknown error>");
                        WriteLine(msg);
                        throw new Exception(msg);
                    }
					else
					{
                            WriteLine(string.Format("Thread 1, fstat Success:{0}",
                                (result.InfoOutput != null && result.InfoOutput.Count > 0)
                                    ? result.InfoOutput[0].Message
                                    : "<no output>"));
					}

					//Assert.IsTrue(result.Success);
					if (delay != TimeSpan.Zero)
					{
						Thread.Sleep(delay);
					}
				}
                }
                
				WriteLine(string.Format("Thread 1 cleanly exited after running {0} commands", cmdCnt1));
				return;
			}
			catch (ThreadAbortException)
			{
				Thread.ResetAbort();
				return;
			}
			catch (Exception ex)
			{
                WriteLine($"cmdThreadProc1 failed with exception: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                runThreadExeptions.Add(new RunThreadException(1, ex));
            }
		}

        private void cmdThreadProc2(object obj)
		{
            CancellationToken token = (CancellationToken)obj;
			try
			{
                P4Server server = serverMT.getServer();

                while (run)
				{
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
					cmdCnt2++; 
                    using (P4Command cmd2 = new P4Command(server, "dirs", false, "//depot/*"))
                    {

					DateTime StartedAt = DateTime.Now;

					WriteLine(string.Format("Thread 2 starting command: {0:X8}, at {1}",
						cmd2.CommandId, StartedAt.ToLongTimeString()));

					P4CommandResult result = cmd2.Run();

					WriteLine(string.Format("Thread 2 Finished command: {0:X8}, at {1}, run time {2} Milliseconds",
                            cmd2.CommandId, StartedAt.ToLongTimeString(),
                            (DateTime.Now - StartedAt).TotalMilliseconds));

					P4CommandResult lastResult = server.LastResults;

                    // Assert is not allowed on the non primary thread
                    Assert.AreEqual(result.Success, lastResult.Success);

					if (!result.Success)
					{
                            string msg = string.Format("Thread 2, fstat failed:{0}",
                                (result.ErrorList != null && result.ErrorList.Count > 0)
                                    ? result.ErrorList[0].ErrorMessage
                                    : "<unknown error>");
                        WriteLine(msg);
                        throw new Exception(msg);
                    }
					else
					{
                            WriteLine(string.Format("Thread 2, dirs Success:{0}",
                                (result.InfoOutput != null && result.InfoOutput.Count > 0)
                                    ? result.InfoOutput[0].Message
                                    : "<no output>"));
					}

					//Assert.IsTrue(result.Success);
					if (delay != TimeSpan.Zero)
					{
						Thread.Sleep(delay);
					}
				}
                }
				WriteLine(string.Format("Thread 2 cleanly exited after running {0} commands", cmdCnt2));
			}
			catch (ThreadAbortException)
			{
				Thread.ResetAbort();
				return;
			}
			catch (Exception ex)
			{
                WriteLine($"cmdThreadProc2 failed with exception: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                runThreadExeptions.Add(new RunThreadException(2, ex));
            }
		}

        private void cmdThreadProc3(object obj)
		{
            CancellationToken token = (CancellationToken)obj;
			try
			{
                P4Server server = serverMT.getServer();

                while (run)
				{
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
					cmdCnt3++;
                    using (P4Command cmd3 = new P4Command(server, "edit", false, "-n", "//depot/..."))
                    {

					DateTime StartedAt = DateTime.Now;

					WriteLine(string.Format("Thread 3 starting command: {0:X8}, at {1}",
						cmd3.CommandId, StartedAt.ToLongTimeString()));

					P4CommandResult result = cmd3.Run();

					WriteLine(string.Format("Thread 3 Finished command: {0:X8}, at {1}, run time {2} Milliseconds",
                            cmd3.CommandId, StartedAt.ToLongTimeString(),
                            (DateTime.Now - StartedAt).TotalMilliseconds));

					P4CommandResult lastResult = server.LastResults;

					if (!result.Success)
					{
                            string msg = string.Format("Thread 3, fstat failed:{0}",
                                (result.ErrorList != null && result.ErrorList.Count > 0)
                                    ? result.ErrorList[0].ErrorMessage
                                    : "<unknown error>");
                        WriteLine(msg);
                        throw new Exception(msg);
                    }
					else
					{
                            WriteLine(string.Format("Thread 3, edit Success:{0}",
                                (result.InfoOutput != null && result.InfoOutput.Count > 0)
                                    ? result.InfoOutput[0].Message
                                    : "<no output>"));
					}

					//Assert.IsTrue(result.Success);
					if (delay != TimeSpan.Zero)
					{
						Thread.Sleep(delay);
					}
				}
                }
				WriteLine(string.Format("Thread 3 cleanly exited after running {0} commands", cmdCnt3));
			}
			catch (ThreadAbortException)
			{
				Thread.ResetAbort();
				return;
			}
			catch (Exception ex)
			{
                WriteLine($"cmdThreadProc3 failed with exception: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                runThreadExeptions.Add(new RunThreadException(3, ex));
            }
		}

        private void cmdThreadProc4(object obj)
		{
            CancellationToken token = (CancellationToken)obj;
			try
			{
                P4Server server = serverMT.getServer();

                while (run)
				{
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
					cmdCnt4++;

						string val = P4Server.Get("P4IGNORE");
						bool _p4IgnoreSet = !string.IsNullOrEmpty(val);

						if (_p4IgnoreSet)
						{
							WriteLine(string.Format("P4Ignore is set, {0}", val));
						}
						else
						{
							WriteLine("P4Ignore is not set");
						}

                    using (P4Command cmd4 = new P4Command(server, "fstat", false, "//depot/..."))
                    {
					DateTime StartedAt = DateTime.Now;

					WriteLine(string.Format("Thread 4 starting command: {0:X8}, at {1}",
						cmd4.CommandId, StartedAt.ToLongTimeString()));

					P4CommandResult result = cmd4.Run();

					WriteLine(string.Format("Thread 4 Finished command: {0:X8}, at {1}, run time {2} Milliseconds",
                            cmd4.CommandId, StartedAt.ToLongTimeString(),
                            (DateTime.Now - StartedAt).TotalMilliseconds));

					P4CommandResult lastResult = server.LastResults;
					if (!result.Success)
                    {
                            string msg = string.Format("Thread 4, fstat failed:{0}",
                                (result.ErrorList != null && result.ErrorList.Count > 0)
                                    ? result.ErrorList[0].ErrorMessage
                                    : "<unknown error>");
						WriteLine(msg);
                        throw new Exception(msg);
                    }
					else
					{
                            WriteLine(string.Format("Thread 4, fstat Success:{0}",
                                (result.InfoOutput != null && result.InfoOutput.Count > 0)
                                    ? result.InfoOutput[0].Message
                                    : "<no output>"));
					}

					//Assert.IsTrue(result.Success);
					if (delay != TimeSpan.Zero)
					{
						Thread.Sleep(delay);
					}
				}
                }
				WriteLine(string.Format("Thread 4 cleanly exited after running {0} commands", cmdCnt4));
			}
			catch (ThreadAbortException)
			{
				Thread.ResetAbort();
				return;
			}
			catch (Exception ex)
			{
                WriteLine($"cmdThreadProc4 failed with exception: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                runThreadExeptions.Add(new RunThreadException(4, ex));
            }
		}

        private void cmdThreadProc5(object obj)
		{
            CancellationToken token = (CancellationToken)obj;
			try
			{
                P4Server server = serverMT.getServer();

                while (run)
				{
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
					cmdCnt5++;
                    using (P4Command cmd5 = new P4Command(server, "dirs", false, "//depot/*"))
                    {

                        DateTime startedAt = DateTime.Now;

					WriteLine(string.Format("Thread 5 starting command: {0:X8}, at {1}",
                            cmd5.CommandId, startedAt.ToLongTimeString()));

					P4CommandResult result = cmd5.Run();

					WriteLine(string.Format("Thread 5 Finished command: {0:X8}, at {1}, run time {2} Milliseconds",
                            cmd5.CommandId, startedAt.ToLongTimeString(),
                            (DateTime.Now - startedAt).TotalMilliseconds));

					P4CommandResult lastResult = server.LastResults;

                        // Asserts are not allowed on the non primary thread

					if (!result.Success)
					{
                            string msg = string.Format("Thread 5, fstat failed:{0}",
                                (result.ErrorList != null && result.ErrorList.Count > 0)
                                    ? result.ErrorList[0].ErrorMessage
                                    : "<unknown error>");
                        WriteLine(msg);
                        throw new Exception(msg);
                    }
					else
					{
                            WriteLine(string.Format("Thread 5, dirs Success:{0}",
                                (result.InfoOutput != null && result.InfoOutput.Count > 0)
                                    ? result.InfoOutput[0].Message
                                    : "<no output>"));
					}

					if (delay != TimeSpan.Zero)
					{
						Thread.Sleep(delay);
					}
				}
                }
                WriteLine($"Thread 5 cleanly exited after running {cmdCnt5} commands");
			}
			catch (ThreadAbortException)
			{
				Thread.ResetAbort();
				return;
			}
			catch (Exception ex)
			{
                WriteLine($"cmdThreadProc5 failed with exception: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                runThreadExeptions.Add(new RunThreadException(5, ex));
            }
		}

        private void cmdThreadProc6a(object obj)
		{
            CancellationToken token = (CancellationToken)obj;
			try
			{
                if (token.IsCancellationRequested)
                {
                    return;
                }
                P4Server server = serverMT.getServer();

                    cmdCnt6++;
                using (P4Command cmd6 = new P4Command(server, "edit", false, "-n", "//depot/..."))
                {

					DateTime StartedAt = DateTime.Now;
					WriteLine(string.Format("{2} starting command: {0:X8}, at {1}",
						cmd6.CommandId, StartedAt.ToLongTimeString(), t6Name));

					P4CommandResult result = cmd6.Run();

					WriteLine(string.Format("{3} Finished command: {0:X8}, at {1}, run time {2} Milliseconds",
                        cmd6.CommandId, StartedAt.ToLongTimeString(), (DateTime.Now - StartedAt).TotalMilliseconds,
                        t6Name));

					P4CommandResult lastResult = server.LastResults;

					Assert.AreEqual(result.Success, lastResult.Success);
					if (result.InfoOutput != null)
					{
						Assert.AreEqual(result.InfoOutput.Count, lastResult.InfoOutput.Count);
					}
					else
					{
						Assert.IsNull(lastResult.InfoOutput);
					}

					if (result.ErrorList != null)
					{
						Assert.AreEqual(result.ErrorList.Count, lastResult.ErrorList.Count);
					}
					else
					{
						Assert.IsNull(result.ErrorList);
					}

					if (result.TextOutput != null)
					{
						Assert.AreEqual(result.TextOutput, lastResult.TextOutput);
					}
					else
					{
						Assert.IsNull(lastResult.TextOutput);
					}

					if (result.TaggedOutput != null)
					{
						Assert.AreEqual(result.TaggedOutput.Count, lastResult.TaggedOutput.Count);
					}
					else
					{
						Assert.IsNull(lastResult.TaggedOutput);
					}

					Assert.AreEqual(result.Cmd, lastResult.Cmd);
					if (result.CmdArgs != null)
					{
						Assert.AreEqual(result.CmdArgs.Length, lastResult.CmdArgs.Length);
					}
					else
					{
						Assert.IsNull(lastResult.CmdArgs);
					}

					if (!result.Success)
					{
                        string msg = string.Format("Thread 6, fstat failed:{0}",
                            (result.ErrorList != null && result.ErrorList.Count > 0)
                                ? result.ErrorList[0].ErrorMessage
                                : "<unknown error>");
                        WriteLine(msg);
                        throw new Exception(msg);
                    }
					else
					{
                        WriteLine(string.Format("{1}, edit Success:{0}",
                            (result.InfoOutput != null && result.InfoOutput.Count > 0)
                                ? result.InfoOutput[0].Message
                                : "<no output>", t6Name));
					}

					if (delay != TimeSpan.Zero)
					{
						Thread.Sleep(delay);
					}
                }

                WriteLine($"{t6Name} cleanly exited after running 1 of {cmdCnt6} commands");
			}
			catch (ThreadAbortException)
			{
				Thread.ResetAbort();
				return;
			}
			catch (Exception ex)
			{
                WriteLine($"cmdThreadProc6 failed with exception: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                runThreadExeptions.Add(new RunThreadException(6, ex));
            }
		}

		string t6Name = null;

        // thread 6 continually spawns new threads to run commands
		// this generates large numbers of threads that are used only once
		// to test that we can handle large numbers of unique thread ids
        private void cmdThreadProc6(object obj)
		{
            CancellationToken token = (CancellationToken)obj;
            try
            {
                int threadNum = 0;
                while (run)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    CancellationTokenSource cts6a = new CancellationTokenSource();

                    t6Name = string.Format("RunAsyncTest Thread t6a-{0}", threadNum.ToString());
                    Thread t6a = new Thread(new ParameterizedThreadStart(cmdThreadProc6a));
                    t6a.Name = t6Name;
                    threadNum++;

                    WriteLine("Spawning " + t6Name);

                    t6a.Start(cts6a.Token);

                    if (t6a.Join(JoinTime) == false)
                    {
                        WriteLine(t6Name + " did not cleanly exit");
                        cts6a.Cancel();
                    }
                }
                WriteLine($"Thread 6 cleanly exited after running {cmdCnt6} commands");
            }
			catch (ThreadAbortException)
			{
                Thread.ResetAbort();
				return;
			}
            catch (Exception ex)
            {
                WriteLine($"cmdThreadProc1 failed with exception: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                runThreadExeptions.Add(new RunThreadException(6, ex));
            }
		}
		P4ServerMT serverMT = null;

#if _LOG_TO_FILE
		static System.IO.StreamWriter sw = null;

		public static void WriteLine(string msg)
		{
			lock (sw)
			{
				sw.WriteLine(msg);
				sw.Flush();
			}
		}

		public static void LogBridgeMessage( int log_level,String source,String message )
		{
			WriteLine(string.Format("[{0}] {1}:{2}", source, log_level, message));
		}

		private static LogFile.LogMessageDelegate LogFn = new LogFile.LogMessageDelegate(LogBridgeMessage);

#else
		public void WriteLine(string msg)
		{
			Trace.WriteLine(msg);
		}
#endif
		/// <summary>
		///A test for Running multiple command concurrently
		///</summary>
		[TestMethod()]
		public void RunAsyncTest()
		{
#if _LOG_TO_FILE
#if _WINDOWS
			string logpath = @"\Logs\RunAsyncTestLog.Txt";
#else
			string logpath = "/tmp/RunAsyncTestLog.Txt";
#endif
			using (sw = new System.IO.StreamWriter(logpath, true))
			{
				LogFile.SetLoggingFunction(LogFn);
#endif
            string serverAddr = configuration.ServerPort;
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			// turn off exceptions for this test
			ErrorSeverity oldExceptionLevel = P4Exception.MinThrowLevel;
			P4Exception.MinThrowLevel = ErrorSeverity.E_NOEXC;

			for (int i = 0; i < 1; i++) // run once for ascii, change < 1 to < 2 to also run once for unicode
			{
                var cptype = (Utilities.CheckpointType)i;

                Process p4d = Utilities.DeployP4TestServer(TestDir, cptype, TestContext.TestName);
                Assert.IsNotNull(p4d, "Setup Failure");

				try
				{
					using (serverMT = new P4ServerMT(serverAddr, user, pass, ws_client))
					{
                        using (CancellationTokenSource cts1 = new CancellationTokenSource())
                        using (CancellationTokenSource cts2 = new CancellationTokenSource())
                        using (CancellationTokenSource cts3 = new CancellationTokenSource())
                        using (CancellationTokenSource cts4 = new CancellationTokenSource())
                        using (CancellationTokenSource cts5 = new CancellationTokenSource())
                        using (CancellationTokenSource cts6 = new CancellationTokenSource())
                        {
                            using (P4Server server = serverMT.getServer())
                            {
                                if (cptype == Utilities.CheckpointType.U)
                                    Assert.IsTrue(server.UseUnicode,
                                        "Unicode server detected as not supporting Unicode");
						else
                                    Assert.IsFalse(server.UseUnicode,
                                        "Non Unicode server detected as supporting Unicode");
                            }

						cmdCnt1 = 0;
						cmdCnt2 = 0;
						cmdCnt3 = 0;
						cmdCnt4 = 0;
						cmdCnt5 = 0;
						cmdCnt6 = 0;

						run = true;

                            Thread t1 = new Thread(new ParameterizedThreadStart(cmdThreadProc1));
						t1.Name = "RunAsyncTest Thread t1";
                            Thread t2 = new Thread(new ParameterizedThreadStart(cmdThreadProc2));
						t2.Name = "RunAsyncTest Thread t2";
                            Thread t3 = new Thread(new ParameterizedThreadStart(cmdThreadProc3));
						t3.Name = "RunAsyncTest Thread t3";

                            Thread t4 = new Thread(new ParameterizedThreadStart(cmdThreadProc4));
						t4.Name = "RunAsyncTest Thread t4";
                            Thread t5 = new Thread(new ParameterizedThreadStart(cmdThreadProc5));
						t5.Name = "RunAsyncTest Thread t5";
                            Thread t6 = new Thread(new ParameterizedThreadStart(cmdThreadProc6));
						t6.Name = "RunAsyncTest Thread t6";

                            t1.Start(cts1.Token);
						Thread.Sleep(TimeSpan.FromSeconds(5)); // wait to start a 4th thread
                            t2.Start(cts2.Token);
                            t3.Start(cts3.Token);
						Thread.Sleep(TimeSpan.FromSeconds(5)); // run a bit 

						run = false; //now stop running commands

						if (t1.Join(JoinTime) == false)
						{
							WriteLine("Thread 1 did not cleanly exit");
                                cts1.Cancel();
						}

						if (t2.Join(JoinTime) == false)
						{
							WriteLine("Thread 2 did not cleanly exit");
                                cts2.Cancel();
						}

						if (t3.Join(JoinTime) == false)
						{
							WriteLine("Thread 3 did not cleanly exit");
                                cts3.Cancel();
						}

                        if (runThreadExeptions.Count > 0)
                        {
                                // one or more run threads threw an exception
                            string msg = string.Empty;
                            foreach (RunThreadException runThreadException in runThreadExeptions)
                            {
                                msg += string.Format("Thread {0} threw exception: {1}",
                                    runThreadException.ThreadNumber,
                                    runThreadException.ThreadException.Message);
                            }

                            Assert.Fail(msg);
                        }

						Thread.Sleep(TimeSpan.FromSeconds(15)); // wait 15 seconds so will disconnect

                            run = true;
                            ;

                            t1 = new Thread(new ParameterizedThreadStart(cmdThreadProc1));
						t1.Name = "RunAsyncTest Thread t1b";
                            t2 = new Thread(new ParameterizedThreadStart(cmdThreadProc2));
						t2.Name = "RunAsyncTest Thread t2b";
                            t3 = new Thread(new ParameterizedThreadStart(cmdThreadProc3));
						t3.Name = "RunAsyncTest Thread t3b";

                            t1.Start(cts1.Token);
                            t2.Start(cts2.Token);
                            t3.Start(cts3.Token);
						Thread.Sleep(TimeSpan.FromSeconds(1)); // wait to start a 4th thread

                            t4.Start(cts4.Token);
						Thread.Sleep(TimeSpan.FromSeconds(2)); // wait to start a 5th thread
                            t5.Start(cts5.Token);
						Thread.Sleep(TimeSpan.FromSeconds(3)); // wait to start a 6th thread
                            t6.Start(cts6.Token);

#if SlowJoin
						Thread.Sleep(TimeSpan.FromMinutes(15)); // run all threads for 15 minutes
#else
                            Thread.Sleep(TimeSpan.FromSeconds(15)); // run all threads for 15 seconds
#endif
						run = false;

						if (t1.Join(JoinTime) == false)
						{
							WriteLine("Thread 1 did not cleanly exit");
                                cts1.Cancel();
						}

						if (t2.Join(JoinTime) == false)
						{
							WriteLine("Thread 2 did not cleanly exit");
                                cts2.Cancel();
						}

						if (t3.Join(JoinTime) == false)
						{
							WriteLine("Thread 3 did not cleanly exit");
                                cts3.Cancel();
						}

						if (t4.Join(JoinTime) == false)
						{
							WriteLine("Thread 4 did not cleanly exit");
                                cts4.Cancel();
						}

						if (t5.Join(JoinTime) == false)
						{
							WriteLine("Thread 5 did not cleanly exit");
                                cts5.Cancel();
						}

						if (t6.Join(JoinTime) == false)
						{
							WriteLine("Thread 6 did not cleanly exit");
                                cts6.Cancel();
						}

                        if (runThreadExeptions.Count > 0)
                        {
                            // one or more run threads threw an excaption
                            string msg = string.Empty;
                            foreach (RunThreadException runThreadException in runThreadExeptions)
                            {
                                    msg += string.Format(
                                        $"Thread {{0}} threw exception: {{1}}{Environment.NewLine}{{2}}",
                                    runThreadException.ThreadNumber,
                                    runThreadException.ThreadException.Message,
                                    runThreadException.ThreadException.StackTrace);
                            }

                            Assert.Fail(msg);
                        }
                        }
                    }
				}
				catch (Exception ex)
				{
                    Assert.Fail($"Test threw an exception: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
			}
			// reset the exception level
			P4Exception.MinThrowLevel = oldExceptionLevel;
#if _LOG_TO_FILE
			}
#endif
		}
	}
}
