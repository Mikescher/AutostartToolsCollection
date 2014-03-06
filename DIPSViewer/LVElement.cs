using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIPSViewer
{
	public class LVElement
	{
		public DateTime time;
		public string path;

		public LVElement(DateTime t, string p)
		{
			time = t;
			path = p;
		}

		public override string ToString()
		{
			return time.ToString("dd.MM.yyy hh:mm");
		}
	}
}
