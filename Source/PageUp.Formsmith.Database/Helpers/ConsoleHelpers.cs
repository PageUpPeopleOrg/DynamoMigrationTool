using System;

namespace PageUp.Formsmith.Database.Helpers
{
    public class ConsoleHelpers
    {
        public static void Information(string text)
        {
            WriteToConsole(text, ConsoleColor.White);
        }

        public static void Error(string text)
        {
            WriteToConsole(text, ConsoleColor.Red);
        }

        public static void Success(string text)
        {
            WriteToConsole(text, ConsoleColor.Green);
        }

        public static void WriteToConsole(string text, ConsoleColor color)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = originalColor;
        }
    }
}