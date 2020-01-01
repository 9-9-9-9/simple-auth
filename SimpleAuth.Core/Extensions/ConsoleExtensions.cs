using System;

namespace SimpleAuth.Core.Extensions
{
    public static class ConsoleExtensions
    {
        public static void Write(this object obj)
        {
            Console.WriteLine(obj ?? "null");
        }
    }
}