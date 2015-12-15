using ATC.config;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ATC.modules.AWC
{
	public class AutoWallChange : ATCModule
	{
		private AWCSettings Settings { get { return (AWCSettings)SettingsBase; } }

		private string exclusionfileHD1080;

		public AutoWallChange(ATCLogger l, AWCSettings s, string wd)
			: base(l, s, wd, "AWC")
		{

		}

		public override void Start()
		{
			LogHeader("AutoWallChange");

			if (!Settings.AWC_enabled)
			{
				Log("AWC not enabled.");
				return;
			}

			if (String.IsNullOrWhiteSpace(Settings.pathWallpaper))
			{
				Log("pathWallpaper not set");
				return;
			}

			if (Path.GetExtension(Settings.pathWallpaper) != ".bmp")
			{
				Log("pathWallpaper must direct to a *.bmp file");
				return;
			}

			exclusionfileHD1080 = Path.Combine(WorkingDirectory, "exclusions.config");

			MonitorConstellation mc = ScreenHelper.getMonitorConstellation();

			Log("Monitor Constellation: " + ScreenHelper.mcToString(mc));

			if (mc == MonitorConstellation.Other)
			{
				Log("Unknown Monitor Constellation");
				return;
			}

			Log();

			bool succ = false;
			switch (mc)
			{
				case MonitorConstellation.Dual_HD1080_SXGA:
					succ = setImage_HD1080_SXGA();
					break;
				case MonitorConstellation.Dual_HD1080_HD1080:
					succ = setImage_HD1080_HD1080();
					break;
				case MonitorConstellation.Single_HD1080:
					succ = setImage_HD1080();
					break;
				case MonitorConstellation.Single_SXGA:
					succ = setImage_SXGA();
					break;
				default:
					throw new ArgumentException();
			}

			if (!succ)
				return;

			WindowsWallpaperAPI.Set(Settings.pathWallpaper, WindowsWallpaperAPI.W_WP_Style.Tiled);
		}

		private bool setImage_SXGA()
		{
			if (String.IsNullOrWhiteSpace(Settings.pathWallpaperLD_normal))
			{
				Log("pathWallpaperLD_normal not set");
				return false;
			}

			RandomImageAccessor r = new RandomImageAccessor(Settings.pathWallpaperLD_normal);

			int found, excluded;
			string choosen;

			Image i = r.getRandomImage(out found, out excluded, out choosen);

			if (i == null)
			{
				Log("No Images found.");
				return false;
			}

			Log("[ SXGA ]  Images Found:    " + found);
			Log("[ SXGA ]  Image Choosen:   " + Path.GetFileName(choosen));

			i.Save(Settings.pathWallpaper, ImageFormat.Bmp);

			return true;
		}

		private bool setImage_HD1080()
		{
			if (String.IsNullOrWhiteSpace(Settings.pathWallpaperHD))
			{
				Log("pathWallpaperHD not set");
				return false;
			}

			int found, excluded;
			string choosen;

			RandomImageAccessor r = new RandomImageAccessor(Settings.pathWallpaperHD, exclusionfileHD1080);
			Image i = r.getRandomImage(out excluded, out found, out choosen);

			if (i == null)
			{
				Log("No Images found.");
				return false;
			}

			Log("[HD1080]  Images Found:    " + found);
			Log("[HD1080]  Images Excluded: " + excluded);
			Log("[HD1080]  Image Choosen:   " + Path.GetFileName(choosen));

			i.Save(Settings.pathWallpaper, ImageFormat.Bmp);

			return true;
		}

		private bool setImage_HD1080_HD1080()
		{
			if (String.IsNullOrWhiteSpace(Settings.pathWallpaperHD))
			{
				Log("pathWallpaperHD not set");
				return false;
			}

			int found, excluded;
			string choosen;

			RandomImageAccessor r = new RandomImageAccessor(Settings.pathWallpaperHD, exclusionfileHD1080);
			Image i1 = r.getRandomImage(out excluded, out found, out choosen);

			if (i1 == null)
			{
				Log("No Images found.");
				return false;
			}
			Log("[HD1080]  Images Found:    " + found);
			Log("[HD1080]  Images Excluded: " + excluded);
			Log("[HD1080]  Image Choosen:   " + Path.GetFileName(choosen));

			Log();

			RandomImageAccessor r2 = new RandomImageAccessor(Settings.pathWallpaperHD, exclusionfileHD1080);
			Image i2 = r.getRandomImage(out excluded, out found, out choosen);

			if (i2 == null)
			{
				Log("No Images found.");
				return false;
			}
			Log("[HD1080]  Images Found:    " + found);
			Log("[HD1080]  Images Excluded: " + excluded);
			Log("[HD1080]  Image Choosen:   " + Path.GetFileName(choosen));

			WallpaperJoiner joiner = new WallpaperJoiner(MonitorConstellation.Dual_HD1080_HD1080, Settings);
			Image final = joiner.Join(i1, i2);

			final.Save(Settings.pathWallpaper, ImageFormat.Bmp);

			return true;
		}

		private bool setImage_HD1080_SXGA()
		{
			if (String.IsNullOrWhiteSpace(Settings.pathWallpaperHD))
			{
				Log("pathWallpaperHD not set");
				return false;
			}

			if (String.IsNullOrWhiteSpace(Settings.pathWallpaperLD_background))
			{
				Log("pathWallpaperLD_background not set");
				return false;
			}

			if (String.IsNullOrWhiteSpace(Settings.pathWallpaperTUX))
			{
				Log("pathWallpaperTUX not set");
				return false;
			}

			int found, excluded;
			string choosen;

			RandomImageAccessor r1 = new RandomImageAccessor(Settings.pathWallpaperHD, exclusionfileHD1080);
			Image i1 = r1.getRandomImage(out excluded, out found, out choosen);

			if (i1 == null)
			{
				Log("No Images found.");
				return false;
			}
			Log("[HD1080]  Images Found:    " + found);
			Log("[HD1080]  Images Excluded: " + excluded);
			Log("[HD1080]  Image Choosen:   " + Path.GetFileName(choosen));

			Log();

			RandomImageAccessor r2 = new RandomImageAccessor(Settings.pathWallpaperLD_background);
			Image i2 = r2.getRandomImage(out excluded, out found, out choosen);

			if (i2 == null)
			{
				Log("No Images found.");
				return false;
			}

			Log("[ SXGA ]  Images Found:    " + found);
			Log("[ SXGA ]  Image Choosen:   " + Path.GetFileName(choosen));

			Log();

			RandomImageAccessor r3 = new RandomImageAccessor(Settings.pathWallpaperTUX);
			Image i3 = r3.getRandomImage(out excluded, out found, out choosen);

			if (i3 == null)
			{
				Log("No Images found.");
				return false;
			}


			Log("[ TUX  ]  Images Found:    " + found);
			Log("[ TUX  ]  Image Choosen:   " + Path.GetFileName(choosen));

			WallpaperJoiner joiner = new WallpaperJoiner(MonitorConstellation.Dual_HD1080_SXGA, Settings);
			Image final = joiner.Join(i1, i2, i3);

			final.Save(Settings.pathWallpaper, ImageFormat.Bmp);

			return true;
		}
	}
}
