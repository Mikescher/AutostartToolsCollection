using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIPSViewer
{
	public class DesktopIcon
	{
		public string name;
		public int x;
		public int y;

		public DesktopIcon(string nn, int xx, int yy)
		{
			name = nn;
			x = xx;
			y = yy;
		}

		public override bool Equals(System.Object obj)
		{
			if (obj == null)
			{
				return false;
			}
			DesktopIcon p = obj as DesktopIcon;
			if ((System.Object)p == null)
			{
				return false;
			}
			return (x == p.x) && (y == p.y);
		}

		public bool Equals(DesktopIcon p)
		{
			if ((object)p == null)
			{
				return false;
			}
			return (x == p.x) && (y == p.y) && (name == p.name);
		}

		public override int GetHashCode()
		{
			return x ^ y + name.GetHashCode();
		}
	}
}
