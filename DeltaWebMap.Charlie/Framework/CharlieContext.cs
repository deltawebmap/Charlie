using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.Charlie.Framework
{
    public abstract class CharlieContext
    {
        public void Log(string topic, string msg)
        {
            Console.WriteLine($"[{topic}] {msg}");
        }
    }
}
