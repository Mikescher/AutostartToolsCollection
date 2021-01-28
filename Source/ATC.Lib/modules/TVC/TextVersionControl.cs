using ATC.Lib.config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ATC.Lib.modules.TVC.Formatter;
using System.Threading;

namespace ATC.Lib.modules.TVC
{
	public class TextVersionControl : ATCModule
	{
		private TVCSettings Settings => (TVCSettings)SettingsBase;

		private ATCTaskProxy rootTask;
		private List<(TVCEntry entry, ATCTaskProxy proxy)> _tasks = new List<(TVCEntry, ATCTaskProxy)>();

		public TextVersionControl(ATCLogger l, TVCSettings s, string wd)
			: base(l, s, wd, "TVC")
		{
			// NOP
		}

		public override List<ATCTaskProxy> Init(ATCTaskProxy root)
		{
			rootTask = root;

			if (!Settings.TVC_enabled)
			{
				LogRoot("TVC not enabled.");
				rootTask.FinishSuccess();
				return new List<ATCTaskProxy>();
			}

			if (string.IsNullOrWhiteSpace(Settings.output))
			{
				LogRoot("Outputpath not set.");
				rootTask.SetErrored();
				return new List<ATCTaskProxy>();
			}

			if (Settings.paths.Count == 0)
			{
				LogRoot("No files in control");
				rootTask.SetErrored();
				return new List<ATCTaskProxy>();
			}

			_tasks = Settings.paths.Select(p => (p, new ATCTaskProxy($"Backup {p.GetFoldername()}", Modulename, Guid.NewGuid()))).ToList();

			return _tasks.Select(p => p.Item2).ToList();
		}

		public override void Start()
		{
			LogHeader("TextVersionControl");

			foreach (var file in _tasks)
			{
                try
				{
					file.proxy.Start();

					if (Settings.cleanHistory) CleanUpHistory(file.entry, file.proxy);

					if (file.entry.isRecursiveFolder) ProcessDirectory(file.entry, file.proxy);
					else ProcessFile(file.entry, file.proxy);

					LogProxy(file.proxy, "");
					file.proxy.FinishSuccess();

					Thread.Sleep(250);
				}
                catch (Exception e)
                {
					LogProxy(file.proxy, "Exception: " + e.ToString());
					file.proxy.SetErrored();
				}
			}
		}

