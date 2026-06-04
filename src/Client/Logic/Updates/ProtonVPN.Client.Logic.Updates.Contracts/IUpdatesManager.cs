namespace ProtonVPN.Client.Logic.Updates.Contracts;

public interface IUpdatesManager
{
    void Initialize();

    void CheckForUpdate(bool isManualCheck);

    Task UpdateAsync(bool isToOpenOnDesktop);

    void SkipCurrentUpdate();

    bool IsAutoUpdated { get; }

    bool IsAutoUpdateInProgress { get; }

    bool IsUpdateAvailable { get; }

    bool CanSkipCurrentUpdate { get; }
}
