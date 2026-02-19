using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using VRCX.App.ViewModels;

namespace VRCX.App.Views;

public partial class OverlayDebugWindow : Window
{
    public OverlayDebugWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        Dispatcher.UIThread.InvokeAsync(Load);
    }

    private async Task Load()
    {
        if (DataContext is OverlayDebugWindowViewModel viewModel)
        {
            await viewModel.LoadAsync().ConfigureAwait(true);
        }
    }
}