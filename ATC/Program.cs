using System;

namespace ATC
{
	static class Program
	{
		static void Main(string[] args)
		{
			try
			{
				Console.SetWindowSize(80, 66);
			}
			catch (ArgumentOutOfRangeException)
			{
				// it's ok
			}

			ATCProgram prog = new ATCProgram();

			try
			{
				prog.Start();
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
