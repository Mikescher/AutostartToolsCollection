using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ATC.modules.AWC
{
	public class RandomImageAccessor
	{
		private static Random random = new Random();

		private string searchpath;
		private string exclusionpath;

		public RandomImageAccessor(string sp, string ep = null)
		{
			searchpath = sp;
			exclusionpath = ep;
		}

		public Bitmap getRandomImage(out int excludedImages, out int imagesFound, out string choosen)
		{
			List<string> files = EnumerateFiles(searchpath, @"\.bmp|\.jpg|\.jpeg|\.png").ToList();

			imagesFound = files.Count;

			string[] exclusions = new string[0];

			if (exclusionpath != null && File.Exists(exclusionpath))
				exclusions = File.ReadAllLines(exclusionpath);

			excludedImages = exclusions.Length;

			List<string> files_filtered = files.Where(p => !exclusions.Contains(Path.GetFileName(p))).ToList();

			if (files_filtered.Count > 0)
			{
				string used = files_filtered[random.Next(files_filtered.Count)];

				Image img = Image.FromFile(used);

				if (exclusionpath != null)
					File.AppendAllLines(exclusionpath, new List<string> { Path.GetFileName(used) });

				choosen = used;
				return SafeConvert(img);
			}
			else
			{
				if (files.Count > 0)
				{
					string used = files[random.Next(files.Count)];

					Image img = Image.FromFile(used);

					if (exclusionpath != null)
						File.WriteAllText(exclusionpath, Path.GetFileName(used)); // Resets list

					choosen = used;
					return SafeConvert(img);
				}
				else
				{
					choosen = null;
					return null;
				}
			}
		}

		public static IEnumerable<string> EnumerateFiles(string path, string searchPatternExpression = "", SearchOption searchOption = SearchOption.TopDirectoryOnly)
		{
			Regex reSearchPattern = new Regex(searchPatternExpression);
			return Directory.EnumerateFiles(path, "*", searchOption).Where(file => reSearchPattern.IsMatch(Path.GetExtension(file)));
		}

		private Bitmap SafeConvert(Image img)
		{
			Bitmap bmp = new Bitmap(img.Width, img.Height);

			using(Graphics g = Graphics.FromImage(bmp)) { g.DrawImage(img, 0, 0, img.Width, img.Height); }

			return bmp;
		}
	}
}
