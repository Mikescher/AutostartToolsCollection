﻿using ATC.Lib;
using ATC.Lib.config;
using ATC.Lib.modules.AWC;
using ATC.Lib.modules.CSE;
using ATC.Lib.modules.DIPS;
using ATC.Lib.modules.TVC;
using MSHC.Util.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace ATC.UI
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewmodel _vm;

        private readonly ATCTaskProxy _mainTask;

        private bool _isAutoClosing = false;
        private bool _abortAutoClosing = false;

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = _vm = new MainWindowViewmodel();

            _vm.Tasks.Add(_mainTask = new ATCTaskProxy("AutostartToolsCollection", "ATC"));

            DispatcherHelper.InvokeDelayed(Start, 1250);
        }

        private void Start()
        {
            ATCLogger logger = null;

            try
            {
                new Thread(() =>
                {
                    try
                    {
                        _mainTask.Start();

                        var args = new CommandLineArguments(Environment.GetCommandLineArgs(), false);

                        var workingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"ATC\");
                        if (args.Contains("dir")) workingDirectory = args.GetStringDefault("dir", workingDirectory);

                        logger = new ATCLogger(workingDirectory);
                        _mainTask.RegisterRoot();

                        var config = new ConfigWrapper(workingDirectory);

                        var doAWC = !args.Contains("runonly") || args.GetStringDefault("runonly", "").ToLower() == "awc";
                        var doDPS = !args.Contains("runonly") || args.GetStringDefault("runonly", "").ToLower() == "dips";
                        var doTVC = !args.Contains("runonly") || args.GetStringDefault("runonly", "").ToLower() == "tvc";
                        var doCSE = !args.Contains("runonly") || args.GetStringDefault("runonly", "").ToLower() == "cse";

                        config.load(logger);


                        var awc = new AutoWallChange(logger, config.settings.awc, workingDirectory);
                        var dips = new DesktopIconPositionSaver(logger, config.settings.dips, workingDirectory);
                        var tvc = new TextVersionControl(logger, config.settings.tvc, workingDirectory);
                        var cse = new CronScriptExecutor(logger, config.settings.cse, workingDirectory);

                        // =====================================================================================================

                        ATCTaskProxy taskAWC = null;
                        ATCTaskProxy taskDPS = null;
                        ATCTaskProxy taskTVC = null;
                        ATCTaskProxy taskCSE = null;

                        var newtasks = new List<ATCTaskProxy>();

                        if (doAWC)
                        {
                            newtasks.Add(taskAWC = new ATCTaskProxy("AutoWallChange", "AWC"));
                            newtasks.AddRange(awc.Init(taskAWC));
                        }
                        if (doDPS)
                        {
                            newtasks.Add(taskDPS = new ATCTaskProxy("DesktopIconPositionSaver", "DIPS"));
                            newtasks.AddRange(dips.Init(taskDPS));
                        }
                        if (doTVC)
                        {
                            newtasks.Add(taskTVC = new ATCTaskProxy("TextVersionControl", "TVC"));
                            newtasks.AddRange(tvc.Init(taskTVC));
                        }
                        if (doCSE)
                        {
                            newtasks.Add(taskCSE = new ATCTaskProxy("CronScriptExecutor", "CSE"));
                            newtasks.AddRange(cse.Init(taskCSE));
                        }

                        foreach (var t in newtasks)
                        {
                            DispatcherHelper.SmartInvoke(() => { _vm.Tasks.Add(t); });
                            Thread.Sleep(50);
                        }

                        // =====================================================================================================

                        if (doAWC)
                        {
                            taskAWC.Start();
                            awc.Start();
                            taskAWC.FinishSuccess();
                            Thread.Sleep(500);
                        }

                        if (doDPS)
                        {
                            taskDPS.Start();
                            dips.Start();
                            taskDPS.FinishSuccess();
                            Thread.Sleep(500);
                        }

                        if (doTVC)
                        {
                            taskTVC.Start();
                            tvc.Start();
                            taskTVC.FinishSuccess();
                            Thread.Sleep(500);
                        }

                        if (doCSE)
                        {
                            taskCSE.Start();
                            cse.Start();
                            taskCSE.FinishSuccess();
                            Thread.Sleep(500);
                        }

                        // =====================================================================================================

                        config.save();

                        _mainTask.FinishSuccess();

                        logger.SaveAll();

                        if (_vm.Tasks.All(p => p.State == ProxyState.Success))
                        {
                            _isAutoClosing = true;

                            var max = 10 * 1000;

                            new Thread(() =>
                            {
                                var start = Environment.TickCount;
                                for (; ; )
                                {
                                    if (_abortAutoClosing)
                                    {
                                        DispatcherHelper.SmartInvoke(() => { _vm.CloseProgress = 0; });
                                        return;
                                    }

                                    var delta = Environment.TickCount - start;

                                    if (delta > max)
                                    {
                                        DispatcherHelper.SmartInvoke(() => { App.Current.MainWindow.Close(); });
                                        return;
                                    }
                                    else
                                    {
                                        var p = ((delta * 100) / max);
                                        if (p != _vm.CloseProgress) DispatcherHelper.SmartInvoke(() => { _vm.CloseProgress = p; });
                                    }

                                    Thread.Yield();
                                }

                            }).Start();
                        }
                    }
                    catch (Exception e)
                    {
                        logger?.Log("ATC", null, "Uncaught Exception: " + e);
                        ATCModule.ShowExtMessage("Uncaught Exception in ATC", e.ToString());

                        _mainTask.SetErrored();
                    }
                }).Start();
            }
            catch (Exception e)
            {
                logger?.Log("ATC", null, "Uncaught Exception: " + e);
                ATCModule.ShowExtMessage("Uncaught Exception in ATC", e.ToString());

                _mainTask.SetErrored();
                logger.SaveAll();
            }
        }

        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_isAutoClosing) _abortAutoClosing = true;
        }

        private void Window_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (_isAutoClosing) _abortAutoClosing = true;
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_isAutoClosing) _abortAutoClosing = true;
        }

        private void Window_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_isAutoClosing) _abortAutoClosing = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_mainTask.State == ProxyState.Running)
            {
                if (MessageBox.Show("Some tasks are still running, really close?", "Close ATC?", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
