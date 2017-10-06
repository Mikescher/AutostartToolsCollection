using System.Collections.Generic;
using System.IO;
using ATC.Json;
using Newtonsoft.Json;

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

		public List<TVCPostProcessorEntry> postprocessors = new List<TVCPostProcessorEntry>();

		public string GetFoldername()
		{
			return name ?? Path.GetFileName(path);
		}

		public string GetOutputPath(TVCSettings settings)
		{
			return Path.Combine(settings.output, GetFoldername());
		}
	}

	public class TVCPostProcessorEntry
	{
		public string name = null;

		[JsonConverter(typeof(JsonGenericDictionaryOrArrayConverter))]
		public IDictionary<string, string> settings = null;
	}
}
