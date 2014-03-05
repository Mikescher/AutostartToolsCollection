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
		protected SettingsModule settings_base;
		protected string workingDirectory;
		private string modulename;

		public ATCModule(ATCLogger l, SettingsModule s, string wd, string m)
		{
			logger = l;
			settings_base = s;
			modulename = m;
			workingDirectory = Path.Combine(wd, modulename);
			Directory.CreateDirectory(workingDirectory);
		}

		protected void log(string text = "") 
		{
			logger.log(modulename, text);
		}

		protected void logHeader(string fullname)
		{
			log(String.Format(@"################  {0}  ################",fullname));
			log();
			log(String.Format(@"Date: {0}", DateTime.Now.ToString("R")));
			log();
			log();
		}

		public abstract void start();
	}
}
