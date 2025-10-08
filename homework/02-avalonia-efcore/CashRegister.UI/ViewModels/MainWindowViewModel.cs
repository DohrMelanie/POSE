using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CashRegister.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;

namespace CashRegister.UI.ViewModels;

public partial class MainWindowViewModel: ViewModelBase
{
    private readonly ApplicationDataContext dbContext;
    [ObservableProperty] 
    private ObservableCollection<Item> items = [];
    public ObservableCollection<ReceiptLineViewModel> ReceiptLines { get; } = [];
    
    [ObservableProperty] 
    private decimal sumOfReceipt;

    public MainWindowViewModel(IDbContextFactory<ApplicationDataContext> contextFactory) {
        dbContext = contextFactory.CreateDbContext();
        AddInitialData();
    }

    private async Task AddInitialData()
    {
        if (!dbContext.Items.Any())
        {
            dbContext.Items.Add(new Item() { Name = "Bananen", Price = 1.99m, Amount = 1, AmountName = "kg" });
            dbContext.Items.Add(new Item() { Name = "Äpfel", Price = 2.99m, Amount = 1, AmountName = "kg" });
            dbContext.Items.Add(new Item() { Name = "Trauben Weiß", Price = 2.49m, Amount = 500, AmountName = "g" });
            dbContext.Items.Add(new Item() { Name = "Himbeeren", Price = 2.49m, Amount = 125, AmountName = "g" });
            dbContext.Items.Add(new Item() { Name = "Karotten", Price = 3.49m, Amount = 500, AmountName = "g" });
            dbContext.Items.Add(new Item() { Name = "Eissalat", Price = 0.99m, Amount = 1, AmountName = " Stück" });
            dbContext.Items.Add(new Item() { Name = "Zucchini", Price = 0.99m, Amount = 1, AmountName = " Stück" });
            dbContext.Items.Add(new Item() { Name = "Knoblauch", Price = 0.99m, Amount = 1, AmountName = " Stück" });
            dbContext.Items.Add(new Item() { Name = "Joghurt", Price = 0.49m, Amount = 200, AmountName = "g" });
            dbContext.Items.Add(new Item() { Name = "Butter", Price = 1.49m });
            await dbContext.SaveChangesAsync();
        }

        foreach (var item in await dbContext.Items.ToListAsync())
        {
            Items.Add(item);
        }
    }
    
    [RelayCommand]
    public void AddItemToReceiptCommand(Item item)
    {
        var existingLine = ReceiptLines.FirstOrDefault(l => l.ItemId == item.Id);
        if (existingLine == null)
        {
            var line = new ReceiptLineViewModel
            {
                ItemId = item.Id,
                Item = item,
                Quantity = 1,
                TotalPrice = item.Price
            };
            ReceiptLines.Add(line);
        }
        else
        {
            existingLine.Quantity++;
            existingLine.TotalPrice = existingLine.Quantity * item.Price;
        }
        SumOfReceipt = ReceiptLines.Sum(l => l.TotalPrice);
    }

    [RelayCommand]
    public async Task CheckoutCommand()
    {
        var receipt = new Receipt
        {
            Total = SumOfReceipt,
            ReceiptLines = ReceiptLines.Select(l => new ReceiptLine
            {
                ItemId = l.ItemId,
                Quantity = l.Quantity,
                TotalPrice = l.TotalPrice
            }).ToList()
        };
        dbContext.Receipts.Add(receipt);
        await dbContext.SaveChangesAsync();
        ReceiptLines.Clear();
        SumOfReceipt = 0;
    }
}

public partial class ReceiptLineViewModel : ObservableObject
{
    [ObservableProperty]
    private int itemId;
    
    [ObservableProperty]
    private Item? item;
    
    [ObservableProperty]
    private int quantity;
    
    [ObservableProperty]
    private decimal totalPrice;
}
