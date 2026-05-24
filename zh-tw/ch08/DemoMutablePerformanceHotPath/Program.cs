using System;
using System.Diagnostics;

namespace DemoMutablePerformanceHotPath;

public sealed class MutableOrderLine
{
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public sealed record ImmutableOrderLine(int Quantity, decimal Price);

public class Program
{
    private const int Iterations = 10_000_000;

    public static void Main()
    {
        Console.WriteLine($"執行 {Iterations:N0} 次狀態更新測試...");

        // 1. 測試 Mutable state 原地修改
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long startAllocated = GC.GetAllocatedBytesForCurrentThread();
        var sw = Stopwatch.StartNew();

        var mLine = new MutableOrderLine();
        for (int i = 0; i < Iterations; i++)
        {
            mLine.Quantity = i;
            mLine.Price = i;
        }

        sw.Stop();
        long endAllocated = GC.GetAllocatedBytesForCurrentThread();
        long mutableAllocated = endAllocated - startAllocated;
        TimeSpan mutableTime = sw.Elapsed;

        Console.WriteLine("\n[可變 (Mutable) 原地修改]");
        Console.WriteLine($"時間：{mutableTime.TotalMilliseconds:F2} ms");
        Console.WriteLine($"配置：{mutableAllocated / 1024.0:F2} KB");

        // 2. 測試 Immutable state 非破壞性修改
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        startAllocated = GC.GetAllocatedBytesForCurrentThread();
        sw.Restart();

        var imLine = new ImmutableOrderLine(0, 0);
        for (int i = 0; i < Iterations; i++)
        {
            imLine = imLine with { Quantity = i, Price = i };
        }

        sw.Stop();
        endAllocated = GC.GetAllocatedBytesForCurrentThread();
        long immutableAllocated = endAllocated - startAllocated;
        TimeSpan immutableTime = sw.Elapsed;

        Console.WriteLine("\n[不可變 (Immutable) non-destructive]");
        Console.WriteLine($"時間：{immutableTime.TotalMilliseconds:F2} ms");
        Console.WriteLine(
            $"配置：{immutableAllocated / 1024.0 / 1024.0:F2} MB");
    }
}
