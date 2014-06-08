using System;

namespace ATC
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				Console.SetWindowSize(80, 66);
			}
			catch (ArgumentOutOfRangeException e)
			{
				// it's ok
			}

			ATCProgram prog = new ATCProgram();

			try
			{
				prog.start();
			}
			catch (Exception e)
			{
				Console.WriteLine("FATAL ERROR");
				Console.WriteLine("##################################");
				Console.WriteLine(e.ToString());
				Console.ReadLine(); // PAUSE
			}
		}
	}
}
