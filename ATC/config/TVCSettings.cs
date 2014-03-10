using System.Collections.Generic;

namespace ATC.config
{
	public class TVCSettings : SettingsModule
	{
		public bool TVC_enabled = false;

		public List<string> paths = new List<string>();

		public string output = "";

		public bool cleanHistory = true;
	}
}
