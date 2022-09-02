// From case 00564291   "Bridge is intercepting Control-C before C# can handle it."
// job100836


using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Perforce.P4;

namespace ControlCTest
{
	// When p4bridge.dll is loaded by P4API.NET,
	// it registers its own Win32 SetConsoleCtrlHandler handler which is not cooperative.
	// Once p4bridge.dll is loaded, any Ctrl-C event causes the process to be terminated without notice.
	//
	// Run this program. It will ask you to press Ctrl-C twice.
	// The first time, p4bridge.dll is not loaded.
	// The console event handler is able to see the event and respond.
	// The second time, p4bridge.dll is loaded.
	// The console event handler does not see the console event.
	// Instead, the process was terminating without notice.
	//
	// fixed the initial problem, Added post DLL load routine which
	// resets SIGNAL to DISDFL and prevents the bridge from exiting
	// from a signal by default.
	//
	// Also discovered that this application crashes on linux if logging is not enabled 
	//  if I initialize the logDelegate to null, it doesn't crash...
	
	class Program
    {
		static CancellationTokenSource s_cancellationTokenSource;
		private static P4CallBacks.LogMessageDelegate logDelegate = null;

		private static bool bridge_logging = false;
		
		public static void Main(string[] args)
		{
			if (bridge_logging)
			{
				logDelegate = new P4CallBacks.LogMessageDelegate(LogFunction);
				P4Debugging.SetBridgeLogFunction(logDelegate);
			}
			else
			{
				P4Debugging.SetBridgeLogFunction(null);
			}
			
			Console.WriteLine("Testing Ctrl-C handling");
			Console.CancelKeyPress += Console_CancelKeyPress;
			try
			{
				WaitForCtrlC();

				TriggerP4ApiLoad();

				WaitForCtrlC();

                Console.WriteLine("Finished testing");
            }
			finally
			{
				Console.CancelKeyPress -= Console_CancelKeyPress;
			}
		}

		private static void LogFunction(int loglevel, string file, int line, string message)
		{
			Console.WriteLine($"{file}({line}): {message}");
		}

		private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			Console.WriteLine("Ctrl-C received.");
			s_cancellationTokenSource?.Cancel();
			e.Cancel = true;
		}

		private static bool LogIfP4BridgeIsLoaded()
		{
			// This routine does not work on OSX or Linux, it looks like the bridge isn't getting added to the Modules list
			string dllname = "p4bridge.dll";

			var isLoaded = Process.GetCurrentProcess().Modules
				.Cast<ProcessModule>()
				.Any(m => string.Equals(m.ModuleName, dllname, StringComparison.OrdinalIgnoreCase));
			
			Console.WriteLine($"p4bridge is loaded: {isLoaded}");
			return isLoaded;
		}

		private static void WaitForCtrlC()
		{
			using (s_cancellationTokenSource = new CancellationTokenSource())
			{
				// LogIfP4BridgeIsLoaded();
				Console.WriteLine("Waiting for Ctrl-C event. I will print out a message when I see the event.");
				s_cancellationTokenSource.Token.WaitHandle.WaitOne();
			}
		}

		private static void TriggerP4ApiLoad()
		{
			Console.WriteLine("Triggering P4API to load p4bridge.dll");

			using (var repository = new Repository(new Server(new ServerAddress("bad address"))))
			{
				try
				{
					repository.Connection.Connect(null);
				}
				catch (P4Exception ex)
				when (ex.ErrorCode == 824577061)
				{
				}
			}
		}
	}
}
