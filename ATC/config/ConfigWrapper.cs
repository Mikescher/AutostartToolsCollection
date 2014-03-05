using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATC.config
{
	public class ConfigWrapper
	{
		private const string logfilename = "config.json";

		public ATCSettings settings;

		private string workingDirectory;

		public ConfigWrapper(string wd)
		{
			workingDirectory = wd;
		}

		public void load()
		{
			string path = Path.Combine(workingDirectory, logfilename);

			if (File.Exists(path)) 
			{
				string json = File.ReadAllText(path);
				settings = (ATCSettings)JsonConvert.DeserializeObject(json, typeof(ATCSettings));
			}
			else
			{
				settings = new ATCSettings();
			}

			
		}

		public void save()
		{
			Directory.CreateDirectory(workingDirectory);

			string path = Path.Combine(workingDirectory, logfilename);
			string json = JsonConvert.SerializeObject(settings, Formatting.Indented);

			File.WriteAllText(path, json);
		}
	}
}
