using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
namespace ATC.modules.DIPS
{
	public class DesktopIconPositionSave : ATCModule
	{
		public override void start()
		{
			RemoteListView.LVItem[] dicons = RemoteListView.GetListView("Progman", "Program Manager", "SysListView32", "FolderView");
			foreach (var di in dicons)
			{
				Console.WriteLine(di.Name + " @ " + di.Location.ToString());
			}
		}
    }
}
