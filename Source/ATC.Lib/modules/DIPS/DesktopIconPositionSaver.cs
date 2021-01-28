using ATC.Lib.config;
using DesktopAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
namespace ATC.Lib.modules.DIPS
{
	public class DesktopIconPositionSaver : ATCModule
	{
		private DIPSSettings settings { get { return (DIPSSettings)SettingsBase; } }

		private ATCTaskProxy rootTask;
		private ATCTaskProxy _task;

		public DesktopIconPositionSaver(ATCLogger l, DIPSSettings s, string wd)
			: base(l, s, wd, "DIPS")
		{

		}

		public override List<ATCTaskProxy> Init(ATCTaskProxy root)
		{
			rootTask = root;

			if (!settings.DIPS_enabled)
			{
				LogRoot("DIPS not enabled.");
				rootTask.FinishSuccess();
				_task = null;
				return new List<ATCTaskProxy>();
			}

			_task = new ATCTaskProxy($"Save Desktop", Modulename, Guid.NewGuid());
			return new List<ATCTaskProxy>() { _task };
		}

		public override void Start()
		{
			if (_task == null) return;
			_task.Start();

			LogHeader("DesktopIconPositionSaver");

			LVItem[] dicons = RemoteListView.GetDesktopListView();

			LogProxy(_task, $@"Found {dicons.Length} Icons on Desktop");

			JObject iconsav = new JObject();

			JObject screeninfo = new JObject();
			screeninfo["count"] = Screen.AllScreens.Length;
			screeninfo["constellation"] = ScreenHelper.mcToString(ScreenHelper.getMonitorConstellation());

			JObject s_prim = null;
			JObject s_sec = null;

			JArray screens = new JArray();
			foreach (Screen  scr in Screen.AllScreens)
			{
				JObject screenobj = new JObject();
				screenobj["primary"] = scr.Primary;
				screenobj["x"] = scr.Bounds.X;
				screenobj["y"] = scr.Bounds.Y;
				screenobj["width"] = scr.Bounds.Width;
				screenobj["height"] = scr.Bounds.Height;
				screenobj["name"] = scr.DeviceName;

				if (scr == ScreenHelper.getPrimary())
				{
					s_prim = screenobj;
				}
				else if (scr == ScreenHelper.getSecondary())
				{
					s_sec = screenobj;
				}

				screens.Add(screenobj);
			}

			if (s_prim != null)
				screeninfo.Add("primary", s_prim);
			else
				screeninfo["primary"] = "null";

			if (s_sec != null)
				screeninfo.Add("secondary", s_sec);
			else
				screeninfo["secondary"] = "null";

			screeninfo.Add("AllScreens", screens);

			iconsav.Add("screens", screeninfo);

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

			VcontrolIconSav(iconsav_content);

			_task.FinishSuccess();
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

		private void VcontrolIconSav(string iconsav_content)
		{
			string outputDirectory = Path.Combine(WorkingDirectory, "history");
			Directory.CreateDirectory(outputDirectory);

			string filename = string.Format("{0:yyyy}_{0:MM}_{0:dd}_{0:HH}_{0:mm}.json", StartTime);
			string filepath = Path.Combine(outputDirectory, filename);

			if (File.Exists(filepath))
			{
				LogProxy(_task, $"File {filepath} does already exist in DIPS/history directory");
				_task.SetErrored();
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
				LogProxy(_task, string.Format("Desktop Icons differ from last Version - creating new Entry"));
				LogProxy(_task, "");
				LogProxy(_task, string.Format("MD5 Current File: {0}", curr_hash));
				LogProxy(_task, string.Format("MD5 Previous File: {0}", last_hash));
				LogProxy(_task, "");

				try
				{
					File.WriteAllText(filepath, iconsav_content);
					LogProxy(_task, string.Format(@"Desktop icons succesfully backuped to '{0}'", filepath));
					_task.FinishSuccess();
					return;
				}
				catch (Exception ex)
				{
					LogProxy(_task, string.Format(@"ERROR backuping icons to '{0}' : {1}", filepath, ex.Message));
					_task.SetErrored();
					return;
				}
			}
			else
			{
				LogProxy(_task, string.Format("No changes in desktop icons detected (MD5: {0})", curr_hash));
				_task.FinishSuccess();
				return;
			}
		}
	}
}
