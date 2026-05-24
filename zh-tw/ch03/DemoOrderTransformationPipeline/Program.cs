using System;
using System.Collections.Generic;

namespace DemoOrderTransformationPipeline;

public enum OrderStatus
{
    Draft,
    Submitted,
    Paid,
    Shipped,
    Cancelled
}

public sealed record OrderLine(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice)
{
    public decimal Amount => Quantity * UnitPrice;
}

public sealed record Order(
    string Id,
    string CustomerId,
    OrderStatus Status,
    IReadOnlyList<OrderLine> Lines,
    decimal Total,
    decimal DiscountRate,
    decimal ShippingFee,
    DateTime CreatedAt,
    DateTime? SubmittedAt = null,
    DateTime? PaidAt = null,
    DateTime? ShippedAt = null)
{
    // 計算最終金額
    public decimal FinalTotal => (Total * DiscountRate) + ShippingFee;

    public Order ApplyDiscount(decimal rate)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("只有草稿可以套用折扣");

        return this with { DiscountRate = rate };
    }

    public Order ApplyShippingFee(decimal fee)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("只有草稿可以計算運費");

        return this with { ShippingFee = fee };
    }

    public Order Submit(DateTime now)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("只有草稿可以提交");

        return this with
        {
            Status = OrderStatus.Submitted,
            SubmittedAt = now
        };
    }

    public Order Pay(DateTime now)
    {
        if (Status != OrderStatus.Submitted)
            throw new InvalidOperationException(
                "只有已提交的訂單可以付款");

        return this with
        {
            Status = OrderStatus.Paid,
            PaidAt = now
        };
    }
}

public class Program
{
    public static void Main()
    {
        var lines = new List<OrderLine>
        {
            new("P001", "C# 不可變資料設計入門", 1, 500m)
        };

        var draft = new Order(
            Id: "ORD-001",
            CustomerId: "CUST-42",
            Status: OrderStatus.Draft,
            Lines: lines,
            Total: 500m,
            DiscountRate: 1.0m,
            ShippingFee: 0m,
            CreatedAt: DateTime.UtcNow);

        var now = DateTime.UtcNow;

        // 轉換管線：鏈式呼叫
        var processedOrder = draft
            .ApplyDiscount(0.9m)       // 9 折折扣
            .ApplyShippingFee(60m)     // 運費 60 元
            .Submit(now)               // 提交
            .Pay(now.AddMinutes(5));   // 付款

        Console.WriteLine($"原始訂單總金額：{draft.FinalTotal}");
        Console.WriteLine($"處理後訂單狀態：{processedOrder.Status}");
        Console.WriteLine($"處理後折扣率：{processedOrder.DiscountRate}");
        Console.WriteLine($"處理後運費：{processedOrder.ShippingFee}");
        Console.WriteLine($"處理後最終金額：{processedOrder.FinalTotal}");
    }
}
