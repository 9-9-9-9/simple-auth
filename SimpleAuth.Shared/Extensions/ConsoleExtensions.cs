using System;

namespace SimpleAuth.Shared.Extensions
{
    public static class ConsoleExtensions
    {
        public static void Write(this object obj)
        {
            Console.WriteLine(obj ?? "null");
        }
    }
}