using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoadTestApp
{
    public static class ConsoleUtils
    {
        public static void WriteStepInfo(int caseNumber, string description)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            string message = $"\nStep #{caseNumber}. {description}";

            Console.WriteLine(message);

            Console.WriteLine(new string('-', message.Length));

            Console.ResetColor();


        }
    }
}
