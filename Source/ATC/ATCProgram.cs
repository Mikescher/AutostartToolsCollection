using ATC.config;
using ATC.modules.AWC;
using ATC.modules.DIPS;
using ATC.modules.TVC;
using System;
using System.IO;
using System.Threading;
using ATC.modules.CSE;
using MSHC.Util.Helper;

namespace ATC
{
	public class ATCProgram
	{
		private readonly string workingDirectory;

		private readonly ATCLogger logger;
		private readonly ConfigWrapper config;

		public ATCProgram()
		{
			workingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"ATC\");

			logger = new ATCLogger(workingDirectory);
			config = new ConfigWrapper(workingDirectory);
		}

		public void Start()
		{
			try
			{
				var args = new CommandLineArguments(Environment.GetCommandLineArgs(), false);

				var doAWC = !args.Contains("runonly") || args.GetStringDefault("runonly", "").ToLower() == "awc";
				var doDPS = !args.Contains("runonly") || args.GetStringDefault("runonly", "").ToLower() == "dips";
				var doTVC = !args.Contains("runonly") || args.GetStringDefault("runonly", "").ToLower() == "tvc";
				var doCSE = !args.Contains("runonly") || args.GetStringDefault("runonly", "").ToLower() == "cse";

				config.load(logger);

				var awc = new AutoWallChange(logger, config.settings.awc, workingDirectory);
				var dips = new DesktopIconPositionSaver(logger, config.settings.dips, workingDirectory);
				var tvc = new TextVersionControl(logger, config.settings.tvc, workingDirectory);
				var cse = new CronScriptExecutor(logger, config.settings.cse, workingDirectory);

				if (doAWC) awc.Start();
				Thread.Sleep(500);

				if (doDPS) dips.Start();
				Thread.Sleep(500);

				if (doTVC) tvc.Start();
				Thread.Sleep(500);

				if (doCSE) cse.Start();
				Thread.Sleep(500);

				config.save();

#if DEBUG
				Console.WriteLine();
				Console.WriteLine("Prease any key to quit...");
				Console.ReadLine();
#endif

			}
			catch (Exception e)
			{
				logger.Log("ATC", "Uncaught Exception: " + e);
				ATCModule.ShowExtMessage("Uncaught Exception in ATC", e.ToString());
			}
			finally
			{
				logger.SaveAll();
			}
		}
	}
}
