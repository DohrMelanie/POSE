using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace _01_calculator.ViewModels;

public partial class CalculatorViewModel : ViewModelBase
{
    [ObservableProperty] private string displayText = "[ Display Area ]";
    
    [RelayCommand]
    private void Append(string value)
    {
        if (DisplayText[0] == '[')
        {
            DisplayText = "";
        }
        DisplayText += value;
    }

    [RelayCommand]
    private void Clear()
    {
        DisplayText = "[ Display Area ]";
    }

    [RelayCommand]
    private void Calculate()
    {
        var result = 0;

        if (DisplayText.Length == 1)
        {
            result = DisplayText[0];
        }
        else
        {
            for (var i = 0; i < DisplayText.Length; i += 3)
            {
                if (DisplayText[i + 2] - '0' == 0 && DisplayText[i + 1] == '/')
                {
                    
                }
                result += CalculateExpression(DisplayText[i] - '0', DisplayText[i + 2] - '0', DisplayText[i + 1]);
            }
        }
        DisplayText = result.ToString();
    }

    private static int CalculateExpression(int nr1, int nr2, char selectedOperator)
    {
        return selectedOperator switch
        {
            '+' => nr1 + nr2,
            '-' => nr1 - nr2,
            '*' => nr1 * nr2,
            '/' => nr1 / nr2,
            _ => throw new InvalidOperationException()
        };
    }
}