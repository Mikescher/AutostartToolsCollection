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
		public string path = "";
		public string parameter = "";
		public int timeout = 2500;

		public bool failOnStdErr   = true;
		public bool failOnTimeout  = true;
		public bool failOnExitCode = true;

		public string Name
		{
			get
			{
				if (string.IsNullOrWhiteSpace(parameter))
					return (Path.GetFileName(path) ?? "").Trim();

				try
				{
					if (parameter.StartsWith("\"") && parameter.EndsWith("\""))
						return Path.GetFileName(path) + " " + Path.GetFileName(parameter.Trim('"'));
				}
				catch (ArgumentException)
				{
					// nothing
				}
				try
				{
					return (Path.GetFileName(path) + " " + parameter).Trim();
				}
				catch (ArgumentException)
				{
					// nothing
				}
				return path + " " + parameter;
			}
		}
	}
}
