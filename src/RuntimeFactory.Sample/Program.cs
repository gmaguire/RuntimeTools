using System;
using System.Threading;

namespace DebuggerFactories
{
    class Program
    {
        static void Main()
        {
            try
            {
                while (true)
                {
                    var calculator = CalculatorFactory.Create();
                    Console.WriteLine("2 + 2 = " + calculator.Sum(2, 2));
                    Thread.Sleep(1000);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.Read();
            }
      
        }
    }
}
