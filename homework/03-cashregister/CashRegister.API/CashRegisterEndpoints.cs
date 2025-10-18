using CashRegister.Data;
using Microsoft.EntityFrameworkCore;

namespace CashRegister.API;

public class CashRegisterEndpoints
{
    public static async Task<IResult> AddInitialData(ApplicationDataContext context)
    {
        if (!context.Items.Any())
        {
            var items = new List<Item>
            {
                new() { Name = "Bananen", Price = 1.99m, Amount = 1, AmountName = "kg" },
                new() { Name = "Äpfel", Price = 2.99m, Amount = 1, AmountName = "kg" },
                new() { Name = "Trauben Weiß", Price = 2.49m, Amount = 500, AmountName = "g" },
                new() { Name = "Himbeeren", Price = 2.49m, Amount = 125, AmountName = "g" },
                new() { Name = "Karotten", Price = 3.49m, Amount = 500, AmountName = "g" },
                new() { Name = "Eissalat", Price = 0.99m, Amount = 1, AmountName = " Stück" },
                new() { Name = "Zucchini", Price = 0.99m, Amount = 1, AmountName = " Stück" },
                new() { Name = "Knoblauch", Price = 0.99m, Amount = 1, AmountName = " Stück" },
                new() { Name = "Joghurt", Price = 0.49m, Amount = 200, AmountName = "g" },
                new() { Name = "Butter", Price = 1.49m }
            };

            context.Items.AddRange(items);
            await context.SaveChangesAsync();
        }
        return Results.Ok();
    }

    public static async Task<IResult> Checkout(ApplicationDataContext context, List<ReceiptLineDto> receiptLines)
    {
        var receipt = new Receipt();
        var total = 0m;

        foreach (var lineDto in receiptLines)
        {
            var item = await context.Items.FindAsync(lineDto.ItemId);
            if (item == null)
            {
                return Results.BadRequest($"Item with ID {lineDto.ItemId} not found.");
            }

            var lineTotalPrice = item.Price * lineDto.Quantity;
            total += lineTotalPrice;

            var receiptLine = new ReceiptLine
            {
                ItemId = lineDto.ItemId,
                Quantity = lineDto.Quantity,
                TotalPrice = lineTotalPrice
            };

            receipt.ReceiptLines.Add(receiptLine);
        }

        receipt.Total = total;
        context.Receipts.Add(receipt);
        await context.SaveChangesAsync();

        return Results.Ok(receipt);
    }
    public record ReceiptLineDto(int ItemId, int Quantity); // calculate rest

    public static async Task<IResult> GetItems(ApplicationDataContext context)
    {
        var items = await context.Items.ToListAsync();
        return Results.Ok(items);
    }
}