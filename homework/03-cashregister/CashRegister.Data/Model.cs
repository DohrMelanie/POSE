using System.ComponentModel.DataAnnotations;

namespace CashRegister.Data;

public class Item
{
    public int Id { get; set; }
    [MaxLength(100)] public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int? Amount { get; set; }
    [MaxLength(20)] public string? AmountName { get; set; } = string.Empty;
}

public class ReceiptLine
{
    public int Id { get; set; }
    public int ReceiptId { get; set; }
    public Receipt Receipt { get; set; } = null!;
    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
}

public class Receipt
{
    public int Id { get; set; }
    public List<ReceiptLine> ReceiptLines { get; set; } = [];
    public decimal Total { get; set; }
}