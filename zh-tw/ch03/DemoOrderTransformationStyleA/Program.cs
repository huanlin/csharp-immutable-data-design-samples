using System;
using System.Collections.Generic;

namespace DemoOrderTransformationStyleA;

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
    DateTime CreatedAt,
    DateTime? SubmittedAt = null,
    DateTime? PaidAt = null,
    DateTime? ShippedAt = null,
    string? CancelReason = null)
{
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

    public Order Ship(DateTime now)
    {
        if (Status != OrderStatus.Paid)
            throw new InvalidOperationException(
                "只有已付款的訂單可以出貨");

        return this with
        {
            Status = OrderStatus.Shipped,
            ShippedAt = now
        };
    }

    public Order Cancel(string reason)
    {
        if (Status == OrderStatus.Shipped)
            throw new InvalidOperationException("已出貨的訂單無法取消");

        return this with
        {
            Status = OrderStatus.Cancelled,
            CancelReason = reason
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
            "ORD-001",
            "CUST-42",
            OrderStatus.Draft,
            lines,
            500m,
            DateTime.UtcNow);

        Console.WriteLine($"初始狀態：{draft.Status}");

        var now = DateTime.UtcNow;
        var submitted = draft.Submit(now);
        Console.WriteLine($"提交後狀態：{submitted.Status}");

        var paid = submitted.Pay(now.AddMinutes(5));
        Console.WriteLine($"付款後狀態：{paid.Status}");

        var shipped = paid.Ship(now.AddHours(2));
        Console.WriteLine($"出貨後狀態：{shipped.Status}");
    }
}
