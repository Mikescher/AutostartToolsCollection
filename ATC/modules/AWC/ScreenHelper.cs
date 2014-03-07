using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ATC.modules.AWC
{
	public enum MonitorConstellation
	{
		Dual_HD1080_SXGA,
		Dual_HD1080_HD1080,
		Single_HD1080,
		Single_SXGA,
		Other
	}

	public class ScreenHelper
	{
		public static Screen getPrimary()
		{
			return Screen.PrimaryScreen;
		}

		public static Screen getSecondary()
		{
			return Screen.AllScreens.FirstOrDefault(p => !p.Primary);
		}

		public static MonitorConstellation getMonitorConstellation()
		{
			if (Screen.AllScreens.Length == 1)
			{
				Screen prim = getPrimary();

				if (prim.Bounds.Width == 1920 && prim.Bounds.Height == 1080)
				{
					return MonitorConstellation.Single_HD1080;
				}
				else if (prim.Bounds.Width == 1280 && prim.Bounds.Height == 1024)
				{
					return MonitorConstellation.Single_SXGA;
				}
			}
			else if (Screen.AllScreens.Length == 2)
			{
				Screen prim = getPrimary();
				Screen sec = getSecondary();

				if (prim.Bounds.Width == 1920 && prim.Bounds.Height == 1080)
				{
					if (sec.Bounds.Width == 1920 && sec.Bounds.Height == 1080)
					{
						return MonitorConstellation.Dual_HD1080_HD1080;
					}
					else if (sec.Bounds.Width == 1280 && sec.Bounds.Height == 1024)
					{
						return MonitorConstellation.Dual_HD1080_SXGA;
					}
				}
			}

			return MonitorConstellation.Other;
		}

		public static string mcToString(MonitorConstellation mc)
		{
			switch (mc)
			{
				case MonitorConstellation.Dual_HD1080_SXGA:
					return "[1920x1080] -- [1280x1024]";
				case MonitorConstellation.Dual_HD1080_HD1080:
					return "[1920x1080] -- [1920x1080]";
				case MonitorConstellation.Single_HD1080:
					return "[1920x1080]";
				case MonitorConstellation.Single_SXGA:
					return "[1280x1024]";
				case MonitorConstellation.Other:
					return "[OTHER] -- " + getConstellationString();
				default:
					throw new ArgumentException();
			}
		}

		public static string getConstellationString()
		{
			StringBuilder b = new StringBuilder();

			for (int i = 0; i < Screen.AllScreens.Length; i++)
			{
				if (i > 0)
					b.Append(" -- ");

				b.Append("[");
				b.Append(Screen.AllScreens[i].Bounds.Width);
				b.Append("x");
				b.Append(Screen.AllScreens[i].Bounds.Height);
				b.Append("]");

			}

			return b.ToString();
		}

		public static Rectangle getDualScreenBounds()
		{
			Screen p = getPrimary();
			Screen s = getSecondary();

			int minX = Math.Min(p.Bounds.X, s.Bounds.X);
			int minY = Math.Min(p.Bounds.Y, s.Bounds.Y);

			int maxX = Math.Max(p.Bounds.Right, s.Bounds.Right);
			int maxY = Math.Max(p.Bounds.Bottom, s.Bounds.Bottom);

			return new Rectangle(minX, minY, maxX - minX, maxY - minY);
		}

		public static Point getDualScreenOffset()
		{
			Screen p = getPrimary();
			Screen s = getSecondary();

			int minX = Math.Min(p.Bounds.X, s.Bounds.X);
			int minY = Math.Min(p.Bounds.Y, s.Bounds.Y);

			return new Point(-minX, -minY);
		}
	}
}
