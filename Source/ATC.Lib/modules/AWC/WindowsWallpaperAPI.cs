using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace ATC.Lib.modules.AWC
{
	public class WindowsWallpaperAPI
	{
		const int SPI_SETDESKWALLPAPER = 20;
		const int SPIF_UPDATEINIFILE = 0x01;
		const int SPIF_SENDWININICHANGE = 0x02;

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

		public enum W_WP_Style : int
		{
			Tiled,
			Centered,
			Stretched
		}

		public static void Set(string path, W_WP_Style style)
		{
			RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
			if (style == W_WP_Style.Stretched)
			{
				key.SetValue(@"WallpaperStyle", 2.ToString());
				key.SetValue(@"TileWallpaper", 0.ToString());
			}

			if (style == W_WP_Style.Centered)
			{
				key.SetValue(@"WallpaperStyle", 1.ToString());
				key.SetValue(@"TileWallpaper", 0.ToString());
			}

			if (style == W_WP_Style.Tiled)
			{
				key.SetValue(@"WallpaperStyle", 1.ToString());
				key.SetValue(@"TileWallpaper", 1.ToString());
			}

			SystemParametersInfo(SPI_SETDESKWALLPAPER,
				0,
				path,
				SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
		}
	}
}
