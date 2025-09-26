using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using _01_Avalonia_Dependency_Injection.ViewModels;
using _01_Avalonia_Dependency_Injection.Views;
using Microsoft.Extensions.DependencyInjection;

namespace _01_Avalonia_Dependency_Injection;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            var collection = new ServiceCollection();
            collection.AddTransient<MainWindowViewModel>(); // wie builder.addServices in .NET
            collection.AddTransient<MainWindow>(); // Transient = jedes mal neu, bei GUI egal
            collection.AddTransient<TodoListView>();
            collection.AddTransient<TodoListViewModel>(); // Reihenfolge ist egal
            // AddDbContext muss man machen! in DI factory geben, jedes Fenster bekommt dann neue Instanz von DbContext, deshalb factory veröffentlichen
            // Db Context nicht als Transient weil anlegen aufwendig -> bei DI kann man mehr entscheiden als nur new + Exceptionhandling möglich
            // ... add additional services if necessary
            // collection.AddTransient<IMyService, MyService>();
             
            var services = collection.BuildServiceProvider();
            desktop.MainWindow = services.GetRequiredService<MainWindow>();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}