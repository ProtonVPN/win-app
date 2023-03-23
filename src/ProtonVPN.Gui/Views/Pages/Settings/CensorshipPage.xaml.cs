using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Gui.ViewModels.Pages.Settings;

namespace ProtonVPN.Gui.Views.Pages.Settings;

public sealed partial class CensorshipPage : Page
{
    public CensorshipViewModel ViewModel
    {
        get;
    }

    public CensorshipPage()
    {
        ViewModel = App.GetService<CensorshipViewModel>();
        InitializeComponent();
    }
}
