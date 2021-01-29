using ATC.Lib.config;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ATC.Lib.modules.AWC
{
	public class AutoWallChange : ATCModule
	{
		private AWCSettings Settings => (AWCSettings) SettingsBase;

		private const int TUX_OFFSET = 4;

		private string exclusionfileHD1080;

		private ATCTaskProxy rootTask;
		private ATCTaskProxy _task;

		public AutoWallChange(ATCLogger l, AWCSettings s, string wd)
			: base(l, s, wd, "AWC")
		{

		}

		public override List<ATCTaskProxy> Init(ATCTaskProxy root)
		{
			rootTask = root;

			if (!Settings.AWC_enabled)
			{
				LogRoot("AWC not enabled.");
				rootTask.FinishSuccess();
				_task = null;
				return new List<ATCTaskProxy>();
			}

			_task = new ATCTaskProxy($"Update Wallpaper", Modulename, Guid.NewGuid());
			return new List<ATCTaskProxy>() { _task };
        }

		public override void Start()
		{
			if (_task == null) return;
			_task.Start();

            try
			{
				LogHeader("AutoWallChange");

				if (string.IsNullOrWhiteSpace(Settings.pathWallpaperFile))
				{
					LogProxy(_task, "pathWallpaperFile not set");
					_task.SetErrored();
					rootTask.SetErrored();
					return;
				}

				if (Path.GetExtension(Settings.pathWallpaperFile) != ".bmp")
				{
					LogProxy(_task, "pathWallpaper must direct to a *.bmp file");
					_task.SetErrored();
					rootTask.SetErrored();
					return;
				}

				exclusionfileHD1080 = Path.Combine(WorkingDirectory, "exclusions.config");

				LogProxy(_task, "Monitor Constellation: " + GetConstellationString());

				if (Screen.AllScreens.Any(p => !(Eq(p, 1920, 1080) || Eq(p, 1280, 1024))))
				{
					LogProxy(_task, "Unknown Monitor Constellation");
					_task.SetErrored();
					return;
				}

				LogProxy(_task, "");

				var img = CreateMultiMonitorImage();

				if (img == null)
				{
					LogProxy(_task, "Error while creating MultiMonitor Wallpaper");
					_task.SetErrored();
					return;
				}

				img.Save(Settings.pathWallpaperFile, ImageFormat.Bmp);

				WindowsWallpaperAPI.Set(Settings.pathWallpaperFile, WindowsWallpaperAPI.W_WP_Style.Tiled);

				_task.FinishSuccess();
			}
            catch (Exception e)
            {
				LogRoot("Exception: " + e);
				_task.SetErrored();
				rootTask.SetErrored();
            }
		}

		private string GetConstellationString() => string.Join(" -- ", Screen.AllScreens.Select(p => string.Format("[{0}x{1}]", p.Bounds.Width, p.Bounds.Height)));

		private static bool Eq(Screen s, int w, int h) => s != null && s.Bounds.Width == w && s.Bounds.Height == h;

		private Image CreateMultiMonitorImage()
		{
			Rectangle bounds = GetScreenBounds();
			Point offset = GetScreenOffset();

			Bitmap result = new Bitmap(bounds.Width, bounds.Height);

			using (var graphics = Graphics.FromImage(result))
			{
				foreach (var screen in Screen.AllScreens)
				{
					var img = GetImage(screen);
					if (img == null) return null;
					graphics.DrawImage(img, screen.Bounds.Location.X + offset.X, screen.Bounds.Location.Y + offset.Y, screen.Bounds.Width, screen.Bounds.Height);
				}
			}

			result = TileImageY(result, bounds, offset);
			result = TileImageX(result, bounds, offset);

			return result;
		}

		private Point GetScreenOffset()
		{
			int minX = Screen.PrimaryScreen.Bounds.X;
			int minY = Screen.PrimaryScreen.Bounds.Y;

			foreach (var b in Screen.AllScreens.Select(p => p.Bounds))
			{
				minX = Math.Min(minX, b.X);
				minY = Math.Min(minY, b.Y);
			}

			return new Point(-minX, -minY);
		}

		private Rectangle GetScreenBounds()
		{
			int minX = Screen.PrimaryScreen.Bounds.X;
			int minY = Screen.PrimaryScreen.Bounds.Y;

			int maxX = Screen.PrimaryScreen.Bounds.Right;
			int maxY = Screen.PrimaryScreen.Bounds.Bottom;

			foreach (var b in Screen.AllScreens.Select(p => p.Bounds))
			{
				minX = Math.Min(minX, b.X);
				minY = Math.Min(minY, b.Y);

				maxX = Math.Max(maxX, b.Right);
				maxY = Math.Max(maxY, b.Bottom);
			}
			
			return new Rectangle(minX, minY, maxX - minX, maxY - minY);
		}

		private Image GetImage(Screen screen)
		{
			if (Eq(screen, 1920, 1080))
			{
				int found, excluded;
				string choosen;

				RandomImageAccessor r = new RandomImageAccessor(Settings.pathWallpaperHD, Settings.PseudoRandom ? exclusionfileHD1080 : null);
				Image i1 = r.getRandomImage(out excluded, out found, out choosen);

				if (i1 == null)
				{
					LogProxy(_task, "No Images found.");
					return null;
				}
				LogProxy(_task, "[HD1080]  Images Found:    " + found);
				LogProxy(_task, "[HD1080]  Images Excluded: " + excluded);
				LogProxy(_task, "[HD1080]  Image Choosen:   " + Path.GetFileName(choosen));

				LogProxy(_task, "");

				return i1;
			}

			if (Eq(screen, 1280, 1024))
			{
				int found, excluded;
				string choosen;

				RandomImageAccessor r2 = new RandomImageAccessor(Settings.pathWallpaperLD);
				Image i2 = r2.getRandomImage(out excluded, out found, out choosen);

				if (i2 == null)
				{
					LogProxy(_task, "No Images found.");
					return null;
				}

				LogProxy(_task, "[ SXGA ]  Images Found:    " + found);
				LogProxy(_task, "[ SXGA ]  Image Choosen:   " + Path.GetFileName(choosen));

				LogProxy(_task, "");

				if (!Settings.EnableSXGATux) return i2;

				RandomImageAccessor r3 = new RandomImageAccessor(Settings.pathWallpaperTUX);
				Image i3 = r3.getRandomImage(out excluded, out found, out choosen);

				if (i3 == null)
				{
					LogProxy(_task, "No Images found.");
					return null;
				}

				LogProxy(_task, "[ TUX  ]  Images Found:    " + found);
				LogProxy(_task, "[ TUX  ]  Image Choosen:   " + Path.GetFileName(choosen));

				return MakeTuxImage(i2, i3);
			}

			return null;
		}

		private Image MakeTuxImage(Image bg, Image tux)
		{
			int tuxWidth  = 256;
			int tuxHeight = (int)(((tux.Height * 1d)/tux.Width)*tuxWidth);

			using (var graphics = Graphics.FromImage(bg))
			{
				var target = new Rectangle(bg.Width - tuxWidth - TUX_OFFSET, bg.Height - tuxHeight - TUX_OFFSET, tuxWidth, tuxHeight);
				var source = new Rectangle(0, 0, tux.Width, tux.Height);
				graphics.DrawImage(tux, target, source, GraphicsUnit.Pixel);
			}

			return bg;
		}

		private Bitmap TileImageY(Bitmap img, Rectangle bounds, Point offset)
		{
			Bitmap result = new Bitmap(bounds.Width, bounds.Height);

			if (offset.Y <= 0) return img;

			using (var graphics = Graphics.FromImage(result))
			{
				graphics.DrawImage(
					img,
					new Rectangle(0, bounds.Height - offset.Y, bounds.Width, offset.Y),
					new Rectangle(0, 0,                        bounds.Width, offset.Y),
					GraphicsUnit.Pixel);

				graphics.DrawImage(
					img,
					new Rectangle(0, 0,        bounds.Width, bounds.Height - offset.Y),
					new Rectangle(0, offset.Y, bounds.Width, bounds.Height - offset.Y),
					GraphicsUnit.Pixel);
			}

			return result;
		}

		private Bitmap TileImageX(Bitmap img, Rectangle bounds, Point offset)
		{
			Bitmap result = new Bitmap(bounds.Width, bounds.Height);

			if (offset.X <= 0) return img;

			using (var graphics = Graphics.FromImage(result))
			{
				graphics.DrawImage(
					img,
					new Rectangle(bounds.Width - offset.X, 0, offset.X, bounds.Height),
					new Rectangle(0,                       0, offset.X, bounds.Height),
					GraphicsUnit.Pixel);

				graphics.DrawImage(
					img,
					new Rectangle(0,        0, bounds.Width - offset.X, bounds.Height),
					new Rectangle(offset.X, 0, bounds.Width - offset.X, bounds.Height),
					GraphicsUnit.Pixel);
			}

			return result;
		}
	}
}
