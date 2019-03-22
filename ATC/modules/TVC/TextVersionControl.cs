using ATC.config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ATC.modules.TVC.Formatter;

namespace ATC.modules.TVC
{
	public class TextVersionControl : ATCModule
	{
		private TVCSettings Settings => (TVCSettings)SettingsBase;

		public TextVersionControl(ATCLogger l, TVCSettings s, string wd)
			: base(l, s, wd, "TVC")
		{
			// NOP
		}

		public override void Start()
		{
			LogHeader("TextVersionControl");

			if (!Settings.TVC_enabled)
			{
				Log("TVC not enabled.");
				return;
			}

			if (string.IsNullOrWhiteSpace(Settings.output))
			{
				Log("Outputpath not set.");
				return;
			}

			if (Settings.paths.Count == 0)
			{
				Log("No files in control");
				return;
			}

			foreach (var file in Settings.paths)
			{
				if (Settings.cleanHistory)
					CleanUpHistory(file);

				ProcessFile(file);

				Log();
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

		private void ProcessFile(TVCEntry file)
		{
			if (!File.Exists(file.path))
			{
				Log($@"File {file.path} does not exist");
				return;
			}

			var original = File.ReadAllText(file.path);
			var current = Transform(original, file.postprocessors);
			if (current==null) return;

			var outputpath = file.GetOutputPath(Settings);
			Directory.CreateDirectory(outputpath);

			var filename = string.Format("{0:yyyy}_{0:MM}_{0:dd}_{0:HH}_{0:mm}.txt", StartTime);

			var filepath = Path.Combine(outputpath, filename);

			if (File.Exists(filepath))
			{
				Log($@"File {filepath} does already exist in ouput directory");
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
				Log($"File {file.GetFoldername()} differs from last Version - copying current version");
				Log($"MD5 Current File:  {currHash}");
				Log($"MD5 Previous File: {lastHash}");

				try
				{
					if (file.warnOnDiff && versions.Count > 0)
					{
						ShowDiff(file, last, current, versions[0], file.path, file.prediffprocessors);
					}
				}
				catch (Exception ex)
				{
					Log($@"ERROR diffing File '{file.GetFoldername()}' to '{filepath}' : {ex.Message}");
					ShowExtMessage($@"ERROR diffing File '{file.GetFoldername()}' to '{filepath}'", ex.ToString());
				}

				try
				{
					if (original != current)
					{
						File.WriteAllText(filepath, current, Encoding.UTF8);
						Log($@"File '{file.GetFoldername()}' succesfully written to '{filepath}' (UTF-8)");
					}
					else
					{
						File.Copy(file.path, filepath, false);
						Log($@"File '{file.GetFoldername()}' succesfully copied to '{filepath}'");
					}
				}
				catch (Exception ex)
				{
					Log($@"ERROR copying File '{file.GetFoldername()}' to '{filepath}' : {ex.Message}");
					ShowExtMessage($@"ERROR copying File '{file.GetFoldername()}' to '{filepath}'", ex.ToString());
				}
			}
			else
			{
				Log($"File {file.GetFoldername()} remains unchanged (MD5: {currHash})");
			}
		}

		private string Transform(string input, IEnumerable<TVCTransformatorEntry> processors)
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
					Log($@"ERROR formatting content:\r\n\r\n{ex.Message}");
					ShowExtMessage("ERROR formatting content", ex.ToString());
					return null;
				}
			}
			return data;
		}

		private void ShowDiff(TVCEntry file, string txtold, string txtnew, string pathOld, string pathNew, List<TVCTransformatorEntry> transform)
		{
			var linesOld = Regex.Split(txtold, @"\r?\n");
			var linesNew = Regex.Split(txtnew, @"\r?\n");

			bool LineCompare(string x, string y)
			{
				return Transform(x, transform) == Transform(y, transform);
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

			foreach (var line in linesMissing) Log($"[WarnOnDiff] Line was removed from {file.GetFoldername()}: '{line}'");
			ShowExternalMessage($"TVC :: DiffCheck ({file.GetFoldername()})", b.ToString());
		}

		private void CleanUpHistory(TVCEntry file)
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
				Log($"Cleaned up duplicate Entry in History for {Path.GetFileNameWithoutExtension(file.GetFoldername())}: {Path.GetFileNameWithoutExtension(versions[deletions[i]])}");

				File.Delete(versions[deletions[i]]);
			}
		}
	}
}
