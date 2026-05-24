using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Frozen;

public record OrderLine(string ProductId, int Quantity);

public class SafeOrder
{
    public string Id { get; init; }
    public IReadOnlyList<OrderLine> Lines { get; }

    public SafeOrder(string id, IEnumerable<OrderLine> lines)
    {
        Id = id;
        // 防禦性複製，避免外部可變集合的修改
        Lines = lines.ToArray();
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== 示範 唯讀與不可變集合 ===");

        // 1. 防禦性複製
        var sourceList = new List<OrderLine> { new("P001", 1) };
        var order = new SafeOrder("ORD-001", sourceList);
        sourceList.Add(new("P002", 5)); // 外部修改
        Console.WriteLine($"SafeOrder 中的項目數量 (防禦性複製阻斷外部修改): {order.Lines.Count}"); // 1

        // 2. ImmutableArray default 陷阱
        ImmutableArray<OrderLine> items = default;
        Console.WriteLine($"items.IsDefault = {items.IsDefault}"); // True
        try
        {
            _ = items.Length;
        }
        catch (NullReferenceException)
        {
            Console.WriteLine("捕獲 NullReferenceException！未初始化的 ImmutableArray 預設底層是 null。");
        }

        // 3. FrozenDictionary 唯讀快取
        var dict = new Dictionary<string, string> { ["USD"] = "美元", ["TWD"] = "新台幣" };
        var frozen = dict.ToFrozenDictionary();
        Console.WriteLine($"FrozenDictionary 查詢結果: {frozen["TWD"]}");
    }
}
