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
		private string workingDirectory;

		private ATCLogger logger;
		private ConfigWrapper config;

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

			awc.start();
			Thread.Sleep(500);

			dips.start();
			Thread.Sleep(500);

			tvc.start();
			Thread.Sleep(500);

			config.save();
			logger.saveAll();
		}
	}
}
