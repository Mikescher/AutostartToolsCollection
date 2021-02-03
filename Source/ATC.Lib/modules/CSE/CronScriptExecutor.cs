using ATC.Lib.config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using MSHC.Util.Helper;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace ATC.Lib.modules.CSE
{
	public class CronScriptExecutor : ATCModule
	{
		private CSESettings settings { get { return (CSESettings)SettingsBase; } }

		private ATCTaskProxy rootTask;
		private List<(CSEEntry script, ATCTaskProxy proxy)> _tasks = new List<(CSEEntry, ATCTaskProxy)>();

		public CronScriptExecutor(ATCLogger l, CSESettings s, string wd)
			: base(l, s, wd, "CSE")
		{
			// NOP
		}

		public override List<ATCTaskProxy> Init(ATCTaskProxy root)
		{
			rootTask = root;

			if (!settings.CSE_enabled)
			{
				LogRoot("CSE not enabled.");
				rootTask.FinishSuccess();
				return new List<ATCTaskProxy>();
			}

			if (settings.scripts.Select(p => p.name).Distinct().Count() != settings.scripts.Count)
			{
				rootTask.SetErrored();
				throw new Exception("Script names in CSE must be unique");
			}

			_tasks = settings.scripts.Select(p => (p, new ATCTaskProxy($"Execute {p.name}", Modulename, Guid.NewGuid()))).ToList();
			
			return _tasks.Select(p => p.proxy).ToList();
		}

		public override void Start()
		{
			LogHeader("CronScriptExecutor");

			ExecuteParallel();
			LogRoot();
		}

		private void ExecuteParallel()
		{
			if (!_tasks.Any()) return;

			var processes = new List<(int id, string name, Thread thread, (CSEEntry script, ATCTaskProxy proxy) entry)>();
			var timeoutProcesses = new ConcurrentQueue<int>();

			var i = 0;
			foreach (var entry in _tasks)
			{
				var id = i++;
				var t = new Thread(() => { ExecuteSingle(entry.script, entry.proxy, id, timeoutProcesses); });
				Thread.Sleep(500);

				processes.Add((id, entry.script.name, t, entry));

				t.Start();
			}

			while (processes.Any())
			{
				Thread.Sleep(32);

				foreach (var finProc in processes.Where(p => !p.thread.IsAlive).ToList())
				{
					LogRoot("Process " + finProc.entry.script.name + " finished.");
					processes.Remove(finProc);
				}

				while (timeoutProcesses.TryDequeue(out var id))
                {
					var rm = processes.FirstOrDefault(p => p.id == id);
					if (rm != default)
					{
						LogRoot("Process " + rm.entry.script.name + " timed out.");
						processes.Remove(rm);
					}
				}
			}

			LogRoot("Finished");
		}

		private void ExecuteSingle(CSEEntry entry, ATCTaskProxy proxy, int id, ConcurrentQueue<int> timeoutQueue)
		{
			proxy.Start();

			if (entry.path.Contains("\\") && !File.Exists(entry.path))
			{
				LogProxy(proxy, $@"Script {entry.path} does not exist - skipping execution");
				proxy.SetErrored();
				return;
			}

			LogProxy(proxy, $@"Start script [{id}] {entry.name}");

			var currThread = Thread.CurrentThread;
			var timeout = entry.TimeoutMilliseconds;
			var timeoutFail = entry.failOnTimeout;
			(new Thread(() =>
			{
				// Watchdog
				var sw = Stopwatch.StartNew();
				for (;;)
                {
					if (!currThread.IsAlive) return;

					if (sw.ElapsedMilliseconds > timeout)
                    {
						if (timeoutFail)
						{
							LogProxy(proxy, $"Process '{entry.name}' timeout (fail-on-timeout) -- continue ATC...");
							proxy.SetErrored();
						}
						else
						{
							LogProxy(proxy, $"Process '{entry.name}' still running (no-fail-on-timeout) - continue ATC...");
						}

						timeoutQueue.Enqueue(id);
						return;
					}

					Thread.Sleep(100);
                }
			}){ IsBackground = true }).Start();

			ProcessOutput output;
			try
			{
				LogProxy(proxy, string.Empty);
				LogProxy(proxy, $"> {entry.path} {entry.parameter.Replace("\r", "\\r").Replace("\n", "\\n")}");
				LogProxy(proxy, string.Empty);

				var sw = Stopwatch.StartNew();
				output = ProcessHelper.ProcExecute(entry.path, entry.parameter, string.IsNullOrWhiteSpace(entry.workingdirectory) ? null : entry.workingdirectory, (s, txt) =>
				{
					if (s == ProcessHelperStream.StdErr) LogProxyOnly(proxy, $"[E] {txt}");
					if (s == ProcessHelperStream.StdOut) LogProxyOnly(proxy, $"[O] {txt}");
				});
				sw.Stop();

				LogProxy(proxy, string.Empty);
				LogProxy(proxy, string.Empty);
				LogProxy(proxy, $"Exitcode: {output.ExitCode}");
				LogProxy(proxy, $"Finished after {sw.Elapsed:mm\\:ss\\.fff}");
				LogProxy(proxy, string.Empty);
			}
			catch (Exception e)
			{
				LogProxy(proxy, $@"Process creation failed for {entry.name}");
				LogProxy(proxy, e.ToString());
				proxy.SetErrored();
				return;
			}

			var dlog = $"> {output.Command}\n\n\n\n{output.StdCombined}\n\n\n\nExitcode: {output.ExitCode}";

			LogNewFile(new[]{"Output", FilenameHelper.StripStringForFilename(entry.name), $"Run_{DateTime.Now:yyyy-MM-dd_HH-mm-ss_ffff}.txt"}, dlog);

			var op1 = "========================  [" + id + "]-STDOUT  ========================";
			var op2 = "========================  [" + id + "]-STDERR  ========================";

			var b = new StringBuilder();
			b.AppendLine(string.Format(@"Finished script [{2}] {0} with {1}", entry.name, output.ExitCode, id));
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

			LogRoot(b.ToString());

			if (output.ExitCode != 0 && entry.failOnExitCode)
			{
				LogProxy(proxy, $"Script failed on ExitCode {output.ExitCode}");
				proxy.SetErrored();
				return;
			}
			else if (!string.IsNullOrWhiteSpace(output.StdErr) && entry.failOnStdErr)
			{
				LogProxy(proxy, $"Script failed on stderr output");
				proxy.SetErrored();
				return;
			}
			else
			{
				proxy.FinishSuccess();
				return;
			}
		}
	}
}
