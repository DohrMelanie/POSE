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
    [ObservableProperty] private ObservableCollection<Item> items = [];
    [ObservableProperty] private ObservableCollection<ReceiptLine> receiptLines = [];
    [ObservableProperty] private decimal sumOfReceipt = 0;

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
    public async Task AddItemToReceiptCommand(Item item)
    {
        if (ReceiptLines.Any(l => l.ItemId == item.Id))
        {
            var line = ReceiptLines.First(l => l.ItemId == item.Id);
            line.Quantity++;
            line.TotalPrice = line.Quantity * item.Price;
        }
        else
        {
            var line = new ReceiptLine();
            line.ItemId = item.Id;
            line.Item = item;
            line.Quantity = 1;
            line.TotalPrice = item.Price;
            ReceiptLines.Add(line);
            await dbContext.ReceiptLines.AddAsync(line);
        }
        SumOfReceipt = ReceiptLines.Sum(l => l.TotalPrice);
        
        await dbContext.SaveChangesAsync();
    }

    [RelayCommand]
    public async Task CheckoutCommand()
    {
        
    }
}
