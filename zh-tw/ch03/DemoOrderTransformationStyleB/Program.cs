using System;
using System.Collections.Generic;

namespace DemoOrderTransformationStyleB;

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
    string? CancelReason = null);

public sealed record OrderTransitionResult(
    bool IsSuccess,
    Order? Order,
    string? ErrorMessage)
{
    public static OrderTransitionResult Success(Order order)
        => new(true, order, null);

    public static OrderTransitionResult Fail(string message)
        => new(false, null, message);
}

public static class OrderWorkflow
{
    public static OrderTransitionResult Submit(
        Order order, DateTime now)
    {
        if (order.Status != OrderStatus.Draft)
        {
            return OrderTransitionResult.Fail(
                $"只有草稿可以提交，目前狀態：{order.Status}");
        }

        var submitted = order with
        {
            Status = OrderStatus.Submitted,
            SubmittedAt = now
        };

        return OrderTransitionResult.Success(submitted);
    }

    public static OrderTransitionResult Pay(
        Order order, DateTime now)
    {
        if (order.Status != OrderStatus.Submitted)
        {
            return OrderTransitionResult.Fail(
                $"只有已提交訂單可付款，目前狀態：{order.Status}");
        }

        var paid = order with
        {
            Status = OrderStatus.Paid,
            PaidAt = now
        };

        return OrderTransitionResult.Success(paid);
    }

    public static OrderTransitionResult Ship(
        Order order, DateTime now)
    {
        if (order.Status != OrderStatus.Paid)
        {
            return OrderTransitionResult.Fail(
                $"只有已付款訂單可出貨，目前狀態：{order.Status}");
        }

        var shipped = order with
        {
            Status = OrderStatus.Shipped,
            ShippedAt = now
        };

        return OrderTransitionResult.Success(shipped);
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
        var submitResult = OrderWorkflow.Submit(draft, now);
        if (!submitResult.IsSuccess)
        {
            Console.WriteLine($"提交失敗：{submitResult.ErrorMessage}");
            return;
        }

        var submitted = submitResult.Order!;
        Console.WriteLine($"提交後狀態：{submitted.Status}");

        var payResult = OrderWorkflow.Pay(submitted, now.AddMinutes(5));
        if (!payResult.IsSuccess)
        {
            Console.WriteLine($"付款失敗：{payResult.ErrorMessage}");
            return;
        }

        var paid = payResult.Order!;
        Console.WriteLine($"付款後狀態：{paid.Status}");
    }
}
