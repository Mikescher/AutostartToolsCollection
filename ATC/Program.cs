using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATC
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.SetWindowSize(80, 66);

			ATCProgram prog = new ATCProgram();

			prog.start();

			Console.ReadLine(); // PAUSE
		}
	}
}
