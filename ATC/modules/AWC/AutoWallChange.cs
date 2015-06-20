using ATC.config;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ATC.modules.AWC
{
	public class AutoWallChange : ATCModule
	{
		private AWCSettings settings { get { return (AWCSettings)settings_base; } }

		private string exclusionfile_HD1080;

		public AutoWallChange(ATCLogger l, AWCSettings s, string wd)
			: base(l, s, wd, "AWC")
		{

		}

		public override void start()
		{
			logHeader("AutoWallChange");

			if (!settings.AWC_enabled)
			{
				log("AWC not enabled.");
				return;
			}

			if (String.IsNullOrWhiteSpace(settings.pathWallpaper))
			{
				log("pathWallpaper not set");
				return;
			}

			if (Path.GetExtension(settings.pathWallpaper) != ".bmp")
			{
				log("pathWallpaper must direct to a *.bmp file");
				return;
			}

			exclusionfile_HD1080 = Path.Combine(workingDirectory, "exclusions.config");

			MonitorConstellation mc = ScreenHelper.getMonitorConstellation();

			log("Monitor Constellation: " + ScreenHelper.mcToString(mc));

			if (mc == MonitorConstellation.Other)
			{
				log("Unknown Monitor Constellation");
				return;
			}

			log();

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

			WindowsWallpaperAPI.Set(settings.pathWallpaper, WindowsWallpaperAPI.W_WP_Style.Tiled);
		}

		private bool setImage_SXGA()
		{
			if (String.IsNullOrWhiteSpace(settings.pathWallpaperLD_normal))
			{
				log("pathWallpaperLD_normal not set");
				return false;
			}

			RandomImageAccessor r = new RandomImageAccessor(settings.pathWallpaperLD_normal);

			int found, excluded;
			string choosen;

			Image i = r.getRandomImage(out found, out excluded, out choosen);

			if (i == null)
			{
				log("No Images found.");
				return false;
			}

			log("[ SXGA ]  Images Found:    " + found);
			log("[ SXGA ]  Image Choosen:   " + Path.GetFileName(choosen));

			i.Save(settings.pathWallpaper, ImageFormat.Bmp);

			return true;
		}

		private bool setImage_HD1080()
		{
			if (String.IsNullOrWhiteSpace(settings.pathWallpaperHD))
			{
				log("pathWallpaperHD not set");
				return false;
			}

			int found, excluded;
			string choosen;

			RandomImageAccessor r = new RandomImageAccessor(settings.pathWallpaperHD, exclusionfile_HD1080);
			Image i = r.getRandomImage(out excluded, out found, out choosen);

			if (i == null)
			{
				log("No Images found.");
				return false;
			}

			log("[HD1080]  Images Found:    " + found);
			log("[HD1080]  Images Excluded: " + excluded);
			log("[HD1080]  Image Choosen:   " + Path.GetFileName(choosen));

			i.Save(settings.pathWallpaper, ImageFormat.Bmp);

			return true;
		}

		private bool setImage_HD1080_HD1080()
		{
			if (String.IsNullOrWhiteSpace(settings.pathWallpaperHD))
			{
				log("pathWallpaperHD not set");
				return false;
			}

			int found, excluded;
			string choosen;

			RandomImageAccessor r = new RandomImageAccessor(settings.pathWallpaperHD, exclusionfile_HD1080);
			Image i1 = r.getRandomImage(out excluded, out found, out choosen);

			if (i1 == null)
			{
				log("No Images found.");
				return false;
			}
			log("[HD1080]  Images Found:    " + found);
			log("[HD1080]  Images Excluded: " + excluded);
			log("[HD1080]  Image Choosen:   " + Path.GetFileName(choosen));

			log();

			RandomImageAccessor r2 = new RandomImageAccessor(settings.pathWallpaperHD, exclusionfile_HD1080);
			Image i2 = r.getRandomImage(out excluded, out found, out choosen);

			if (i2 == null)
			{
				log("No Images found.");
				return false;
			}
			log("[HD1080]  Images Found:    " + found);
			log("[HD1080]  Images Excluded: " + excluded);
			log("[HD1080]  Image Choosen:   " + Path.GetFileName(choosen));

			WallpaperJoiner joiner = new WallpaperJoiner(MonitorConstellation.Dual_HD1080_HD1080, settings);
			Image final = joiner.Join(i1, i2);

			final.Save(settings.pathWallpaper, ImageFormat.Bmp);

			return true;
		}

		private bool setImage_HD1080_SXGA()
		{
			if (String.IsNullOrWhiteSpace(settings.pathWallpaperHD))
			{
				log("pathWallpaperHD not set");
				return false;
			}

			if (String.IsNullOrWhiteSpace(settings.pathWallpaperLD_background))
			{
				log("pathWallpaperLD_background not set");
				return false;
			}

			if (String.IsNullOrWhiteSpace(settings.pathWallpaperTUX))
			{
				log("pathWallpaperTUX not set");
				return false;
			}

			int found, excluded;
			string choosen;

			RandomImageAccessor r1 = new RandomImageAccessor(settings.pathWallpaperHD, exclusionfile_HD1080);
			Image i1 = r1.getRandomImage(out excluded, out found, out choosen);

			if (i1 == null)
			{
				log("No Images found.");
				return false;
			}
			log("[HD1080]  Images Found:    " + found);
			log("[HD1080]  Images Excluded: " + excluded);
			log("[HD1080]  Image Choosen:   " + Path.GetFileName(choosen));

			log();

			RandomImageAccessor r2 = new RandomImageAccessor(settings.pathWallpaperLD_background);
			Image i2 = r2.getRandomImage(out excluded, out found, out choosen);

			if (i2 == null)
			{
				log("No Images found.");
				return false;
			}

			log("[ SXGA ]  Images Found:    " + found);
			log("[ SXGA ]  Image Choosen:   " + Path.GetFileName(choosen));

			log();

			RandomImageAccessor r3 = new RandomImageAccessor(settings.pathWallpaperTUX);
			Image i3 = r3.getRandomImage(out excluded, out found, out choosen);

			if (i3 == null)
			{
				log("No Images found.");
				return false;
			}


			log("[ TUX  ]  Images Found:    " + found);
			log("[ TUX  ]  Image Choosen:   " + Path.GetFileName(choosen));

			WallpaperJoiner joiner = new WallpaperJoiner(MonitorConstellation.Dual_HD1080_SXGA, settings);
			Image final = joiner.Join(i1, i2, i3);

			final.Save(settings.pathWallpaper, ImageFormat.Bmp);

			return true;
		}
	}
}
