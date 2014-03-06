using System;
using System.Drawing;
using System.Windows.Forms;

namespace ATC.modules.AWC
{
	public class WallpaperJoiner
	{
		private MonitorConstellation constellation;

		public WallpaperJoiner(MonitorConstellation mc)
		{
			constellation = mc;
		}

		public Image Join(Image primary, Image secondary = null, Image tux = null)
		{
			switch (constellation)
			{
				case MonitorConstellation.Dual_HD1080_SXGA:
					return Join_HD1080_SXGA(primary, secondary, tux);
				case MonitorConstellation.Dual_HD1080_HD1080:
					return Join_HD1080_HD1080(primary, secondary);
				case MonitorConstellation.Single_HD1080:
					return primary;
				case MonitorConstellation.Single_SXGA:
					return primary;
				default:
					throw new ArgumentException();
			}
		}

		private Image Join_HD1080_SXGA(Image primary, Image secondary, Image tux)
		{
			Screen s_primary = ScreenHelper.getPrimary();
			Screen s_secondary = ScreenHelper.getSecondary();

			Rectangle bounds = ScreenHelper.getDualScreenBounds();
			Point offset = ScreenHelper.getDualScreenOffset();

			Bitmap result = new Bitmap(bounds.Width, bounds.Height);

			int tux_x = s_secondary.Bounds.Location.X + offset.X + s_secondary.Bounds.Width - tux.Width;
			int tux_y = s_secondary.Bounds.Location.Y + offset.Y + s_secondary.Bounds.Height - tux.Height;

			using (var graphics = Graphics.FromImage(result))
			{
				graphics.DrawImage(primary, s_primary.Bounds.Location.X + offset.X, s_primary.Bounds.Location.Y + offset.Y, 1920, 1080);
				graphics.DrawImage(secondary, s_secondary.Bounds.Location.X + offset.X, s_secondary.Bounds.Location.Y + offset.Y, 1280, 1024);
				graphics.DrawImage(tux, tux_x, tux_y, tux.Width, tux.Height);
			}

			return result;
		}

		private Image Join_HD1080_HD1080(Image primary, Image secondary)
		{
			Screen s_primary = ScreenHelper.getPrimary();
			Screen s_secondary = ScreenHelper.getSecondary();

			Rectangle bounds = ScreenHelper.getDualScreenBounds();
			Point offset = ScreenHelper.getDualScreenOffset();

			Bitmap result = new Bitmap(bounds.Width, bounds.Height);

			using (var graphics = Graphics.FromImage(result))
			{
				graphics.DrawImage(primary, s_primary.Bounds.Location.X + offset.X, s_primary.Bounds.Location.Y + offset.Y, 1920, 1080);
				graphics.DrawImage(secondary, s_secondary.Bounds.Location.X + offset.X, s_secondary.Bounds.Location.Y + offset.Y, 1920, 1080);
			}

			return result;
		}
	}
}
