﻿using System;

namespace PrimitiveSharp.Core
{
    public static class Logger
    {
        public static void WriteLine(int level, string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
    }
}
