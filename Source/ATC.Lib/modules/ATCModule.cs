using ATC.Lib.config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ATC.Lib
{
	public abstract class ATCModule
	{
		private readonly ATCLogger logger;

		protected readonly string Modulename;

		protected readonly SettingsModule SettingsBase;
		protected readonly string WorkingDirectory;
		protected DateTime StartTime;

		protected ATCModule(ATCLogger l, SettingsModule s, string wd, string m)
		{
			logger = l;
			SettingsBase = s;
			Modulename = m;
			WorkingDirectory = Path.Combine(wd, Modulename);
			Directory.CreateDirectory(WorkingDirectory);
			StartTime = DateTime.Now;
		}

		protected void LogRoot(string text = "")
		{
			logger.Log(Modulename, null, text);
		}

		protected void LogProxy(ATCTaskProxy p, string text = "")
		{
			logger.Log(Modulename, p.Subcat, text);
		}

		protected void LogProxyOnly(ATCTaskProxy p, string text = "")
		{
			logger.Log(Modulename, p.Subcat, text, true);
		}

		protected void LogNewFile(string[] path, string text)
		{
			logger.LogNewFile(Modulename, path, text);
		}

		protected void LogHeader(string fullname)
		{
			string rL = new string('#', ((79 - 4 - fullname.Length) / 2));
			string rR = new string('#', (79 - 4 - fullname.Length - rL.Length));

			string date = string.Format(@"Date: {0}", StartTime.ToString("R"));
			string dL = new string(' ', ((79 - 2 - date.Length) / 2));
			string dR = new string(' ', (79 - 2 - date.Length - dL.Length));

			LogRoot();
			LogRoot(string.Format(@"{0}  {1}  {2}", rL, fullname, rR));
			LogRoot(string.Format("#{0}#", new string(' ', 77)));
			LogRoot(string.Format("#{0}{1}{2}#", dL, date, dR));
			LogRoot(string.Format("#{0}#", new string(' ', 77)));
			LogRoot(new string('#', 79));
			LogRoot();
		}

		public static void ShowExtMessage(string title, string msg)
		{
			var path = Path.GetTempFileName();
			File.WriteAllText(path, msg);

			var workDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"ATC\");
			var fn = Path.Combine(workDir, "SimpleMessagePresenter.exe");

			var p = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = fn,
					Arguments = $"\"{title.Replace('"', '\'')}\" \"{path}\"",
					WorkingDirectory = workDir,
				}
			};

			p.Start();
		}

		protected void ShowExternalMessage(string title, string msg)
		{
			ShowExtMessage(title, msg);
		}

		public abstract void Start();

		public abstract List<ATCTaskProxy> Init(ATCTaskProxy root);
	}
}
