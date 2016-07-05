using System;
using System.Diagnostics;

namespace Benchmarks.NET
{
    public class BenchmarkMain
    {
        private static Stopwatch _stopwatch;
        private static Serialization _serialization;

        public static void Main(string[] args)
        {
            _stopwatch = new Stopwatch();
            _serialization = new Serialization();

            for (int i = 0; i < 10; i++)
            {
                _stopwatch.Start();
                _serialization.JsonSerialize();
                _stopwatch.Stop();
                Console.WriteLine("Json: " + _stopwatch.ElapsedMilliseconds);
                _stopwatch.Reset();

                _stopwatch.Start();
                _serialization.MsgPackSerialize();
                _stopwatch.Stop();
                Console.WriteLine("MsgPack: " + _stopwatch.ElapsedMilliseconds);
                _stopwatch.Reset();
            }
        }
    }
}
