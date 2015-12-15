﻿using ATC.config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace ATC.modules.TVC
{
	public class TextVersionControl : ATCModule
	{
		private TVCSettings settings { get { return (TVCSettings)SettingsBase; } }

		public TextVersionControl(ATCLogger l, TVCSettings s, string wd)
			: base(l, s, wd, "TVC")
		{
			// NOP
		}

		public override void Start()
		{
			LogHeader("TextVersionControl");

			if (!settings.TVC_enabled)
			{
				Log("TVC not enabled.");
				return;
			}

			if (string.IsNullOrWhiteSpace(settings.output))
			{
				Log("Outputpath not set.");
				return;
			}

			if (settings.paths.Count == 0)
			{
				Log("No files in control");
				return;
			}

			foreach (var file in settings.paths)
			{
				if (settings.cleanHistory)
					cleanUpHistory(file);

				vcontrolfile(file);

				Log();
			}
		}

		private bool IsValidDateTimeFileName(string path)
		{
			string fn = Path.GetFileName(path);
			string fnwe = Path.GetFileNameWithoutExtension(path);

			DateTime t;

			if (Regex.IsMatch(fn, @"[0-9]{4}_[0-9]{2}_[0-9]{2}_[0-9]{2}_[0-9]{2}.txt"))
				return DateTime.TryParseExact(fnwe, "yyyy_MM_dd_HH_mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out t);
			else
				return false;
		}

		private void vcontrolfile(TVCEntry file)
		{
			if (!File.Exists(file.path))
			{
				Log(String.Format(@"File {0} does not exist", file.path));
				return;
			}

			string current = File.ReadAllText(file.path);

			if (file.jpath != null)
			{
				try
				{
					current = extractJPath(current, file.jpath);
				}
				catch (Exception ex)
				{
					Log(string.Format(@"ERROR extracting content via jpath:\r\n\r\n{0}", ex.Message));
					return;
				}
			}

			if (file.formatOutput)
			{
				try
				{
					current = formatText(current, Path.GetExtension(file.path).TrimStart('.'));
				}
                catch (Exception ex)
				{
					Log(string.Format(@"ERROR formatting content:\r\n\r\n{0}", ex.Message));
					return;
				}
			}

			string outputpath = file.GetOutputPath(settings);
			Directory.CreateDirectory(outputpath);

			string filename = string.Format("{0:yyyy}_{0:MM}_{0:dd}_{0:HH}_{0:mm}.txt", StartTime);

			string filepath = Path.Combine(outputpath, filename);

			if (File.Exists(filepath))
			{
				Log(String.Format(@"File {0} does already exist in ouput directory", filepath));
				return;
			}

			List<string> versions = Directory.EnumerateFiles(outputpath).
				Where(IsValidDateTimeFileName).
				OrderByDescending(p => DateTime.ParseExact(Path.GetFileNameWithoutExtension(p), "yyyy_MM_dd_HH_mm", CultureInfo.InvariantCulture)).
				ToList();



			string last = "";

			if (versions.Count > 0)
				last = File.ReadAllText(versions[0]);

			string lastHash = StringHashing.CalculateMD5Hash(last);
			string currHash = StringHashing.CalculateMD5Hash(current);

			if (lastHash != currHash)
			{
				Log(String.Format("File {0} differs from last Version - copying current version", file.GetFoldername()));
				Log(String.Format("MD5 Current File:  {0}", currHash));
				Log(String.Format("MD5 Previous File: {0}", lastHash));

				try
				{
					if (file.jpath != null || file.formatOutput)
					{
						File.WriteAllText(filepath, current, Encoding.UTF8);
						Log(string.Format(@"File '{0}' succesfully written to '{1}' (UTF-8)", file.GetFoldername(), filepath));
					}
					else
					{
						File.Copy(file.path, filepath, false);
						Log(string.Format(@"File '{0}' succesfully copied to '{1}'", file.GetFoldername(), filepath));
					}
				}
				catch (Exception ex)
				{
					Log(string.Format(@"ERROR copying File '{0}' to '{1}' : {2}", file.GetFoldername(), filepath, ex.Message));
				}
			}
			else
			{
				Log(String.Format("File {0} remains unchanged (MD5: {1})", file.GetFoldername(), currHash));
			}
		}

		private string formatText(string current, string type)
		{
			if (type.ToLower() == "json")
			{
				return JObject.Parse(current).ToString(Newtonsoft.Json.Formatting.Indented);
			}
			else
			{
				throw new Exception("Can't format filetype " + type);
			}
        }

		private string extractJPath(string current, List<string> jpath)
		{
			JToken jccurrent = JObject.Parse(current);

			foreach (var node in jpath)
			{
				jccurrent = ((JObject)jccurrent).GetValue(node);
			}

			return jccurrent.ToString();
		}

		private void cleanUpHistory(TVCEntry file)
		{
			string outputpath = file.GetOutputPath(settings);
			Directory.CreateDirectory(outputpath);

			List<string> versions = Directory.EnumerateFiles(outputpath).
				Where(IsValidDateTimeFileName).
				OrderByDescending(p => DateTime.ParseExact(Path.GetFileNameWithoutExtension(p), "yyyy_MM_dd_HH_mm", CultureInfo.InvariantCulture)).
				ToList();

			List<string> contents = versions.Select(File.ReadAllText).ToList();

			List<int> deletions = new List<int>();

			for (int i = 1; i < versions.Count; i++)
			{
				if (contents[i] == contents[i - 1])
				{
					deletions.Add(i - 1);
				}
			}

			for (int i = deletions.Count - 1; i >= 0; i--)
			{
				Log(string.Format("Cleaned up duplicate Entry in History for {0}: {1}",
					Path.GetFileNameWithoutExtension(file.GetFoldername()),
					Path.GetFileNameWithoutExtension(versions[deletions[i]])));

				File.Delete(versions[deletions[i]]);
			}
		}
	}
}
