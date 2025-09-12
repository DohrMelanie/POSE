using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace _2025_09_12_Avalonia.ViewModels;

public partial class CalculatorViewModel : ViewModelBase
{
    public double FirstNumber { get; set; } = 0;
    public double SecondNumber { get; set; } = 0;
    [ObservableProperty] private double result = 0; // like signal, builds property called Result automatically
    
    public ObservableCollection<string> Operators { get; } = ["+", "-", "*", "/"];
    public string SelectedOperator { get; set; } = "+";

    [RelayCommand] // generates a CalculateCommand
    private void Calculate()
    {
        Result = SelectedOperator switch // if writing in result, UI won't update
        {
            "+" => FirstNumber + SecondNumber,
            "-" => FirstNumber - SecondNumber,
            "*" => FirstNumber * SecondNumber,
            "/" => SecondNumber / SecondNumber,
            _ => throw new InvalidOperationException()
        };
    }
}
