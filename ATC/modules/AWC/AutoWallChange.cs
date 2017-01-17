using ATC.config;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ATC.modules.AWC
{
	public class AutoWallChange : ATCModule
	{
		private AWCSettings Settings => (AWCSettings) SettingsBase;

		private const int TUX_OFFSET = 4;

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

			if (String.IsNullOrWhiteSpace(Settings.pathWallpaperFile))
			{
				Log("pathWallpaperFile not set");
				return;
			}

			if (Path.GetExtension(Settings.pathWallpaperFile) != ".bmp")
			{
				Log("pathWallpaper must direct to a *.bmp file");
				return;
			}

			exclusionfileHD1080 = Path.Combine(WorkingDirectory, "exclusions.config");

			Log("Monitor Constellation: " + GetConstellationString());

			if (Screen.AllScreens.Any(p => !(Eq(p, 1920, 1080) || Eq(p, 1280, 1024))))
			{
				Log("Unknown Monitor Constellation");
				return;
			}

			Log();

			var img = CreateMultiMonitorImage();

			if (img == null)
			{
				Log("Error while creating MultiMonitor Wallpaper");
				return;
			}

			img.Save(Settings.pathWallpaperFile, ImageFormat.Bmp);

			WindowsWallpaperAPI.Set(Settings.pathWallpaperFile, WindowsWallpaperAPI.W_WP_Style.Tiled);
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
					Log("No Images found.");
					return null;
				}
				Log("[HD1080]  Images Found:    " + found);
				Log("[HD1080]  Images Excluded: " + excluded);
				Log("[HD1080]  Image Choosen:   " + Path.GetFileName(choosen));

				Log();

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
					Log("No Images found.");
					return null;
				}

				Log("[ SXGA ]  Images Found:    " + found);
				Log("[ SXGA ]  Image Choosen:   " + Path.GetFileName(choosen));

				Log();

				if (!Settings.EnableSXGATux) return i2;

				RandomImageAccessor r3 = new RandomImageAccessor(Settings.pathWallpaperTUX);
				Image i3 = r3.getRandomImage(out excluded, out found, out choosen);

				if (i3 == null)
				{
					Log("No Images found.");
					return null;
				}

				Log("[ TUX  ]  Images Found:    " + found);
				Log("[ TUX  ]  Image Choosen:   " + Path.GetFileName(choosen));

				return MakeTuxImage(i2, i3);
			}

			return null;
		}

		private Image MakeTuxImage(Image bg, Image tux)
		{
			using (var graphics = Graphics.FromImage(bg))
			{
				graphics.DrawImage(tux, bg.Width - tux.Width - TUX_OFFSET, bg.Height - tux.Height - TUX_OFFSET, tux.Width, tux.Height);
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
