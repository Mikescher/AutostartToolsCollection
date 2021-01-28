using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ATC
{
	public class ATCLogger
	{
		private readonly object lck = new object();

		private readonly List<Tuple<string, string>> loglist = new List<Tuple<string, string>>();
		private readonly List<string> fulllog = new List<string>();
		private readonly string rootWorkingDir;
		private readonly string logDir;
		private readonly DateTime startTime;

		public ATCLogger(string workingDir)
		{
			rootWorkingDir = workingDir;
			logDir = Path.Combine(workingDir, "logs");
			startTime = DateTime.Now;
		}

		public void Log(string cat, string text)
		{
			lock (lck)
			{
				loglist.Add(Tuple.Create(cat, text));
				fulllog.Add(text);
				Console.WriteLine(text);
			};
		}

		public void LogNewFile(string modulename, string[] path, string text)
		{
			var fulldir = Path.Combine(new []{ rootWorkingDir, modulename }.Concat(path.Reverse().Skip(1).Reverse()).ToArray());
			var fullpath = Path.Combine(fulldir, path.Last());

			if (!Directory.Exists(fulldir)) Directory.CreateDirectory(fulldir);

			File.WriteAllText(fullpath, text);
		}

		private IEnumerable<string> GetCategories()
		{
			lock (lck)
			{
				return loglist.Select(p => p.Item1).Distinct().ToList();
			}
		}

		private string GetDateFilename(string path, int idx = 0)
		{
			var prefix = $@"{startTime:yyyy}_{startTime:MM}_{startTime:dd}";
			var suffix = (idx == 0) ? ".log" : $"_{idx,0}.log";

			var filepath = Path.Combine(path, prefix+suffix);

			return File.Exists(filepath) ? GetDateFilename(path, idx + 1) : filepath;
		}

		private IEnumerable<string> GetLog(string cat)
		{
			lock (lck)
			{
				return loglist.Where(p => p.Item1 == cat).Select(p => p.Item2).ToList();
			}
		}

		private IEnumerable<string> GetFullLog()
		{
			lock (lck)
			{
				return fulllog.ToList();
			}
		}

		public void SaveAll()
		{
			var cats = GetCategories();

			foreach (var cat in cats)
			{
				var log = GetLog(cat);

				var slog = string.Join(Environment.NewLine, log);

				var path = Path.Combine(logDir, cat);
				Directory.CreateDirectory(path);
				path = GetDateFilename(path);

				File.WriteAllText(path, slog);
			}

			var flog = GetFullLog();
			var sflog = string.Join(Environment.NewLine, flog);

			var fpath = Path.Combine(logDir, "_raw");
			Directory.CreateDirectory(fpath);
			fpath = GetDateFilename(fpath);

			File.WriteAllText(fpath, sflog);
		}
	}
}
