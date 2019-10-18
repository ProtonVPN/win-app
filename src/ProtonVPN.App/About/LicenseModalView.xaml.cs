using System.Diagnostics;
using System.Windows.Input;

namespace ProtonVPN.About
{
    public partial class LicenseModalView
    {
        public LicenseModalView()
        {
            InitializeComponent();

            CommandBindings.Add(new CommandBinding(NavigationCommands.GoToPage, (sender, e) => Process.Start((string)e.Parameter)));
        }
    }
}
