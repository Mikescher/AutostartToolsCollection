using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ATC.Lib
{
	public class ATCLogger
	{
		private readonly object lck = new object();

		private readonly List<Tuple<string, string>> loglist = new List<Tuple<string, string>>();
		private readonly List<string> fulllog = new List<string>();
		private readonly Dictionary<string, ATCTaskProxy> listener = new Dictionary<string, ATCTaskProxy>();
		private readonly string rootWorkingDir;
		private readonly string logDir;
		private readonly DateTime startTime;

		public static ATCLogger Inst;

		public ATCLogger(string workingDir)
		{
			rootWorkingDir = workingDir;
			logDir = Path.Combine(workingDir, "logs");
			startTime = DateTime.Now;

			Inst = this;
		}

		public static void AddListener(string cat, ATCTaskProxy proxy)
		{
			Inst?.listener.Add(cat, proxy);
		}

		public void Log(string rootcat, string subcat, string text, bool subcatonly = false)
		{
			var fullcat = rootcat;
			if (subcat != null) fullcat += "::" + subcat;

			lock (lck)
			{
				if (!subcatonly) loglist.Add(Tuple.Create(rootcat, text));
				if (!subcatonly) fulllog.Add(text);
				Console.WriteLine(text);

				if (!subcatonly && listener.TryGetValue(string.Empty, out var l0)) l0.AddLog(text);
				if (!subcatonly && listener.TryGetValue(rootcat, out var l1)) l1.AddLog(text);
				if (subcat != null && listener.TryGetValue(fullcat, out var l2)) l2.AddLog(text);
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
