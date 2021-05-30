using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace BenchmarkDotnetTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<EmptyVSNewList>();
        }
    }

    /// <summary>
    /// 測試用擂台
    /// </summary>
    [MemoryDiagnoser]
    public class EmptyVSNewList
    {
        // 紅方選手
        [Benchmark]
        public void Empty()
        {
            Enumerable.Empty<Foo>();
        }

        // 藍方選手
        [Benchmark]
        public void NewList()
        {
            new List<Foo>();
        }
    }

    /// <summary>
    /// 協力單位，用來當成串列的填充物
    /// </summary>
    public class Foo
    {
        public Guid Id { get; set; }
        public string Bar1 { get; set; }
        public string Bar2 { get; set; }
        public string Bar3 { get; set; }
        public string Bar4 { get; set; }
        public string Bar5 { get; set; }
    }
}
