using CashRegister.Data;

namespace CashRegister.API;

public class CashRegisterEndpoints
{
    public static async Task<IResult> AddInitialData(ApplicationDataContext context)
    {
        throw new NotImplementedException();
    }

    public static async Task<IResult> Checkout(ApplicationDataContext context, List<ReceiptLineDto> receiptLines)
    {
        throw new NotImplementedException();
    }
    public record ReceiptLineDto(int ItemId, int Quantity); // calculate rest

    public static async Task<IResult> GetItems(ApplicationDataContext context)
    {
        throw new NotImplementedException();
    }
}