using System.Collections.Generic;
using System.IO;

namespace ATC.config
{
	public class TVCSettings : SettingsModule
	{
		public bool TVC_enabled = false;

		public List<TVCEntry> paths = new List<TVCEntry>();

		public string output = "";

		public bool cleanHistory = true;
	}

	public class TVCEntry
	{
		public string path = string.Empty;
		public string name = null;

		public List<string> jpath = null;
		public bool formatOutput = false; 

		public string GetFoldername()
		{
			return name ?? Path.GetFileName(path);
		}

		public string GetOutputPath(TVCSettings settings)
		{
			return Path.Combine(settings.output, GetFoldername());
		}
	}
}
