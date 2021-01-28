using Newtonsoft.Json;
using System;
using System.IO;
using ATC.Lib.Json;

namespace ATC.Lib.config
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
					var jss = new JsonSerializerSettings { Converters = new JsonConverter[] { new JsonGenericDictionaryOrArrayConverter() } };

					settings = JsonConvert.DeserializeObject<ATCSettings>(json, jss);
				}
				catch (Exception e)
				{
					logger.Log("ATC", null, "Could not load Config-File");
					logger.Log("ATC", null, "    " + e.Message);
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

			var jss = new JsonSerializerSettings
			{
				//Converters = new JsonConverter[] { new JsonGenericDictionaryOrArrayConverter() },
				ContractResolver = new SkipEmptyContractResolver(),
				NullValueHandling = NullValueHandling.Ignore
			};

			string path = Path.Combine(workingDirectory, logfilename);
			string json = JsonConvert.SerializeObject(settings, Formatting.Indented, jss);

			File.WriteAllText(path, json);
		}
	}
}
