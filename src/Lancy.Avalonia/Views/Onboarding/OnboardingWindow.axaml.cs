using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Lancy.Avalonia.ViewModels.Onboarding;
using Lancy.Avalonia.ViewModels.Shell;
using Lancy.Avalonia.Views.Shell;
using Microsoft.Extensions.DependencyInjection;

namespace Lancy.Avalonia.Views.Onboarding;

public partial class OnboardingWindow : Window
{
    public OnboardingWindow()
    {
        InitializeComponent();
        try
        {
            Icon = new WindowIcon(global::Avalonia.Platform.AssetLoader.Open(new System.Uri("avares://Lancy/Assets/app.ico")));
        }
        catch
        {
            // Fallback to XAML Icon attribute
        }
        DataContextChanged += (_, _) =>
        {
            if (DataContext is OnboardingFlowViewModel vm)
                vm.OnboardingCompleted += OnOnboardingCompleted;
        };
    }

    private void OnOnboardingCompleted()
    {
        var mainVm = App.Services.GetRequiredService<MainWindowViewModel>();
        var mainWindow = new MainWindow { DataContext = mainVm };

        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = mainWindow;

        mainWindow.Show();
        Close();
    }
}
