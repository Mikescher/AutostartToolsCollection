using System;
using System.Collections.Generic;
using System.IO;

namespace ATC.config
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
		public int timeout = 2500;

		public bool failOnStdErr   = true;
		public bool failOnTimeout  = true;
		public bool failOnExitCode = true;
	}
}
