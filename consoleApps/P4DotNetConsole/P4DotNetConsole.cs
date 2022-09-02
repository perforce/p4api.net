// This is a DOTNET Core sample command line for p4
// At first it may not do much other than allow you to type "p4 info", but we'll see

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Perforce.P4;

namespace P4DotNetConsole
{
	
	class Program
    {
		private static string port = "perforce:1666";
		private static string username = "nmorse";
		private static string client = "ws_client";
		
		static CancellationTokenSource s_cancellationTokenSource = new CancellationTokenSource();
		private static P4CallBacks.LogMessageDelegate logDelegate;

		// setting this will give you logging from the bridge.
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
			
			Console.WriteLine("P4DotNetConsole - Welcome!");

			// Set up a Control-C interrupt
			Console.CancelKeyPress += Console_CancelKeyPress;
			try
			{
				WaitForCtrlC();

				RunStuff();

				WaitForCtrlC();

				RunSetName();

				WaitForCtrlC();

                Console.WriteLine("Finished testing");
            }
			finally
			{
				Console.CancelKeyPress -= Console_CancelKeyPress;
				s_cancellationTokenSource.Dispose();
				if (bridge_logging)
                {
					P4Debugging.SetBridgeLogFunction(null);
                }
			}
		}

		private static void LogFunction(int loglevel, string file, int line, string message)
		{
			Console.WriteLine($"{file}({line}): {message}");
		}

		private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs a)
		{
			Console.WriteLine("Ctrl-C received.");
			s_cancellationTokenSource.Cancel();
			a.Cancel = true;
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

		private static void RunStuff()
		{
			Console.WriteLine("Connecting as {0} to {1}",username,port);

			using (var repository = new Repository(new Server(new ServerAddress(port))))
			{
				try
				{
					repository.Connection.Connect(null);
					Console.WriteLine("Connected");
				}
				catch (P4Exception ex)
				when (ex.ErrorCode == 824577061)
				{
				}
			}
		}

		private static void RunSetName()
        {
			// define the server, repository and connection
			Server server = new Server(new ServerAddress(port));
			Repository rep = new Repository(server);
			Connection con = rep.Connection;

			con.UserName = username;
			con.Client = new Client();
			con.Client.Name = client;

			// set the program name and version
			Options options = new Options();
			options["ProgramName"] = "P4DotNetConsole";
			options["ProgramVersion"] = "2021.1.0.0";

			con.Connect(options);

			P4Command cmd = new P4Command(rep, "info", true);


			P4CommandResult results = cmd.Run(new Options());
            foreach (TaggedObject obj in results.TaggedOutput)
            {
				Console.WriteLine(string.Join(Environment.NewLine, obj));
            }

        }
	}
}
