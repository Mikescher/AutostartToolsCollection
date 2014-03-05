using ATC.config;
using ATC.modules.AWC;
using ATC.modules.DIPS;
using ATC.modules.TVC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATC
{
	public class ATCProgram
	{
		private string workingDirectory;

		private ATCLogger logger;
		private ConfigWrapper config;

		public ATCProgram()
		{
			workingDirectory = Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"ATC\");

			logger = new ATCLogger(workingDirectory);
			config = new ConfigWrapper(workingDirectory);
		}

		public void start()
		{
			config.load();

			AutoWallChange awc = new AutoWallChange(logger, config.settings.awc, workingDirectory);
			DesktopIconPositionSaver dips = new DesktopIconPositionSaver(logger, config.settings.dips, workingDirectory);
			TextVersionControl tvc = new TextVersionControl(logger, config.settings.tvc, workingDirectory);

			awc.start();
			dips.start();
			tvc.start();

			config.save();
			logger.saveAll();
		}
	}
}
