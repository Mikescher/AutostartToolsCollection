using ATC.config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATC.modules.AWC
{
	public class AutoWallChange : ATCModule
	{
		private AWCSettings settings { get { return (AWCSettings)settings_base; } }

		public AutoWallChange(ATCLogger l, AWCSettings s, string wd)
			: base(l, s, wd, "AWC")
		{
			
		}

		public override void start()
		{
			Console.Out.WriteLine("NotImplementedException()");
		}
	}
}
