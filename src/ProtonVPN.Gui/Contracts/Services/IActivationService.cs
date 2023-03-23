namespace ProtonVPN.Gui.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
