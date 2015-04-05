using ATC.config;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ATC.modules.TVC
{
	public class CronScriptExecutor : ATCModule
	{
		private CSESettings settings { get { return (CSESettings)settings_base; } }

		public CronScriptExecutor(ATCLogger l, CSESettings s, string wd)
			: base(l, s, wd, "CSE")
		{
			// NOP
		}

		public override void start()
		{
			logHeader("CronScriptExecutor");

			if (!settings.CSE_enabled)
			{
				log("CSE not enabled.");
				return;
			}

			foreach (string script in settings.scripts)
			{
				log("Execute " + Path.GetFileName(script));

				executeScript(script);

				log();
			}
		}

		private void executeScript(string file)
		{
			if (!File.Exists(file))
			{
				log(String.Format(@"File {0} does not exist", file));
				return;
			}

			ProcessStartInfo start = new ProcessStartInfo();
			start.FileName = file;
			start.UseShellExecute = true;
			using (Process process = Process.Start(start))
			{
				int ltime = settings.timeout;
				while (ltime > 0 && !process.HasExited)
				{
					Thread.Sleep(50);
					ltime -= 50;
				}

				if (process.HasExited)
				{
					log("Process finished.");
				}
				else
				{
					log("Process still running - continue ATC...");
				}
			}
		}
	}
}