		private bool IsValidDateTimeFileName(string path)
		{
			var fn = Path.GetFileName(path);
			var fnwe = Path.GetFileNameWithoutExtension(path);

			if (Regex.IsMatch(fn, @"[0-9]{4}_[0-9]{2}_[0-9]{2}_[0-9]{2}_[0-9]{2}.txt"))
				return DateTime.TryParseExact(fnwe, "yyyy_MM_dd_HH_mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);

			return false;
		}

		private void ProcessDirectory(TVCEntry file, ATCTaskProxy proxy)
		{
			if (!Directory.Exists(file.path))
			{
				LogProxy(proxy, $"Directory {file.path} does not exist");
				proxy.SetErrored();
				return;
			}

			var dirfiles = Directory.EnumerateFiles(file.path);

			foreach (var subfile in dirfiles)
			{
				var subentry = file.CreateSubEntry(subfile);
				ProcessFile(subentry, proxy);
				LogProxy(proxy, $"");
			}
		}

		private void ProcessFile(TVCEntry file, ATCTaskProxy proxy)
		{
			if (!File.Exists(file.path))
			{
				LogProxy(proxy, $"File {file.path} does not exist");
				return;
			}

			var original = File.ReadAllText(file.path);
			var current = Transform(original, file.postprocessors, proxy);
			if (current==null) return;

			var outputpath = file.GetOutputPath(Settings);
			Directory.CreateDirectory(outputpath);

			var filename = string.Format("{0:yyyy}_{0:MM}_{0:dd}_{0:HH}_{0:mm}.txt", StartTime);

			var filepath = Path.Combine(outputpath, filename);

			if (File.Exists(filepath))
			{
				LogProxy(proxy, $@"File {filepath} does already exist in ouput directory");
				proxy.SetErrored();
				return;
			}

			var versions = Directory.EnumerateFiles(outputpath).
				Where(IsValidDateTimeFileName).
				OrderByDescending(p => DateTime.ParseExact(Path.GetFileNameWithoutExtension(p), "yyyy_MM_dd_HH_mm", CultureInfo.InvariantCulture)).
				ToList();

			var last = "";

			if (versions.Count > 0) last = File.ReadAllText(versions[0]);

			var lastHash = StringHashing.CalculateMD5Hash(last);
			var currHash = StringHashing.CalculateMD5Hash(current);

			if (lastHash != currHash)
			{
				LogProxy(proxy, $"File {file.GetFoldername()} differs from last Version - copying current version");
				LogProxy(proxy, $"MD5 Current File:  {currHash}");
				LogProxy(proxy, $"MD5 Previous File: {lastHash}");

				try
				{
					if (file.warnOnDiff && versions.Count > 0)
					{
						ShowDiff(file, last, current, versions[0], file.path, file.prediffprocessors, proxy);
					}
				}
				catch (Exception ex)
				{
					LogProxy(proxy, $@"ERROR diffing File '{file.GetFoldername()}' to '{filepath}' : {ex.Message}");
					proxy.SetErrored();
				}

				try
				{
					if (original != current)
					{
						File.WriteAllText(filepath, current, Encoding.UTF8);
						LogProxy(proxy, $@"File '{file.GetFoldername()}' succesfully written to '{filepath}' (UTF-8)");
						proxy.FinishSuccess();
						return;
					}
					else
					{
						File.Copy(file.path, filepath, false);
						LogProxy(proxy, $@"File '{file.GetFoldername()}' succesfully copied to '{filepath}'");
						proxy.FinishSuccess();
						return;
					}
				}
				catch (Exception ex)
				{
					LogProxy(proxy, $@"ERROR copying File '{file.GetFoldername()}' to '{filepath}' : {ex.Message}");
					LogProxy(proxy, ex.ToString());
					proxy.SetErrored();
					return;
				}
			}
			else
			{
				LogProxy(proxy, $"File {file.GetFoldername()} remains unchanged (MD5: {currHash})");
				proxy.FinishSuccess();
				return;
			}
		}

		private string Transform(string input, IEnumerable<TVCTransformatorEntry> processors, ATCTaskProxy proxy)
		{
			var data = input;
			foreach (var method in processors)
			{
				try
				{
					var processor = TVCTransformators.Processors[method.name];
					//Log(string.Format(@"Apply postprocessor {1} to '{0}'", file.GetFoldername(), processor.Name));

					data = processor.Process(data, method.settings);
				}
				catch (Exception ex)
				{
					LogProxy(proxy, $@"ERROR formatting content:\r\n\r\n{ex.Message}");
					proxy.SetErrored();
					return null;
				}
			}
			return data;
		}

		private void ShowDiff(TVCEntry file, string txtold, string txtnew, string pathOld, string pathNew, List<TVCTransformatorEntry> transform, ATCTaskProxy proxy)
		{
			var linesOld = Regex.Split(txtold, @"\r?\n");
			var linesNew = Regex.Split(txtnew, @"\r?\n");

			bool LineCompare(string x, string y)
			{
				return Transform(x, transform, proxy) == Transform(y, transform, proxy);
			}

			var linesMissing = linesOld.Except(linesNew, new LambdaEqualityComparer<string>(LineCompare)).ToList();

			if (linesMissing.Count == 0) return;

			var linesAdded = linesNew.Except(linesOld, new LambdaEqualityComparer<string>(LineCompare)).ToList();


			var b = new StringBuilder();
			b.AppendLine("There were differences between the following files:");
			b.AppendLine(" - " + pathOld);
			b.AppendLine(" - " + pathNew);
			b.AppendLine();
			b.AppendLine();
			b.AppendLine("######## REMOVED LINES ########");
			b.AppendLine();
			foreach (var line in linesMissing) b.AppendLine(line);
			b.AppendLine();
			b.AppendLine();
			b.AppendLine("######### ADDED LINES #########");
			b.AppendLine();
			foreach (var line in linesAdded) b.AppendLine(line);

			foreach (var line in linesMissing) LogProxy(proxy, $"[WarnOnDiff] Line was removed from {file.GetFoldername()}: '{line}'");

			LogProxy(proxy, $"TVC :: DiffCheck ({file.GetFoldername()})\n" + b.ToString());
			ShowExternalMessage($"TVC :: DiffCheck ({file.GetFoldername()})", b.ToString());
		}

		private void CleanUpHistory(TVCEntry file, ATCTaskProxy proxy)
		{
			var outputpath = file.GetOutputPath(Settings);
			Directory.CreateDirectory(outputpath);

			var versions = Directory.EnumerateFiles(outputpath).
				Where(IsValidDateTimeFileName).
				OrderByDescending(p => DateTime.ParseExact(Path.GetFileNameWithoutExtension(p), "yyyy_MM_dd_HH_mm", CultureInfo.InvariantCulture)).
				ToList();

			var contents = versions.Select(File.ReadAllText).ToList();

			var deletions = new List<int>();

			for (var i = 1; i < versions.Count; i++)
			{
				if (contents[i] == contents[i - 1])
				{
					deletions.Add(i - 1);
				}
			}

			for (var i = deletions.Count - 1; i >= 0; i--)
			{
				LogProxy(proxy, $"Cleaned up duplicate Entry in History for {Path.GetFileNameWithoutExtension(file.GetFoldername())}: {Path.GetFileNameWithoutExtension(versions[deletions[i]])}");

				File.Delete(versions[deletions[i]]);
			}
		}
	}
}