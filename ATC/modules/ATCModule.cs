using ATC.config;
using System;
using System.IO;

namespace ATC
{
	public abstract class ATCModule
	{
		private ATCLogger logger;
		private string modulename;

		protected SettingsModule settings_base;
		protected string workingDirectory;
		protected DateTime startTime;

		public ATCModule(ATCLogger l, SettingsModule s, string wd, string m)
		{
			logger = l;
			settings_base = s;
			modulename = m;
			workingDirectory = Path.Combine(wd, modulename);
			Directory.CreateDirectory(workingDirectory);
			startTime = DateTime.Now;
		}

		protected void log(string text = "")
		{
			logger.log(modulename, text);
		}

		protected void logHeader(string fullname)
		{
			string r_l = new String('#', ((79 - 4 - fullname.Length) / 2));
			string r_r = new String('#', (79 - 4 - fullname.Length - r_l.Length));

			string date = String.Format(@"Date: {0}", startTime.ToString("R"));
			string d_l = new String(' ', ((79 - 2 - date.Length) / 2));
			string d_r = new String(' ', (79 - 2 - date.Length - d_l.Length));

			log();
			log(String.Format(@"{0}  {1}  {2}", r_l, fullname, r_r));
			log(String.Format("#{0}#", new String(' ', 77)));
			log(String.Format("#{0}{1}{2}#", d_l, date, d_r));
			log(String.Format("#{0}#", new String(' ', 77)));
			log(new String('#', 79));
			log();
		}

		public abstract void start();
	}
}
