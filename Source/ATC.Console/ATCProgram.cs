using ATC.Lib.config;
using ATC.Lib.modules.AWC;
using ATC.Lib.modules.DIPS;
using ATC.Lib.modules.TVC;
using System;
using System.IO;
using System.Threading;
using ATC.Lib.modules.CSE;
using MSHC.Util.Helper;
using ATC.Lib;

namespace ATC.Console
{
    public class ATCProgram
    {
        private readonly string workingDirectory;

        private readonly ATCLogger logger;
        private readonly ConfigWrapper config;

        public ATCProgram()
        {
            var args = new CommandLineArguments(Environment.GetCommandLineArgs(), false);

            workingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"ATC\");
            if (args.Contains("dir")) workingDirectory = args.GetStringDefault("dir", workingDirectory);

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

                var awc  = new AutoWallChange(logger, config.settings.awc, workingDirectory);
                var dips = new DesktopIconPositionSaver(logger, config.settings.dips, workingDirectory);
                var tvc  = new TextVersionControl(logger, config.settings.tvc, workingDirectory);
                var cse  = new CronScriptExecutor(logger, config.settings.cse, workingDirectory);

                if (doAWC) awc.Init(new ATCTaskProxy("AutoWallChange", "AWC"));
                if (doDPS) dips.Init(new ATCTaskProxy("DesktopIconPositionSaver", "DIPS"));
                if (doTVC) tvc.Init(new ATCTaskProxy("TextVersionControl", "TVC"));
                if (doCSE) cse.Init(new ATCTaskProxy("CronScriptExecutor", "CSE"));

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
                System.Console.WriteLine();
                System.Console.WriteLine("Prease any key to quit...");
                System.Console.ReadLine();
#endif

            }
            catch (Exception e)
            {
                logger.Log("ATC", null, "Uncaught Exception: " + e);
                ATCModule.ShowExtMessage("Uncaught Exception in ATC", e.ToString());
            }
            finally
            {
                logger.SaveAll();
            }
        }
    }
}
