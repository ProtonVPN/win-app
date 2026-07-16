/*
 * Copyright (c) 2026 Proton AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Contracts.Services.Lifecycle;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Extensions;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Activation.Bases;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.GuestHole;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Models;
using ProtonVPN.Client.UI.Dialogs.DebugTools.Models;
using ProtonVPN.Client.UI.Main.Map;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Common.Core.Geographical;
using ProtonVPN.Common.Legacy.Abstract;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Restrictions;
using ProtonVPN.ProcessCommunication.Contracts.Entities.Vpn;
using ProtonVPN.StatisticalEvents.Contracts;

namespace ProtonVPN.Client.UI.Dialogs.DebugTools;

public partial class DebugToolsShellViewModel : ShellViewModelBase<IDebugToolsWindowActivator>
{
    private readonly IServersUpdater _serversUpdater;
    private readonly IServersLoader _serversLoader;
    private readonly IVpnServiceCaller _vpnServiceCaller;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IMainWindowOverlayActivator _mainWindowOverlayActivator;
    private readonly INpsSurveyWindowActivator _npsSurveyWindowActivator;
    private readonly ISettings _settings;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly IAppExitInvoker _appExitInvoker;
    private readonly ISettingsHeartbeatReporter _settingsHeartbeatReporter;
    private readonly IEnumerable<IWindowActivator> _windowActivators;
    private readonly IVpnPlanUpdater _vpnPlanUpdater;
    private readonly ICoordinatesProvider _coordinatesProvider;
    private readonly IConnectionCertificateManager _connectionCertificateManager;
    private readonly IUrlsBrowser _urlsBrowser;
    private readonly IGuestHoleManager _guestHoleManager;

    [ObservableProperty]
    private Overlay _selectedOverlay;

    [ObservableProperty]
    private Overlay _selectedDialog;

    [ObservableProperty]
    private VpnErrorTypeIpcEntity _selectedError = VpnErrorTypeIpcEntity.None;

    [ObservableProperty]
    private VpnPlan _selectedVpnPlan;

    [ObservableProperty]
    private int _xPosition;
    [ObservableProperty]
    private int _yPosition;
    [ObservableProperty]
    private int _windowWidth;
    [ObservableProperty]
    private int _windowHeight;

    public List<Overlay> OverlaysList { get; }
    public List<Overlay> DialogsList { get; }

    public List<VpnPlan> VpnPlans { get; } =
    [
        new("VPN Free", "free", 0, false),
        new("VPN Plus", "vpn", 2, false),
        new("Proton Unlimited", "bundle", 2, false),
        new("Proton Duo", "duo", 2, false),
        new("Proton Family", "family", 2, false),
        new("Proton Visionary", "visionary", 2, false),
        new("VPN Business", "vpnpro", 2, true),
        new("Proton Business", "bundlepro", 2, true),
    ];

    public DebugToolsShellViewModel(
        IVpnServiceCaller vpnServiceCaller,
        IServersUpdater serversUpdater,
        IServersLoader serversLoader,
        IUserAuthenticator userAuthenticator,
        IMainWindowOverlayActivator mainWindowOverlayActivator,
        INpsSurveyWindowActivator npsSurveyWindowActivator,
        ISettings settings,
        IEventMessageSender eventMessageSender,
        IDebugToolsWindowActivator windowActivator,
        IViewModelHelper viewModelHelper,
        IAppExitInvoker appExitInvoker,
        ISettingsHeartbeatReporter settingsHeartbeatReporter,
        IEnumerable<IWindowActivator> windowActivators,
        IVpnPlanUpdater vpnPlanUpdater,
        ICoordinatesProvider coordinatesProvider,
        IConnectionCertificateManager connectionCertificateManager,
        IUrlsBrowser urlsBrowser,
        IGuestHoleManager guestHoleManager)
        : base(windowActivator, viewModelHelper)
    {
        _serversUpdater = serversUpdater;
        _serversLoader = serversLoader;
        _vpnServiceCaller = vpnServiceCaller;
        _userAuthenticator = userAuthenticator;
        _mainWindowOverlayActivator = mainWindowOverlayActivator;
        _npsSurveyWindowActivator = npsSurveyWindowActivator;
        _settings = settings;
        _eventMessageSender = eventMessageSender;
        _appExitInvoker = appExitInvoker;
        _settingsHeartbeatReporter = settingsHeartbeatReporter;
        _windowActivators = windowActivators;
        _vpnPlanUpdater = vpnPlanUpdater;
        _coordinatesProvider = coordinatesProvider;
        _connectionCertificateManager = connectionCertificateManager;
        _urlsBrowser = urlsBrowser;
        _guestHoleManager = guestHoleManager;

        OverlaysList =
        [
            ..typeof(IMainWindowOverlayActivator).GetMethods()
                    .Where(m => m.GetParameters().Length == 0)
                    .Select(m => new Overlay
                    {
                        Id =  m.Name,
                        Name = GenerateOverlayDisplayName(m.Name)
                    })
            .ToList()
        ];
        SelectedOverlay = OverlaysList.First();

        DialogsList =
        [
            .._windowActivators
                .Where(a => a is not IMainWindowActivator or IDebugToolsWindowActivator)
                .Select(m => new Overlay
                {
                    Id =  m.GetType().ToString(),
                    Name = GenerateDialogDisplayName(m.GetType().ToString()),
                })
            .ToList()
        ];
        SelectedDialog = DialogsList.First();

        SelectedVpnPlan = VpnPlans.First();
    }

    [RelayCommand]
    public async Task TriggerRestartAsync()
    {
        // Trigger a client restart from a different thread to test the RestartAsync() method.
        // The reason is that RestartAsync() releases the client mutex and that action requires thread-affinity.
        Thread thread = new(async () => await _appExitInvoker.RestartAsync(isToOpenOnDesktop: false));
        thread.Start();
        thread.Join();
    }

    [RelayCommand]
    public void TriggerUiUnhandledException()
    {
        throw new StackOverflowException("Intentional UI-thread exception test");
    }

    [RelayCommand]
    public void TriggerAppDomainUnhandledException()
    {
        ThreadPool.QueueUserWorkItem(_ =>
        {
            throw new InvalidOperationException("Intentional AppDomain unhandled exception crash test");
        });
    }

    [RelayCommand]
    public async Task TriggerUnobservedTaskExceptionAsync()
    {
        _ = Task.Run(() => throw new Exception("Intentional unobserved task exception test"));
        await Task.Delay(500);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    [RelayCommand]
    public void TriggerFireAndForgetTaskException()
    {
        Task.Run(() => throw new Exception("Intentional fire-and-forget task exception test")).FireAndForget();
    }

    [RelayCommand]
    public async Task TriggerLogicalsRefreshAsync()
    {
        await _serversUpdater.ForceUpdateAsync();
    }

    [RelayCommand]
    public async Task LogoutUserWithClientOutdatedReasonAsync()
    {
        await _userAuthenticator.LogoutAsync(LogoutReason.ClientOutdated);
    }

    [RelayCommand]
    public void ShowOverlay()
    {
        if (SelectedOverlay != null)
        {
            MethodInfo? methodInfo = _mainWindowOverlayActivator.GetType().GetMethod(SelectedOverlay.Id);
            methodInfo?.Invoke(_mainWindowOverlayActivator, null);
        }
    }


    [RelayCommand]
    public void ShowDialog()
    {
        if (SelectedDialog is null)
        {
            return;
        }

        _windowActivators.FirstOrDefault(a => a.GetType().FullName == SelectedDialog.Id)?.Activate();
    }

    [RelayCommand]
    public void ResetInfoBanners()
    {
        _settings.IsGatewayInfoBannerDismissed = false;
        _settings.IsP2PInfoBannerDismissed = false;
        _settings.IsSecureCoreInfoBannerDismissed = false;
        _settings.IsTorInfoBannerDismissed = false;
    }

    [RelayCommand]
    public void SimulatePlanChangedToPlus()
    {
        VpnPlan oldPlan = _settings.VpnPlan;
        VpnPlan newPlan = new("VPN Plus (simulation)", "vpnplus", 1, false);

        _settings.VpnPlan = newPlan;
        _eventMessageSender.Send(new VpnPlanChangedMessage(oldPlan, newPlan));
    }

    [RelayCommand]
    public void SimulatePlanChangedToFree()
    {
        VpnPlan oldPlan = _settings.VpnPlan;
        VpnPlan newPlan = new("VPN Free (simulation)", "vpnfree", 0, false);

        _settings.VpnPlan = newPlan;
        _eventMessageSender.Send(new VpnPlanChangedMessage(oldPlan, newPlan));
    }

    [RelayCommand]
    public void SimulatePlanChanged()
    {
        VpnPlan oldPlan = _settings.VpnPlan;
        VpnPlan newPlan = SelectedVpnPlan;

        _settings.VpnPlan = newPlan;
        _eventMessageSender.Send(new VpnPlanChangedMessage(oldPlan, newPlan));
    }

    [RelayCommand]
    public void DisconnectWithSessionLimitReachedError()
    {
        _vpnServiceCaller.DisconnectAsync(new DisconnectionRequestIpcEntity()
        {
            RetryId = Guid.NewGuid(),
            ErrorType = VpnErrorTypeIpcEntity.SessionLimitReachedPlus
        });
    }

    private string GenerateOverlayDisplayName(string methodName)
    {
        string displayName = Regex.Replace(methodName, "^(Show)", "", RegexOptions.IgnoreCase);
        displayName = Regex.Replace(displayName, "(OverlayAsync)$", "", RegexOptions.IgnoreCase);

        // Insert spaces before uppercase letters, handling acronyms properly (e.g., VPN, B2B)
        displayName = Regex.Replace(displayName, "(?<=[a-z])([A-Z])", " $1");

        return displayName.Trim();
    }

    private string GenerateDialogDisplayName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return string.Empty;
        }

        int lastDotIndex = fullName.LastIndexOf('.');
        string className = fullName.Substring(lastDotIndex + 1);

        return Regex.Replace(className, "(?<!^)([A-Z])", " $1");
    }

    [RelayCommand]
    private void TriggerConnectionError()
    {
        _eventMessageSender.Send(new VpnStateIpcEntity()
        {
            Error = SelectedError
        });
    }

    [RelayCommand]
    public void ShowNpsSurvey()
    {
        _npsSurveyWindowActivator.Activate();
    }

    [RelayCommand]
    public void SetWindowPosition()
    {
        (App.Current as App)?.MainWindow?.MoveAndResize(
            new WindowPositionParameters()
            {
                XPosition = XPosition,
                YPosition = YPosition,
                Width = WindowWidth,
                Height = WindowHeight
            });
    }

    [RelayCommand]
    public void ResetWindowPosition()
    {
        (App.Current as App)?.MainWindow?.MoveAndResize(
            new WindowPositionParameters()
            {
                XPosition = null,
                YPosition = null,
                Width = DefaultSettings.WindowLocation.Width,
                Height = DefaultSettings.WindowLocation.Height,
                IsCentered = true
            });
    }

    [RelayCommand]
    public Task TriggerSettingsTelemetryHeartbeatAsync()
    {
        return _settingsHeartbeatReporter.ReportAsync();
    }

    [RelayCommand]
    public Task TriggerVpnPlanUpdateAsync()
    {
        return _vpnPlanUpdater.ForceUpdateAsync();
    }

    [RelayCommand]
    public void OverrideDeviceLocation(string countryCode)
    {
        if (string.IsNullOrEmpty(countryCode) || countryCode.Length != 2)   
        {
            return;
        }

        countryCode = countryCode.NormalizeCountryCode();

        (double Latitude, double Longitude)? coordinates = _coordinatesProvider.GetCountryCoordinates(countryCode);

        _settings.DeviceLocation = new DeviceLocation()
        {
            IpAddress = "192.168.0.1",
            CountryCode = countryCode,
            Isp = "Mock ISP",
            Latitude = coordinates?.Latitude,
            Longitude = coordinates?.Longitude
        };
    }

    [RelayCommand]
    public void ExcludeAllLocations()
    {
        List<ExcludedLocation> excludedLocations = [];
        foreach (Country country in _serversLoader.GetCountries().OrderBy(c => c.Code))
        {
            excludedLocations.Add(new(country.Code));
        }

        _settings.ExcludedLocationsList = excludedLocations;
    }

    [RelayCommand]
    public void IncludeAllLocations()
    {
        _settings.ExcludedLocationsList = DefaultSettings.ExcludedLocationsList;
    }

    [RelayCommand]
    public Task TriggerConnectionCertificateUpdateAsync()
    {
        return _connectionCertificateManager.ForceRequestNewCertificateAsync();
    }

    [RelayCommand]
    public async Task CreateAccountViaGuestHoleAsync()
    {
        try
        {
            Debug.WriteLine("BEGIN: Create Account via Guest Hole.");

            Result? result = await _guestHoleManager.ExecuteAsync<Result>(OpenCreateAccountPageAsync);

            if (result?.Success == true)
            {
                Debug.WriteLine("SUCCESS: Create account page opened successfully.");
            }
            else
            {
                Debug.WriteLine($"FAIL: Failed to open create account page. Error: {result?.Error ?? "null"}");
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine($"FAIL: Create Account via Guest Hole. Exception: {e.Message}");
        }
        finally
        {
            Debug.WriteLine("END: Create Account via Guest Hole.");
        }
    }

    private async Task<Result> OpenCreateAccountPageAsync()
    {
        try
        {
            Debug.WriteLine("BEGIN: Open Create Account URL.");

            _urlsBrowser.BrowseTo(_urlsBrowser.CreateAccount);

            Debug.WriteLine("SUCCESS: Open Create Account URL.");

            return Result.Ok();
        }
        catch (Exception e)
        {
            Debug.WriteLine("FAIL: Open Create Account URL. {0}", e.Message);

            return Result.Fail(e.Message);
        }
        finally
        { 
            Debug.WriteLine("END: Open Create Account URL.");
        }
    }

    [RelayCommand]
    public void SimulateP2PRestriction()
    {
        RestrictionListIpcEntity restrictionList = new()
        {
            Restrictions = [RestrictionIpcEntity.Bittorrenting]
        };

        _eventMessageSender.Send(restrictionList);
    }

    [RelayCommand]
    public void SimulateStreamingRestriction()
    {
        RestrictionListIpcEntity restrictionList = new()
        {
            Restrictions = [RestrictionIpcEntity.Streaming]
        };

        _eventMessageSender.Send(restrictionList);
    }
}