using System.Collections.Generic;

namespace ATC.config
{
	public class CSESettings : SettingsModule
	{
		public bool CSE_enabled = false;

		public List<string> scripts = new List<string>();

		public int timeout = 2500;
	}
}
