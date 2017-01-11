using ATC.config;
using MSHC.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ATC.modules.TVC
{
	public class CronScriptExecutor : ATCModule
	{
		private CSESettings settings { get { return (CSESettings)SettingsBase; } }

		public CronScriptExecutor(ATCLogger l, CSESettings s, string wd)
			: base(l, s, wd, "CSE")
		{
			// NOP
		}

		public override void Start()
		{
			LogHeader("CronScriptExecutor");

			if (!settings.CSE_enabled)
			{
				Log("CSE not enabled.");
				return;
			}

			if (settings.parallel)
			{
				ExecuteParallel(settings.scripts);
				Log();
			}
			else
			{
				foreach (var script in settings.scripts)
				{
					Log("Execute " + script.Name);

					ExecuteScript(script);
					Log();
				}
			}

		}

		private void ExecuteParallel(List<CSEEntry> scripts)
		{
			if (! scripts.Any()) return;

			int timeout = scripts.Max(p => p.timeout);

			List<Thread> processes = new List<Thread>();
			var entryDict = new Dictionary<Thread, CSEEntry>();

			int i = 0;
			foreach (var entry in scripts)
			{
				int id = i++;
				Thread t = new Thread(() =>
				{
					if (entry.path.Contains("\\") && !File.Exists(entry.path))
					{
						Log(string.Format(@"Script {0} does not exist - skipping execution", entry.path));
						return;
					}

					Log(string.Format(@"Start script [{1}] {0}", entry.Name, id));

					ProcessOutput output;
					try
					{
						output = ProcessHelper.ProcExecute(entry.path, entry.parameter);
					}
					catch (Exception e)
					{
						Log(string.Format(@"Process creation failed for {0}", entry.Name));
						Log(e.ToString());
						return;
					}

					string op1 = "========================  [" + id + "]-STDOUT  ========================";
					string op2 = "========================  [" + id + "]-STDERR  ========================";

					StringBuilder b = new StringBuilder();
					b.AppendLine(string.Format(@"Finished script [{2}] {0} with {1}", entry.Name, output.ExitCode, id));
					b.AppendLine();
					b.AppendLine(op1 + "\n" + output.StdOut + "\n" + new string('=', op1.Length));
					b.AppendLine();
					b.AppendLine();
					if (!string.IsNullOrWhiteSpace(output.StdErr))
					{
						b.AppendLine(op2 + "\n" + output.StdErr + "\n" + new string('=', op2.Length));
						b.AppendLine();
						b.AppendLine();
					}

					Log(b.ToString());
				});

				processes.Add(t);
				entryDict.Add(t, entry);

				t.Start();
			}

			int ltime = timeout;
			while (ltime > 0 && processes.Any())
			{
				Thread.Sleep(32);
				ltime -= 32;

				foreach (var finProc in processes.Where(p => !p.IsAlive).ToList())
				{
					Log("Process " + entryDict[finProc].Name + " finished.");
					processes.Remove(finProc);
				}
			}

			foreach (var errProc in processes)
			{
				Log(entryDict[errProc].Name + " Process still running - continue ATC...");
			}

			if (!processes.Any())
			{
				Log("All processes finished");
			}

		}

		private void ExecuteScript(CSEEntry entry)
		{
			if (entry.path.Contains("\\") && !File.Exists(entry.path))
			{
				Log(string.Format(@"File {0} does not exist", entry.path));
				return;
			}

			var start = new ProcessStartInfo
			{
				FileName = entry.path,
				UseShellExecute = true,
				Arguments = entry.parameter,
			};
			if (entry.hideConsole) start.WindowStyle = ProcessWindowStyle.Hidden;

			using (Process process = Process.Start(start))
			{
				if (process == null)
				{
					Log(string.Format(@"Process creation failed for {0}", entry.Name));
					return;
				}

				int ltime = entry.timeout;
				while (ltime > 0 && !process.HasExited)
				{
					Thread.Sleep(50);
					ltime -= 50;
				}

				if (process.HasExited)
				{
					Log("Process finished.");
				}
				else
				{
					Log("Process still running - continue ATC...");
				}
			}
		}
	}
}
