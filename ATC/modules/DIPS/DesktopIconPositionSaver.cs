﻿using ATC.config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
namespace ATC.modules.DIPS
{
	public class DesktopIconPositionSaver : ATCModule
	{
		private DIPSSettings settings { get { return (DIPSSettings)settings_base; } }

		public DesktopIconPositionSaver(ATCLogger l, DIPSSettings s, string wd)
			: base(l, s, wd, "DIPS")
		{

		}

		public override void start()
		{
			logHeader("DesktopIconPositionSaver");

			if (!settings.DIPS_enabled)
			{
				log("DIPS not enabled.");
				return;
			}

			LVItem[] dicons = RemoteListView.GetDesktopListView();

			log(String.Format(@"Found {0} Icons on Desktop", dicons.Length));

			JObject iconsav = new JObject();

			JArray iconarr = new JArray();

			foreach (LVItem lvicon in dicons)
			{
				JObject jo = new JObject();
				jo["title"] = lvicon.Name;
				jo["x"] = lvicon.Location.x;
				jo["y"] = lvicon.Location.y;

				iconarr.Add(jo);
			}

			iconsav.Add("icons", iconarr);

			string iconsav_content = iconsav.ToString(Formatting.Indented);

			vcontrolIconSav(iconsav_content);
		}

		private bool IsValidDateTimeFileName(string path)
		{
			string fn = Path.GetFileName(path);
			string fnwe = Path.GetFileNameWithoutExtension(path);

			DateTime t;

			if (Regex.IsMatch(fn, @"[0-9]{4}_[0-9]{2}_[0-9]{2}_[0-9]{2}_[0-9]{2}.json"))
				return DateTime.TryParseExact(fnwe, "yyyy_MM_dd_HH_mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out t);
			else
				return false;
		}

		private void vcontrolIconSav(string iconsav_content)
		{
			string outputDirectory = Path.Combine(workingDirectory, "history");
			Directory.CreateDirectory(outputDirectory);

			string filename = string.Format("{0:yyyy}_{0:MM}_{0:dd}_{0:HH}_{0:mm}.json", startTime);
			string filepath = Path.Combine(outputDirectory, filename);

			if (File.Exists(filepath))
			{
				log(String.Format(@"File {0} does already exist in DIPS/history directory", filepath));
				return;
			}

			List<string> versions = Directory.EnumerateFiles(outputDirectory).
				Where(p => IsValidDateTimeFileName(p)).
				OrderByDescending(p => DateTime.ParseExact(Path.GetFileNameWithoutExtension(p), "yyyy_MM_dd_HH_mm", CultureInfo.InvariantCulture)).
				ToList();

			string last = "";

			if (versions.Count > 0)
				last = File.ReadAllText(versions[0]);

			string last_hash = StringHashing.CalculateMD5Hash(last);
			string curr_hash = StringHashing.CalculateMD5Hash(iconsav_content);

			if (last_hash != curr_hash)
			{
				log(String.Format("Desktop Icons differ from last Version - creating new Entry"));
				log(String.Format("MD5 Current File: {0}", curr_hash));
				log(String.Format("MD5 Previous File: {0}", last_hash));

				try
				{
					File.WriteAllText(filepath, iconsav_content);
					log(string.Format(@"Desktop icons succesfully backuped to '{0}'", filepath));
				}
				catch (Exception ex)
				{
					log(string.Format(@"ERROR backuping icons to '{0}' : {1}", filepath, ex.Message));
				}
			}
			else
			{
				log(String.Format("No changes in desktop icons detected (MD5: {0})", curr_hash));
			}
		}
    }
}
