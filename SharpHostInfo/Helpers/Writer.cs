using System;

namespace SharpHostInfo.Helpers
{
    public class Writer
    {
        public static void Info(string info)
        {
            Console.WriteLine($"[*] {info}");
        }

        public static void Line(string info)
        {
            Console.WriteLine($"{info}");
        }

        public static void Failed(string log)
        {
            Console.WriteLine($"[!] Failed: {log}");
        }

        public static void Error(string error)
        {
            Console.WriteLine($"\r\n[!] Error: {error}");
            Environment.Exit(0);
        }
    }
}
