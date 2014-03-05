using ATC.config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ATC.modules.TVC
{
	public class TextVersionControl : ATCModule
	{
		private TVCSettings settings { get { return (TVCSettings)settings_base; } }

		public TextVersionControl(ATCLogger l, TVCSettings s, string wd)
			: base(l, s, wd, "TVC")
		{
			// NOP
		}

		public override void start()
		{
			logHeader("TextVersionControl");

			if (!settings.TVC_enabled)
			{
				log("TVC not enabled.");
				return;
			}

			if (string.IsNullOrWhiteSpace(settings.output))
			{
				log("Outputpath not set.");
				return;
			}

			if (settings.paths.Count == 0)
			{
				log("No files in control");
				return;
			}

			foreach (string file in settings.paths)
			{
				vcontrolfile(file);
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

		private void vcontrolfile(string file)
		{
			if (!File.Exists(file))
			{
				log(String.Format(@"File {0} does not exist", file));
				return;
			}

			string current = File.ReadAllText(file);

			string outputpath = Path.Combine(settings.output, Path.GetFileName(file));
			Directory.CreateDirectory(outputpath);

			string filename = string.Format("{0:yyyy}_{0:MM}_{0:dd}_{0:HH}_{0:mm}.txt", startTime);

			string filepath = Path.Combine(outputpath, filename);

			if (File.Exists(filepath))
			{
				log(String.Format(@"File {0} does already exist in ouput directory", filepath));
				return;
			}

			List<string> versions = Directory.EnumerateFiles(outputpath).
				Where(p => IsValidDateTimeFileName(p)).
				OrderByDescending(p => DateTime.ParseExact(Path.GetFileNameWithoutExtension(p), "yyyy_MM_dd_HH_mm", CultureInfo.InvariantCulture)).
				ToList();

			string last = "";

			if (versions.Count > 0)
				last = File.ReadAllText(versions[0]);

			string last_hash = StringHashing.CalculateMD5Hash(last);
			string curr_hash = StringHashing.CalculateMD5Hash(current);

			if (last_hash != curr_hash)
			{
				log(String.Format("File {0} differs from last Version - copying current version", Path.GetFileName(file)));
				log(String.Format("MD5 Current File: {0}", curr_hash));
				log(String.Format("MD5 Previous File: {0}", last_hash));

				try
				{
					File.Copy(file, filepath, false);
					log(string.Format(@"File '{0}' succesfully copied to '{1}'", file, filepath));
				}
				catch (Exception ex)
				{
					log(string.Format(@"ERROR copying File '{0}' to '{1}' : {2}", file, filepath, ex.Message));
				}
			}
			else
			{
				log(String.Format("File {0} remains unchanged (MD5: {1})", Path.GetFileName(file), curr_hash));
			}
		}
	}
}
