# C#: BenchmarkDotnet —— 效能測試好簡單

> 本文同步發表於部落格（好讀版 →）：https://igouist.github.io/post/2021/06/benchmarkdotnet/

![](https://i.imgur.com/rhmeAUi.png)

「你寫那什麼鬼東西？這個ＯＯＯ寫法比較好啦！」<br/>
『聽你在屁！明明是這個ＸＸＸ寫法快= =』

哇喔！等等！**想戰效能嗎**？那你一定需要這款 **BenchmarkDotnet**！

## 介紹與安裝

![](https://github.com/dotnet/BenchmarkDotNet/raw/master/docs/logo/logo-wide.png)

我們在 Coding 的時候，或多或少都會有「不知道這兩個寫法哪個比較好？」、「聽說Ａ寫法比Ｂ寫法快，真的嗎？」這類關於效能的疑問。

在遠古時期，當我們需要驗證這種想法，可能就要用記錄秒數的方式，或是搭配迴圈、然後再印在畫面上等等這類土法煉鋼的方式。

然而這種單純計秒數的 Print 流測試，可能比較到了時間成本，卻忽略了吃掉的記憶體這些空間成本；又或是每次都要插一堆列印文字的語句，因為麻煩就萌生退意等等…

這時候就是 BenchmarkDotnet 出場的時候啦！  

**BenchmarkDotnet 是一款簡單好用的效能比較工具，可以幫助我們比對多組程式碼，並告訴我們平均的執行時間、耗用的記憶體等等。**

只要使用 BenchmarkDotnet 這個神奇妙妙幫手，它就能幫我們搞定這些麻煩的事情，讓我們可以專注在要測試的程式碼內容囉。

<!--more-->

接下來要記錄的部份有：

- [介紹與安裝](#介紹與安裝)
- [使用 Benchmark 來指定參賽選手](#使用-benchmark-來指定參賽選手)
- [使用 MemoryDiagnoser 加上記憶體的比較](#使用-memorydiagnoser-加上記憶體的比較)
- [使用 Jobs 來指定測試環境](#使用-jobs-來指定測試環境)
- [使用 Exporters 來產生報表](#使用-exporters-來產生報表)
- [用 Params 來指定數值](#用-params-來指定數值)
- [結語與參考資料](#結語與參考資料)

這邊讓我們來實際上跑過一次基本用法並記錄吧！

首先讓我們先到 Nuget 安裝 `BenchmarkDotnet`，因為依賴套件蠻多的，可能需要等待一下。

![](https://i.imgur.com/aqv4KvE.png)

> 補充：本篇的專案範本是「主控台應用程式」，不過平常都是直接使用簡單好用的 [Linqpad](https://ithelp.ithome.com.tw/articles/10193063) 快速測一下比較多。
> 
> 不過反正 BenchmarkDotnet 已經夠太簡潔方便了，各位朋友用順手的方式試試看就好囉。

安裝完畢之後，就可以開始準備一下參賽選手的擂台啦～

## 使用 Benchmark 來指定參賽選手

既然都說要比較效能了，今天就挑個前陣子同事提到過的主題來比較吧：

「**在需要回傳空串列的場合，使用 `Enumerable.Empty` 會比 `new List<>` 更好一些**」

> 針對這個問題，附上補充資訊：[Is it better to use Enumerable.Empty<T>() as opposed to new List<T>() to initialize an IEnumerable<T>? - Stackoverflow](https://stackoverflow.com/questions/1894038/is-it-better-to-use-enumerable-emptyt-as-opposed-to-new-listt-to-initial)

現在讓我們用這個主題來測試吧，假定：

- 紅方選手：用 `Enumerable.Empty` 來建立空串列
- 藍方選手：用 `new List<>` 來建立空串列

因為我們是要測試兩者做出空串列的差異，至少也要有個類別：
```csharp
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
```

然後讓我們開始正式的佈置，先讓我們**開一個 Class 來當作擂台**：

```csharp
/// <summary>
/// 測試用擂台
/// </summary>
public class EmptyVSNewList
{

}
```

然後請紅方選手上擂台：

```csharp
// 紅方選手
[Benchmark]
public void Empty()
{
    Enumerable.Empty<Foo>();
}
```

以及我們的藍方選手：

```csharp
// 藍方選手
[Benchmark]
public void NewList()
{
    new List<Foo>();
}
```

> 補充：要直接 `Empty() => Enumerable.Empty<Foo>();` 也是可以的。
> 
> 我個人習慣都用同一個準備好的 BenchmarkDotnet 擂台，複製改改裡面的內容就拿來測試了，所以乖乖寫出整個 Function 改起來比較方便 XD
> 
> 但為了閱讀方便，本篇後續會整理成比較簡單的 lambda 寫法，請不要太在意。

選手們就定位之後，**替他們加上 `[Benchmark]` 的屬性，作為參賽的證明**。現在擂台上應該是長這樣的：

```csharp
/// <summary>
/// 測試用擂台
/// </summary>
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
```

接著，讓我們回到 `Main` 方法（或任何你要進行比試的地方），加上：

```csharp
var summary = BenchmarkRunner.Run<EmptyVSNewList>();
```

讓 BenchmarkRunner 去抓泛型裡面有 `[Benchmark]` 的選手進行測試。

以我們這次示範的主控台應用程式來說，可能就會像這樣：

```csharp
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
```

這樣就安排妥當啦！（其實也就加個測試用的 Class 和讓兩個測試方法而已囧）

BenchmarkDotnet 必須在 release 環境下啟動，先讓我們切換一下組態：

![](https://i.imgur.com/iKmHI8C.png)

> 小提示：如果跟我一樣，喜歡使用 Linqpad 的朋友，右下角切換成 `/o+` 才是 Release 組態呦。還有記得要用系統管理員開啟嘿。

現在雙方站定，讓我們開始比試吧！

開始執行之後，會先列一下這次測試的環境資訊，接著就會看到很多輪的比試階段（對，你不用自己寫迴圈來重複測試）：

![](https://i.imgur.com/UN7wXuS.gif)

最後就會有測試結果出爐啦：

![](https://i.imgur.com/9UEWxFg.png)

```powershell
// * Summary *

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.18363.1556 (1909/November2019Update/19H2)
Intel Core i7-7700 CPU 3.60GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=5.0.300
  [Host]     : .NET Core 3.1.15 (CoreCLR 4.700.21.21202, CoreFX 4.700.21.21402), X64 RyuJIT  [AttachedDebugger]
  DefaultJob : .NET Core 3.1.15 (CoreCLR 4.700.21.21202, CoreFX 4.700.21.21402), X64 RyuJIT


|  Method |      Mean |     Error |    StdDev |    Median |
|-------- |----------:|----------:|----------:|----------:|
|   Empty | 0.0016 ns | 0.0067 ns | 0.0062 ns | 0.0000 ns |
| NewList | 3.6262 ns | 0.0554 ns | 0.0491 ns | 3.6267 ns |
```

這邊可以看到當年統計課熟悉的那些平均值、標準差等等（不過平常戰效能的時候都直接看平均時間比較多啦）

> 補充：可以在擂台的類別上（在這個例子中就是 `EmptyVSNewList`）加上<br/> **`[MinColumn, MaxColumn]`** 的 Attribute，就會多出最大值和最小值的資訊囉。例如：
> ```powershell
> |  Method |      Mean |     Error |    StdDev |    Median |       Min |       Max |
> |-------- |----------:|----------:|----------:|----------:|----------:|----------:|
> |   Empty | 0.0005 ns | 0.0015 ns | 0.0014 ns | 0.0000 ns | 0.0000 ns | 0.0054 ns |
> | NewList | 3.7158 ns | 0.0490 ns | 0.0459 ns | 3.7072 ns | 3.6365 ns | 3.8187 ns |
> ```

現在第一階段的結果出爐啦：`Enumerable.Empty` 比 `new List<>` 快了好幾倍呢！

## 使用 MemoryDiagnoser 加上記憶體的比較

就像我們前面提到的：效能比較的時候，**除了時間以外，也不能忘了空間**！也就是說，我們還必須考量到記憶體的用量才可以。

我們在前一段知道了 `Enumerable.Empty` 比 `new List<>` 快，但是記憶體的使用呢？現在就讓我們來確認一下吧。

想要加上記憶體的測試，我們只需要在擂台的類別上，加上 `[MemoryDiagnoser]` 就行啦：

```csharp
/// <summary>
/// 測試用擂台
/// </summary>
[MemoryDiagnoser]
public class EmptyVSNewList
{
    [Benchmark] public void Empty() => Enumerable.Empty<Foo>();
    [Benchmark] public void NewList() => new List<Foo>();
}
```

接著讓我們看看結果：

![](https://i.imgur.com/D1hIAFl.png)

```powershell
// * Summary *

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.18363.1556 (1909/November2019Update/19H2)
Intel Core i7-7700 CPU 3.60GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=5.0.300
  [Host]     : .NET Core 3.1.15 (CoreCLR 4.700.21.21202, CoreFX 4.700.21.21402), X64 RyuJIT  [AttachedDebugger]
  DefaultJob : .NET Core 3.1.15 (CoreCLR 4.700.21.21202, CoreFX 4.700.21.21402), X64 RyuJIT


|  Method |      Mean |     Error |    StdDev |    Median |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------- |----------:|----------:|----------:|----------:|-------:|------:|------:|----------:|
|   Empty | 0.0028 ns | 0.0048 ns | 0.0040 ns | 0.0000 ns |      - |     - |     - |         - |
| NewList | 3.8114 ns | 0.0877 ns | 0.1077 ns | 3.8019 ns | 0.0077 |     - |     - |      32 B |
```

可以注意到，`Empty` 根本沒有動用到記憶體，反而是 `new List<>` 占了一些空間，還觸發了 GC。

由於 `Enumerable.Empty` 在時間和空間都拿下了分數，因此這邊宣布：「在需要回傳空串列的場合，使用 `Enumerable.Empty` 會比 `new List<>` 更好一些」－－**正確**！

> 這邊補一下前面提到的 StackOverflow 回答：<br/>
> "Even if you use an empty array or empty list, those are objects and they are stored in memory. The Garbage Collector has to take care of them." <br/>
> "Enumerable.Empty does not create an object per call thus putting less load on the GC."
>
> 因此如果當你的查詢有需要返回空串列的時候（例如說搜尋條件沒有結果），試試看用 `Enumerable.Empty` 吧！

## 使用 Jobs 來指定測試環境

現在我們已經考量了執行時間和記憶體，接下來要問的就是：**那在不同的啟動環境之下會有差別嗎？**

例如說同樣的比較，在 .NET Core 跟 .NET framework 都成立嗎？不需要開好幾個專案來測，在 BenchmarkDotnet，我們可以用 `Jobs` 來搞定。

現在讓我們試試看在擂台上掛上對應 .NET Core 的 `[SimpleJob(RuntimeMoniker.NetCoreApp30)]` 和 .NET Framework 4.7.2 的 `[SimpleJob(RuntimeMoniker.Net472)]` 來試試吧：
    
```csharp
/// <summary>
/// 測試用擂台
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net472)]
[SimpleJob(RuntimeMoniker.NetCoreApp30)]
public class EmptyVSNewList
{
    [Benchmark] public void Empty() => Enumerable.Empty<Foo>();
    [Benchmark] public void NewList() => new List<Foo>();
}
```

接著我們要編輯專案檔，**用多目標的方式把指定的框架加上去**，否則直接執行的話會跑出 `N/A`。

```xml
<TargetFrameworks>netcoreapp3.0;net472</TargetFrameworks>
<PlatformTarget>AnyCPU</PlatformTarget>
```

> 小提示：專案檔就是 .csproj，除了直接開啟編輯外，也可以從 `方案總管 -> (對專案右鍵) -> 編輯專案檔` 來開啟呦。
>
> 此外，多目標的說明和加入方式，也可以參照 Gelis 技術隨筆 的這篇 [使用多目標 TargetFrameworks 來讓 net72 可參考 .netstandard2.1 通過編譯並可使用](http://gelis-dotnet.blogspot.com/2019/12/targetframeworks-net72-netstandard21.html)

以示範專案為例，加入後的 `csproj` 檔案的 `<PropertyGroup>` 可能會長得這樣：

```xml
<PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFrameworks>netcoreapp3.0;net472</TargetFrameworks>
    <PlatformTarget>AnyCPU</PlatformTarget>
</PropertyGroup>
```

> 小提示：更改版本之後，如果編譯有發生遺失資源檔的錯誤，可以先卸載專案再重新載入試試呦。

現在讓我們來執行一次試試吧：

![](https://i.imgur.com/UqhTEPp.png)

```powershell
// * Summary *

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.18363.1556 (1909/November2019Update/19H2)
Intel Core i7-7700 CPU 3.60GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET SDK=5.0.300
  [Host]               : .NET Core 3.1.15 (CoreCLR 4.700.21.21202, CoreFX 4.700.21.21402), X64 RyuJIT  [AttachedDebugger]
  .NET Core 3.0        : .NET Core 3.1.15 (CoreCLR 4.700.21.21202, CoreFX 4.700.21.21402), X64 RyuJIT
  .NET Framework 4.7.2 : .NET Framework 4.8 (4.8.4341.0), X64 RyuJIT


|  Method |                  Job |              Runtime |      Mean |     Error |    StdDev |    Median |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------- |--------------------- |--------------------- |----------:|----------:|----------:|----------:|-------:|------:|------:|----------:|
|   Empty |        .NET Core 3.0 |        .NET Core 3.0 | 0.0563 ns | 0.0310 ns | 0.0583 ns | 0.0370 ns |      - |     - |     - |         - |
| NewList |        .NET Core 3.0 |        .NET Core 3.0 | 3.7849 ns | 0.0734 ns | 0.0686 ns | 3.7755 ns | 0.0077 |     - |     - |      32 B |
|   Empty | .NET Framework 4.7.2 | .NET Framework 4.7.2 | 2.4641 ns | 0.0754 ns | 0.0806 ns | 2.4815 ns |      - |     - |     - |         - |
| NewList | .NET Framework 4.7.2 | .NET Framework 4.7.2 | 7.9914 ns | 0.1877 ns | 0.5139 ns | 7.8857 ns | 0.0096 |     - |     - |      40 B |
```

可以看到測試結果變成了兩組：**在 `.NET Core 3.0` 和 `.NET Framework 4.7.2` 都進行了測試**！

現在我們知道不管是在 Core 還是 Framework，用 `Enumerable.Empty` 來建立空串列都比 `new List<>` 快、也更省資源了。

## 使用 Exporters 來產生報表

現在確定哪一組寫法效能比較好了。想要說服大大改用新寫法，怎麼辦呢？當然是 ~~自己偷偷改~~ 要拿出證據說服大大啦！

剛好，BenchmarkDotnet 也提供了產出報表的功能，並且可以輸出成 HTML、Markdown 等格式，只需要加上對應的屬性就可以囉，例如 **`[HtmlExporter]`**、**`[CsvExporter]`**，現在讓我們一股腦都丟上去看看：

```csharp
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
```
當我們執行的時候，會存一份 Log 在 `.\BenchmarkDotNet.Artifacts` 裡面

而當我們有指定輸出報表，當執行完畢之後，報表就會產生在 Log 的 `result`，也就是 `.\BenchmarkDotNet.Artifacts\results` 裡面。

以我的環境為例，就會產生在專案裡的 `\bin\Release\netcoreapp3.0\BenchmarkDotNet.Artifacts\results`：

![](https://i.imgur.com/O2VCMNC.png)

其中每個格式的樣子會不太一樣（有點廢話），例如 HTML 的是長這樣：

![](https://i.imgur.com/zCiQ5Js.png)

而看起來最厲害的應該是用[Ｒ](https://www.r-project.org/)的，不過因為我沒有安裝Ｒ，這邊就請大家看一下官網的圖過過癮唄：

![](https://benchmarkdotnet.org/images/v0.12.0/rplot.png)

至於報表的詳細操作，大家可以官方介紹的 [Exporters](https://benchmarkdotnet.org/articles/configs/exporters.html) 頁面。

不過我是覺得大多數的場合，光是能拿出執行時間和記憶體用量的比較就已經很有說服力了啦 XD

## 用 Params 來指定數值

最後補充一下：有時候我們測試的對象，可能也會受到內容物的影響。

例如一些純計算的方法，可能就會受到數值大小的影響，這時候我們就可以**使用 `Params` 的屬性來指定數值**，並觀察不同狀況下的表現。例如：

```csharp
public class ParamsTest
{
    [Params(10, 10000)]
    public int A { get; set; }

    [Params(2, 20000)]
    public int B { get; set; }

    [Benchmark] public int Cul() => A * B;
}
```

這樣跑出來的結果就會按照指定的數值分組囉：

```powershell
| Method |     A |     B |      Mean |     Error |    StdDev | Median |
|------- |------ |------ |----------:|----------:|----------:|-------:|
|    Cul |    10 |     2 | 0.0000 ns | 0.0000 ns | 0.0000 ns | 0.0 ns |
|    Cul |    10 | 20000 | 0.0000 ns | 0.0000 ns | 0.0000 ns | 0.0 ns |
|    Cul | 10000 |     2 | 0.0071 ns | 0.0146 ns | 0.0200 ns | 0.0 ns |
|    Cul | 10000 | 20000 | 0.0008 ns | 0.0015 ns | 0.0023 ns | 0.0 ns |
```

如果想要把指定的數值另外拉出來做成串列，比較好維護和測試的話，也可以**用 `ParamsSource` 來指定數值的來源**。

例如上面的例子，也可以改寫成：

```csharp
public class ParamsTest
{
    public IEnumerable<int> SourceA => new [] { 10, 10000 };
    public IEnumerable<int> SourceB => new [] {  2, 20000 };

    [ParamsSource(nameof(SourceA))]
    public int A { get; set; }

    [ParamsSource(nameof(SourceB))]
    public int B { get; set; }

    [Benchmark] public int Cul() => A * B;
}
```

效果會是一樣的，而且也比較好管理。

## 結語與參考資料

我們的 `Enumerable.Empty` vs `new List<>` 對決也告一段落了。

當然，BenchmarkDotNet 能做到的事還有很多，例如 [用 Filter 來篩選指定的測試案例](https://benchmarkdotnet.org/articles/configs/filters.html)、[把某個測試案例作為基準案例](https://benchmarkdotnet.org/articles/features/baselines.html) 等等進階用法，還有更多的 [統計資訊](https://benchmarkdotnet.org/articles/features/statistics.html)。但目前還沒有接觸到，這邊就先不提了，有需要的朋友再翻一下參考資料唄。

這篇文章主要參考自同事提供的範例說明，以及網路大大們的介紹文。特此感謝：

- [使用 BenchmarkDotNet 測試程式碼效能](https://marcus116.blogspot.com/2019/03/netcore-net-benchmarkdotnet.html)
- [還在徒手揮汗寫For測效能，閃開讓BenchmarkDotNet來](https://blog.kkbruce.net/2017/01/donot-use-for-use-benchmark-dotnet.html#.YIFD8qziuUl)
- [Benchmarking Your .NET Core Code With BenchmarkDotNet](https://dotnetcoretutorials.com/2017/12/04/benchmarking-net-core-code-benchmarkdotnet/)

還有相當完善的官方文件：

- [Getting started - benchmarkdotnet.org](https://benchmarkdotnet.org/articles/guides/getting-started.html)

最後，每當你想要問「甘安捏」的時候：

![](https://i.imgur.com/VpNXLeM.png)

**用 BenchmarkDotnet 跑一遍就對啦！** 都給我戰起來！
