using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CashRegister.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace CashRegister.UI.ViewModels;

public partial class MainWindowViewModel: ViewModelBase
{
    private readonly ApplicationDataContext dbContext;
    [ObservableProperty] private ObservableCollection<Item> items = [];

    public MainWindowViewModel(IDbContextFactory<ApplicationDataContext> contextFactory) {
        dbContext = contextFactory.CreateDbContext();
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
            dbContext.SaveChanges();
        }
        foreach (var item in dbContext.Items)
        {
            Items.Add(item);
        }
    }
    [RelayCommand]
    private async Task AddItem()
    {
        await dbContext.SaveChangesAsync();
    }

    [RelayCommand]
    private async Task GetFirstSampleRow()
    {/*
        var firstGreeting = await dbContext.Greetings.FirstOrDefaultAsync();
        if (firstGreeting != null)
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Information",
                firstGreeting.GreetingText, ButtonEnum.Ok);
            await box.ShowAsync();
        }*/
    }
}
