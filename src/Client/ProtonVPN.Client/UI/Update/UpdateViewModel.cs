using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Commands;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Logic.Updates.Contracts;

namespace ProtonVPN.Client.UI.Update;

public partial class UpdateViewModel : ViewModelBase
{
    private readonly IUpdatesManager _updatesManager;

    public IAsyncRelayCommand UpdateCommand { get; }

    public IRelayCommand SkipUpdateCommand { get; }

    public bool CanSkipUpdate => _updatesManager.CanSkipCurrentUpdate;

    public UpdateViewModel(
        IUpdateClientCommand updateClientCommand,
        IUpdatesManager updatesManager,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _updatesManager = updatesManager;

        UpdateCommand = updateClientCommand.Command;
        SkipUpdateCommand = new RelayCommand(SkipUpdate, () => CanSkipUpdate);
    }

    public void InvalidateUpdateCommands()
    {
        OnPropertyChanged(nameof(CanSkipUpdate));
        SkipUpdateCommand.NotifyCanExecuteChanged();
    }

    private void SkipUpdate()
    {
        _updatesManager.SkipCurrentUpdate();
        InvalidateUpdateCommands();
    }
}
