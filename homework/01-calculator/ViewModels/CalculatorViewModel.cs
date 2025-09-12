using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace _01_calculator.ViewModels;

public partial class CalculatorViewModel : ViewModelBase
{
    private const string DISPLAY_EMPTY = "[ Display Area ]";
    [ObservableProperty] private string displayText = DISPLAY_EMPTY;
    private string memory = "";

    
    [RelayCommand]
    private void Append(string value)
    {
        if (DisplayText == DISPLAY_EMPTY)
        {
            DisplayText = "";
        }
        DisplayText += value;
    }

    [RelayCommand]
    private void Clear()
    {
        DisplayText = DISPLAY_EMPTY;
    }

    [RelayCommand]
    private void Store()
    {
        memory = DisplayText;
    }

    [RelayCommand]
    private void Recall()
    {
        DisplayText += memory;
    }
    
    [RelayCommand]
    private void ClearMemory()
    {
        memory = "";
    }

    [RelayCommand]
    private async Task Calculate()
    {
        var result = 0;
        if (!DisplayText.Any(c => "+-*/".Contains(c)))
        {
            result = int.Parse(DisplayText);
            DisplayText = result.ToString();
            return;
        }

        var numbers = new List<int>();
        var operators = new List<char>();
        var currentNumber = "";
        var isPositive = true;

        for (var i = 0; i < DisplayText.Length; i++)
        {
            var c = DisplayText[i];
            if (i == 0 && c == '-')
            {
                isPositive = false;
            }
            else if ("+*/-".Contains(c) && !string.IsNullOrEmpty(currentNumber))
            {
                numbers.Add(int.Parse(isPositive ? currentNumber : "-" + currentNumber));
                isPositive = true;
                operators.Add(c);
                currentNumber = "";
            }
            else if (char.IsDigit(c))
            {
                currentNumber += c;
            }
        }

        if (!string.IsNullOrEmpty(currentNumber))
        {
            numbers.Add(int.Parse(currentNumber));
        }

        result = numbers[0];

        for (var i = 0; i < operators.Count && i + 1 < numbers.Count; i++)
        {
            var nextNumber = numbers[i + 1];
            var op = operators[i];
            
            if (nextNumber == 0 && op == '/')
            {
                var box = MessageBoxManager
                    .GetMessageBoxStandard("Caption", "Are you sure you would like to delete appender_replace_page_1?",
                        ButtonEnum.YesNo);
                var message = await box.ShowAsync();
                DisplayText = DISPLAY_EMPTY;
            }
            switch (op)
            {
                case '+':
                    result += nextNumber;
                    break;
                case '-':
                    result -= nextNumber;
                    break;
                case '*':
                    result *= nextNumber;
                    break;
                case '/':
                    result /= nextNumber;
                    break;
            }
        }
        DisplayText = result.ToString();
    }
}