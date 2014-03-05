using ATC.config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			log();
			log(String.Format(@"################  {0}  ################",fullname));
			log();
			log(String.Format(@"Date: {0}", startTime.ToString("R")));
			log();
			log();
		}

		public abstract void start();
	}
}
