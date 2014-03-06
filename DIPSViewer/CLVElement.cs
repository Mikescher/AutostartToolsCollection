using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIPSViewer
{
	public class CLVElement
	{
		public object icn;
		public string txt;

		public CLVElement(object i, string t)
		{
			icn = i;
			txt = t;
		}

		public override string ToString()
		{
			return txt;
		}
	}
}
