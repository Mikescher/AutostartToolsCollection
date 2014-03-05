using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATC.config
{
	public class ATCSettings : SettingsModule
	{
		public AWCSettings awc = new AWCSettings();
		public DIPSSettings dips = new DIPSSettings();
		public TVCSettings tvc = new TVCSettings();
	}
}
