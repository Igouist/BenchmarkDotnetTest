using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace BenchmarkDotnetTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<ParamsTest>();
        }
    }

    #region -- Empty Vs NewList --

    /// <summary>
    /// 測試用擂台
    /// </summary>
    [HtmlExporter]
    [AsciiDocExporter]
    [CsvExporter]
    [CsvMeasurementsExporter]
    [PlainExporter]
    [RPlotExporter]

    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.Net472)]
    [SimpleJob(RuntimeMoniker.NetCoreApp30)]
    public class EmptyVSNewList
    {
        [Benchmark] public void Empty() => Enumerable.Empty<Foo>();
        [Benchmark] public void NewList() => new List<Foo>();
    }

    /// <summary>
    /// 協力單位，用來當成串列的填充物
    /// </summary>
    public class Foo
    {
        public Guid Id { get; set; }
        public string Bar { get; set; }
    }

    #endregion

    #region -- Params --

    public class ParamsTest
    {
        public IEnumerable<int> SourceA => new [] { 10, 10000 };
        public IEnumerable<int> SourceB => new[] { 2, 20000 };

        [ParamsSource(nameof(SourceA))]
        public int A { get; set; }

        [ParamsSource(nameof(SourceB))]
        public int B { get; set; }

        [Benchmark] public int Cul() => A * B;
    }

    #endregion
}
