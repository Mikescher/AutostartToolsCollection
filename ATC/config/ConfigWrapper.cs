using Newtonsoft.Json;
using System.IO;

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

		public void load(ATCLogger logger)
		{
			string path = Path.Combine(workingDirectory, logfilename);

			if (File.Exists(path))
			{
				string json = File.ReadAllText(path);
				try
				{
					settings = (ATCSettings)JsonConvert.DeserializeObject(json, typeof(ATCSettings));
				}
				catch
				{
					logger.log("ATC", "Could not load Config-File");
				}
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
