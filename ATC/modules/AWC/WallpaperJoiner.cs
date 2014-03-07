﻿using System;
using System.Drawing;
using System.Windows.Forms;

namespace ATC.modules.AWC
{
	public class WallpaperJoiner
	{
		private const int TUX_OFFSET = 4;

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

			Rectangle bounds = new Rectangle(0, 0, 1920, 1080 + 1024);

			Bitmap result = new Bitmap(bounds.Width, bounds.Height);

			int tux_x = bounds.Width - tux.Width - TUX_OFFSET;
			int tux_y = bounds.Height - tux.Height - TUX_OFFSET;

			using (var graphics = Graphics.FromImage(result))
			{
				graphics.FillRectangle(Brushes.Black, bounds);

				graphics.DrawImage(primary, 0, 0, 1920, 1080);
				graphics.DrawImage(secondary, 1920 - 1280, 1080, 1280, 1024);
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
