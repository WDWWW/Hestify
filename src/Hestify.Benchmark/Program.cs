using System;
using BenchmarkDotNet.Running;

namespace Hestify.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<XmlConvertScenario>();
        }
    }
}