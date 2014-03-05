using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATC
{
	public class ATCLogger
	{
		private List<Tuple<string, string>> loglist = new List<Tuple<string, string>>();
		private string logDir;
		private DateTime startTime;

		public ATCLogger(string workingDir)
		{
			logDir = Path.Combine(workingDir, "logs");
			startTime = DateTime.Now;
		}

		public void log(string cat, string text)
		{
			loglist.Add(Tuple.Create(cat, text));
			Console.WriteLine(text);
		}

		private List<string> getCategories()
		{
			return loglist.Select(p => p.Item1).Distinct().ToList();
		}

		private string getDateFilename(string path, int idx = 0)
		{
			string prefix = String.Format(@"{0:yyyy}_{0:MM}_{0:dd}", startTime);
			string suffix = ((idx == 0) ? "" : string.Format("_{0,00}", idx)) + ".log";

			string filepath = Path.Combine(path, prefix+suffix);

			if (File.Exists(filepath))
				return getDateFilename(path, idx + 1);
			else
				return filepath;
		}

		private List<String> getLog(string cat)
		{
			return loglist.Where(p => p.Item1 == cat).Select(p => p.Item2).ToList();
		}

		public void saveAll()
		{
			List<string> cats = getCategories();

			foreach (string cat in cats)
			{
				List<String> log = getLog(cat);
				string slog = string.Join(Environment.NewLine, log);

				string path = Path.Combine(logDir, cat);
				Directory.CreateDirectory(path);
				path = getDateFilename(path);

				File.WriteAllText(path, slog);
			}
		}
	}
}
