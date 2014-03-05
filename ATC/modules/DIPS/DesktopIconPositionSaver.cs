using ATC.config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
namespace ATC.modules.DIPS
{
	public class DesktopIconPositionSaver : ATCModule
	{
		private DIPSSettings settings { get { return (DIPSSettings)settings_base; } }

		public DesktopIconPositionSaver(ATCLogger l, DIPSSettings s, string wd)
			: base(l, s, wd, "DIPS")
		{

		}
		public override void start()
		{
			logHeader("DesktopIconPositionSaver");

			if (!settings.DIPS_enabled)
			{
				log("DIPS not enabled.");
				return;
			}

			RemoteListView.LVItem[] dicons = RemoteListView.GetListView("Progman", "Program Manager", "SysListView32", "FolderView");
			foreach (var di in dicons)
			{
				Console.WriteLine(di.Name + " @ " + di.Location.ToString());
			}
		}
    }
}
