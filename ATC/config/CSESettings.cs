using System.Collections.Generic;
using System.IO;

namespace ATC.config
{

	public class CSESettings : SettingsModule
	{
		public bool CSE_enabled = false;

		public List<CSEEntry> scripts = new List<CSEEntry>();
		
		public bool parallel = false;
	}

	public class CSEEntry
	{
		public string path;
		public int timeout = 2500;
		public bool hideConsole = false;

		public object Name => Path.GetFileName(path);
	}
}
