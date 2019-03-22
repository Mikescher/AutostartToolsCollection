using ATC.config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using MSHC.Util.Helper;

namespace ATC.modules.CSE
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

			ExecuteParallel(settings.scripts);
			Log();
		}

		private void ExecuteParallel(List<CSEEntry> scripts)
		{
			if (! scripts.Any()) return;

			var timeout = scripts.Max(p => p.timeout);

			var processes = new List<Thread>();
			var entryDict = new Dictionary<Thread, CSEEntry>();

			var i = 0;
			foreach (var entry in scripts)
			{
				var id = i++;
				var t = new Thread(() => { ExecuteSingle(entry, id); });

				processes.Add(t);
				entryDict.Add(t, entry);

				t.Start();
			}

			var ltime = timeout;
			while (ltime > 0 && processes.Any())
			{
				Thread.Sleep(32);
				ltime -= 32;

				foreach (var finProc in processes.Where(p => !p.IsAlive).ToList())
				{
					var finProcInfo = entryDict[finProc];
					Log("Process " + finProcInfo.Name + " finished.");
					processes.Remove(finProc);
				}
			}

			foreach (var errProc in processes)
			{
				var errProcInfo = entryDict[errProc];
				Log(errProcInfo.Name + " Process still running - continue ATC...");

				if (errProcInfo.failOnTimeout)
				{
					ShowExternalMessage($"CSE :: Process ({errProcInfo.Name}) timeout", "continue ATC...");
				}
			}

			if (!processes.Any())
			{
				Log("All processes finished");
			}

		}

		private void ExecuteSingle(CSEEntry entry, int id)
		{
			if (entry.path.Contains("\\") && !File.Exists(entry.path))
			{
				Log($@"Script {entry.path} does not exist - skipping execution");
				ShowExternalMessage($"CSE :: Script not found", $"Script\n{entry.path}\ndoes not exist for\n{entry.Name}");
				return;
			}

			Log($@"Start script [{id}] {entry.Name}");

			ProcessOutput output;
			try
			{
				output = ProcessHelper.ProcExecute(entry.path, entry.parameter);
			}
			catch (Exception e)
			{
				Log($@"Process creation failed for {entry.Name}");
				Log(e.ToString());
				ShowExternalMessage($@"CSE :: Process creation failed for {entry.Name}", e.ToString());
				return;
			}

			var dlog = $"> {output.Command}\n\n\n\n{output.StdCombined}\n\n\n\nExitcode: {output.ExitCode}";

			LogNewFile(new[]{"Output", FilenameHelper.StripStringForFilename(entry.Name), $"Run_{DateTime.Now:yyyy-MM-dd_HH-mm-ss_ffff}.txt"}, dlog);

			var op1 = "========================  [" + id + "]-STDOUT  ========================";
			var op2 = "========================  [" + id + "]-STDERR  ========================";

			var b = new StringBuilder();
			b.AppendLine(string.Format(@"Finished script [{2}] {0} with {1}", entry.Name, output.ExitCode, id));
			b.AppendLine();
			b.AppendLine($"{op1}\n{output.StdOut}\n{new string('=', op1.Length)}");
			b.AppendLine();
			b.AppendLine();
			if (!string.IsNullOrWhiteSpace(output.StdErr))
			{
				b.AppendLine($"{op2}\n{output.StdErr}\n{new string('=', op2.Length)}");
				b.AppendLine();
				b.AppendLine();
			}

			Log(b.ToString());

			if (output.ExitCode != 0 && entry.failOnExitCode)
			{
				ShowExternalMessage($"CSE :: Script returned error", b.ToString());
			}
			else if (!string.IsNullOrWhiteSpace(output.StdErr) && entry.failOnStdErr)
			{
				ShowExternalMessage($"CSE :: Script returned stderr", b.ToString());
			}
		}
	}
}
