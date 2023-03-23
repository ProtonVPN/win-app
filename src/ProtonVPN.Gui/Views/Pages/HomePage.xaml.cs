using Microsoft.UI.Xaml.Controls;
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
    }
}
