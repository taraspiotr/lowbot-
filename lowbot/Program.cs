using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace lowbot
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch StopWatch = new Stopwatch();
            StopWatch.Start();

            string Path = @"E:\Lowbot\strategy.xml";
            DrawTrainer T = new DrawTrainer(200000000, 6, Path);
            T.main();

            StopWatch.Stop();
            Console.WriteLine("Runtime = {0}s", StopWatch.ElapsedMilliseconds / 1000);
            Console.ReadKey();
        }
    }
}
