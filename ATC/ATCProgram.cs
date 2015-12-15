using ATC.config;
using ATC.modules.AWC;
using ATC.modules.DIPS;
using ATC.modules.TVC;
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

		public void start()
		{
			config.load(logger);

			AutoWallChange awc = new AutoWallChange(logger, config.settings.awc, workingDirectory);
			DesktopIconPositionSaver dips = new DesktopIconPositionSaver(logger, config.settings.dips, workingDirectory);
			TextVersionControl tvc = new TextVersionControl(logger, config.settings.tvc, workingDirectory);
			CronScriptExecutor cse = new CronScriptExecutor(logger, config.settings.cse, workingDirectory);

			awc.Start();
			Thread.Sleep(500);

			dips.Start();
			Thread.Sleep(500);

			tvc.Start();
			Thread.Sleep(500);

			cse.Start();
			Thread.Sleep(500);

			config.save();
			logger.saveAll();
		}
	}
}
