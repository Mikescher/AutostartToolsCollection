using System;

namespace ATC.Console
{
    static class Program
    {
        static void Main(string[] args)
        {
            try
            {
                System.Console.SetWindowSize(80, 66);
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
                System.Console.WriteLine("FATAL ERROR");
                System.Console.WriteLine("##################################");
                System.Console.WriteLine(e.ToString());
                System.Console.ReadLine(); // PAUSE
            }
        }
    }
}
