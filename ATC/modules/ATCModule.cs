using ATC.config;
using System;
using System.IO;

namespace ATC
{
	public abstract class ATCModule
	{
		private readonly ATCLogger logger;
		private readonly string modulename;

		protected readonly SettingsModule SettingsBase;
		protected readonly string WorkingDirectory;
		protected DateTime StartTime;

		protected ATCModule(ATCLogger l, SettingsModule s, string wd, string m)
		{
			logger = l;
			SettingsBase = s;
			modulename = m;
			WorkingDirectory = Path.Combine(wd, modulename);
			Directory.CreateDirectory(WorkingDirectory);
			StartTime = DateTime.Now;
		}

		protected void Log(string text = "")
		{
			logger.log(modulename, text);
		}

		protected void LogHeader(string fullname)
		{
			string rL = new string('#', ((79 - 4 - fullname.Length) / 2));
			string rR = new string('#', (79 - 4 - fullname.Length - rL.Length));

			string date = string.Format(@"Date: {0}", StartTime.ToString("R"));
			string dL = new string(' ', ((79 - 2 - date.Length) / 2));
			string dR = new string(' ', (79 - 2 - date.Length - dL.Length));

			Log();
			Log(string.Format(@"{0}  {1}  {2}", rL, fullname, rR));
			Log(string.Format("#{0}#", new string(' ', 77)));
			Log(string.Format("#{0}{1}{2}#", dL, date, dR));
			Log(string.Format("#{0}#", new string(' ', 77)));
			Log(new string('#', 79));
			Log();
		}

		public abstract void Start();
	}
}
