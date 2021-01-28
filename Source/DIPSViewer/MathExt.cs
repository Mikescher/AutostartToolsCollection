using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIPSViewer
{
	public class MathExt
	{
		public static void Swap<T>(ref T lhs, ref T rhs)
		{
			T temp;
			temp = lhs;
			lhs = rhs;
			rhs = temp;
		}

		public static int Max(int v1, params int[] va)
		{
			foreach (int v in va)
			{
				v1 = Math.Max(v1, v);
			}
			return v1;
		}

		public static int Min(int v1, params int[] va)
		{
			foreach (int v in va)
			{
				v1 = Math.Min(v1, v);
			}
			return v1;
		}
	}
}
