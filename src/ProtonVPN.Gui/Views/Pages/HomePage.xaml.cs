using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json.Linq;
using ProtonVPN.Gui.ViewModels.Pages;

namespace ProtonVPN.Gui.Views.Pages;

public sealed partial class HomePage : Page
{
    public HomeViewModel ViewModel
    {
        get;
    }

    public HomePage()
    {
        ViewModel = App.GetService<HomeViewModel>();
        InitializeComponent();

        SplitViewArea.SizeChanged += OnSplitViewAreaSizeChanged;
    }

    private void OnSplitViewAreaSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var inlineModeThresholdWidth = 620.0;

        SplitViewArea.DisplayMode = SplitViewArea.ActualWidth >= inlineModeThresholdWidth
            ? SplitViewDisplayMode.Inline
            : SplitViewDisplayMode.Overlay;
    }
}
