using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopAPI
{
	public class LVItem
	{
		public string Name { get; private set; }
		public Point Location { get; private set; }
		public LVItem(string n, Point l) { Name = n; Location = l; }
	}
}
