using ATC.config;
using ATC.modules.AWC;
using ATC.modules.DIPS;
using ATC.modules.TVC;
using MSHC.Helper;
using System;
using System.IO;
using System.Threading;

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
				CommandLineArguments args = new CommandLineArguments(Environment.GetCommandLineArgs(), false);

				bool doAWC = !args.Contains("runonly") || args.GetStringDefault("runonly", "").ToLower() == "awc";
				bool doDPS = !args.Contains("runonly") || args.GetStringDefault("runonly", "").ToLower() == "dips";
				bool doTVC = !args.Contains("runonly") || args.GetStringDefault("runonly", "").ToLower() == "tvc";
				bool doCSE = !args.Contains("runonly") || args.GetStringDefault("runonly", "").ToLower() == "cse";

				config.load(logger);

				AutoWallChange awc = new AutoWallChange(logger, config.settings.awc, workingDirectory);
				DesktopIconPositionSaver dips = new DesktopIconPositionSaver(logger, config.settings.dips, workingDirectory);
				TextVersionControl tvc = new TextVersionControl(logger, config.settings.tvc, workingDirectory);
				CronScriptExecutor cse = new CronScriptExecutor(logger, config.settings.cse, workingDirectory);

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
				Console.ReadLine();
#endif

			}
			catch (Exception e)
			{
				logger.log("ATC", "Uncaught Exception: " + e);
				ATCModule.ShowExtMessage("Uncaught Exception in ATC", e.ToString());
			}
			finally
			{
				logger.saveAll();
			}
		}
	}
}
