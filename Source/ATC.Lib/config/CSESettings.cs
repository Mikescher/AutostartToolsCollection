using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ATC.Lib.config
{
	public class CSESettings : SettingsModule
	{
		public bool CSE_enabled = false;

		public List<CSEEntry> scripts = new List<CSEEntry>();
	}

	public class CSEEntry
	{
		public string name = "";
		public string path = "";
		public string parameter = null;
		public string workingdirectory = "";
		public string timeout = "2.5s";

		public bool failOnStdErr   = true;
		public bool failOnTimeout  = true;
		public bool failOnExitCode = true;

		public int TimeoutMilliseconds
        {
			get
            {
				return timeout.Split(' ').Select(v => 
				{
					v = v.ToLower().Trim();

					if (v.EndsWith("ms")) return int.Parse(v.Substring(0, v.Length - 2));
					if (v.EndsWith("s")) return (int)(1000 * double.Parse(v.Substring(0, v.Length - 1), CultureInfo.InvariantCulture));
					if (v.EndsWith("m")) return (int)(60 * 1000 * double.Parse(v.Substring(0, v.Length - 1), CultureInfo.InvariantCulture));
					if (v.EndsWith("h")) return (int)(60 * 60 * 1000 * double.Parse(v.Substring(0, v.Length - 1), CultureInfo.InvariantCulture));
					if (v.EndsWith("d")) return (int)(24 * 60 * 60 * 1000 * double.Parse(v.Substring(0, v.Length - 1), CultureInfo.InvariantCulture));

					throw new ArgumentException(timeout + " - " + v);

				}).Aggregate((a,b) => a+b);
			}
        }
	}
}
