using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopAPI
{
	public struct Point
	{
		public int x;
		public int y;

		public override string ToString()
		{
			return "(" + x + "|" + y + ")";
		}
	};
}
