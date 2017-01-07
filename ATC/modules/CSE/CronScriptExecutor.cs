using ATC.config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

			List<Process> processes = new List<Process>();
			var entryDict = new Dictionary<Process, CSEEntry>();
			try
			{
				foreach (var entry in scripts)
				{
					if (entry.path.Contains("\\") && !File.Exists(entry.path))
					{
						Log(string.Format(@"Script {0} does not exist - skipping execution", entry.path));
						continue;
					}

					var start = new ProcessStartInfo
					{
						FileName = entry.path,
						UseShellExecute = true
					};
					if (entry.hideConsole) start.WindowStyle = ProcessWindowStyle.Hidden;

					var proc = Process.Start(start);
					if (proc != null)
					{
						processes.Add(proc);
						entryDict.Add(proc, entry);
					}
					else
					{
						Log(string.Format(@"Process creation failed for {0}", entry.Name));
					}
				}
				
				int ltime = timeout;
				while (ltime > 0 && processes.Any())
				{
					Thread.Sleep(32);
					ltime -= 32;

					foreach (var finProc in processes.Where(p => p.HasExited).ToList())
					{
						Log("Process " + entryDict[finProc].Name +" finished.");
						processes.Remove(finProc);

						finProc.Dispose();
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
			finally
			{
				foreach (var proc in processes.Where(p => p != null))
				{
					try
					{
						proc.Dispose();
					}
					catch (Exception)
					{
						 /* swallow */
					}
				}
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
