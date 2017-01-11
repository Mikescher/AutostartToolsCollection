﻿using System;
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
		public string path = "";
		public string parameter = "";
		public int timeout = 2500;
		public bool hideConsole = false;

		public object Name
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
				catch (ArgumentException e)
				{
					// nothing
				}

				return (Path.GetFileName(path) + " " + parameter).Trim();
			}
		}
	}
}
