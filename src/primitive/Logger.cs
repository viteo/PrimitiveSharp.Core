using System;
using System.Collections.Generic;
using System.Text;

namespace primitive
{
    public static class Logger
    {
        public static void WriteLine(int level, string format, params object[] args)
        {
            if (Parameters.LogLevel >= level)
            {
                Console.WriteLine(format, args);
            }
        }
    }
}
