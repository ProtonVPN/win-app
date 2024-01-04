/*
 * Copyright (c) 2023 Proton AG
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using ProtonVPN.Account;
using ProtonVPN.AccountPlan;
using ProtonVPN.Announcements.Contracts;
using ProtonVPN.Announcements.Installers;
using ProtonVPN.Api;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Auth;
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Api.Handlers;
using ProtonVPN.Api.Installers;
using ProtonVPN.BugReporting;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.Cli;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Installers.Extensions;
using ProtonVPN.Common.OS.Services;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Config;
using ProtonVPN.Core.FeatureFlags;
using ProtonVPN.Core.Ioc;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.NetShield;
using ProtonVPN.Core.Network;
using ProtonVPN.Core.PortForwarding;
using ProtonVPN.Core.ReportAnIssue;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.FileStoraging;
using ProtonVPN.Core.Service;
using ProtonVPN.Core.Service.Settings;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Startup;
using ProtonVPN.Core.Users;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Dns.Installers;
using ProtonVPN.EntityMapping.Installers;
using ProtonVPN.ErrorHandling;
using ProtonVPN.HumanVerification.Installers;
using ProtonVPN.IssueReporting.Installers;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Logging.Contracts.Events.AppServiceLogs;
using ProtonVPN.Logging.Contracts.Events.ProcessCommunicationLogs;
using ProtonVPN.Logging.Installers;
using ProtonVPN.Login;
using ProtonVPN.Login.ViewModels;
using ProtonVPN.Login.Views;
using ProtonVPN.Map;
using ProtonVPN.Map.ViewModels;
using ProtonVPN.Modals.ApiActions;
using ProtonVPN.Modals.Welcome;
using ProtonVPN.Notifications;
using ProtonVPN.Onboarding;
using ProtonVPN.P2PDetection;
using ProtonVPN.ProcessCommunication.App.Installers;
using ProtonVPN.ProcessCommunication.Contracts;
using ProtonVPN.ProcessCommunication.Contracts.Controllers;
using ProtonVPN.ProcessCommunication.Installers;
using ProtonVPN.QuickLaunch;
using ProtonVPN.Settings;
using ProtonVPN.Settings.Migrations;
using ProtonVPN.Sidebar;
using ProtonVPN.Sidebar.Announcements;
using ProtonVPN.Streaming;
using ProtonVPN.Translations;
using ProtonVPN.Update;
using ProtonVPN.ViewModels;
using ProtonVPN.Vpn.Connectors;
using ProtonVPN.Windows;

namespace ProtonVPN.Core
{
    public class Bootstrapper : BootstrapperBase
    {
        private IContainer _container;

        private T Resolve<T>() => _container.Resolve<T>();

        public Bootstrapper()
        {
            IssueReportingInitializer.Run();
            Initialize();
        }

        protected override void Configure()
        {
            ContainerBuilder builder = new();
            builder.RegisterLoggerConfiguration(c => c.AppLogDefaultFullFilePath)
                   .RegisterModule<CoreModule>()
                   .RegisterModule<UiModule>()
                   .RegisterModule<AppModule>()
                   .RegisterModule<BugReportingModule>()
                   .RegisterModule<LoginModule>()
                   .RegisterModule<P2PDetectionModule>()
                   .RegisterModule<ProfilesModule>()
                   .RegisterAssemblyModule<LoggingModule>()
                   .RegisterAssemblyModule<HumanVerificationModule>()
                   .RegisterAssemblyModule<ApiModule>()
                   .RegisterAssemblyModule<IssueReportingModule>()
                   .RegisterAssemblyModule<AnnouncementsModule>()
                   .RegisterAssemblyModule<DnsModule>()
                   .RegisterAssemblyModule<EntityMappingModule>()
                   .RegisterAssemblyModule<ProcessCommunicationModule>()
                   .RegisterAssemblyModule<AppProcessCommunicationModule>()
                   .RegisterAssemblyModule<StatisticalEventsModule>();

            _container = builder.Build();
        }

        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            base.OnStartup(sender, e);

            IConfiguration appConfig = Resolve<IConfiguration>();

            Resolve<ILogger>().Info<AppStartLog>($"= Booting ProtonVPN version: {appConfig.AppVersion} os: {Environment.OSVersion.VersionString} {appConfig.OsBits} bit =");

            Resolve<ILogCleaner>().Clean(appConfig.AppLogFolder, 10);

            RegisterMigrations(Resolve<AppSettingsStorage>(), Resolve<IEnumerable<IAppSettingsMigration>>());
            RegisterMigrations(Resolve<UserSettings>(), Resolve<IEnumerable<IUserSettingsMigration>>());

            LoadServersFromCache();
            Resolve<SyncedAutoStartup>().Sync();

            IncreaseAppStartCount();
            SetHardwareAcceleration();
            Resolve<IFeatureFlagsProvider>().UpdateAsync();
            RegisterEvents();
            Resolve<Language>().Initialize(e.Args);
            InitializeUpdates(e.Args);

            if (Resolve<IAppSettings>().StartMinimized == StartMinimizedMode.Disabled)
            {
                ShowInitialWindow();
            }

            Resolve<IReportAnIssueFormDataProvider>().FetchDataAsync();
            StartVpnServiceAsync();

            StartGrpcServerAsync();

            if (Resolve<IUserStorage>().GetUser().Empty() || !await IsUserValid() || await SessionExpired())
            {
                ShowLoginForm();
                return;
            }

            await Resolve<IUserAuthenticator>().InvokeAutoLoginEventAsync();
        }

        private void InitializeUpdates(string[] args)
        {
            CommandLineOption option = new("DisableAutoUpdate", args);
            if (option.Exists())
            {
                Resolve<IAppSettings>().IsToAutoUpdate = false;
            }

            Resolve<UpdateService>().Initialize();
        }

        private async Task StartGrpcServerAsync()
        {
            try
            {
                IGrpcServer grpcServer = Resolve<IGrpcServer>();
                grpcServer.CreateAndStart();
                int appServerPort = grpcServer.Port.Value;
                Resolve<ILogger>().Info<ProcessCommunicationLog>($"Sending app gRPC server port {appServerPort} to service.");
                await Resolve<VpnServiceCaller>().RegisterVpnClient(appServerPort);
            }
            catch (Exception e)
            {
                Resolve<ILogger>().Error<AppServiceStartFailedLog>("An error occurred when starting the VPN Client gRPC server.", e);
                throw;
            }
        }

        public void OnExit()
        {
            Resolve<IGrpcServer>().KillAsync();
            Resolve<ILogger>().Info<AppStopLog>("The app is exiting. Requesting services to stop.");
            Resolve<TrayIcon>().Hide();
            Resolve<IMonitoredVpnService>().StopAsync();
        }

        private async Task<bool> SessionExpired()
        {
            if (string.IsNullOrEmpty(Resolve<IAppSettings>().AccessToken))
            {
                return true;
            }

            try
            {
                ApiResponseResult<VpnInfoWrapperResponse> result = await Resolve<IUserAuthenticator>().RefreshVpnInfoAsync();
                return result.Failure;
            }
            catch
            {
                return false;
            }
        }

        private void IncreaseAppStartCount()
        {
            Resolve<IAppSettings>().AppStartCounter++;
        }

        private void SetHardwareAcceleration()
        {
            HardwareAccelerationManager.Set(Resolve<IAppSettings>().HardwareAccelerationEnabled);
        }

        private void LoadServersFromCache()
        {
            IReadOnlyCollection<LogicalServerResponse> servers = (IReadOnlyCollection<LogicalServerResponse>)
                Resolve<IServersFileStorage>().Get();
            if (servers != null && servers.Any())
            {
                Resolve<ServerManager>().Load(servers);
            }
        }

        private async Task<bool> IsUserValid()
        {
            LoginViewModel loginViewModel = Resolve<LoginViewModel>();
            try
            {
                AuthResult result = await Resolve<UserValidator>().GetValidateResult();
                if (result.Failure)
                {
                    await loginViewModel.HandleAuthFailureAsync(result);
                    ShowLoginForm();
                    return false;
                }
            }
            catch (Exception ex)
            {
                await loginViewModel.HandleAuthFailureAsync(AuthResult.Fail(ex.Message));
                ShowLoginForm();
                return false;
            }

            return true;
        }

        private async Task StartVpnServiceAsync()
        {
            await StartService(Resolve<VpnSystemService>());
            await InitializeStateFromServiceAsync();
        }

        private void ShowInitialWindow()
        {
            LoginWindow loginWindow = Resolve<LoginWindow>();
            LoginWindowViewModel loginWindowViewModel = Resolve<LoginWindowViewModel>();
            Application.Current.MainWindow = loginWindow;
            loginWindowViewModel.CurrentPageViewModel = Resolve<LoadingViewModel>();
            loginWindow.DataContext = loginWindowViewModel;
            loginWindow.Show();
        }

        private void RegisterEvents()
        {
            IVpnServiceManager vpnServiceManager = Resolve<IVpnServiceManager>();
            IUserAuthenticator userAuthenticator = Resolve<IUserAuthenticator>();
            AppWindow appWindow = Resolve<AppWindow>();
            IAppSettings appSettings = Resolve<IAppSettings>();
            Resolve<ISettingsServiceClientManager>();

            Resolve<IFeatureFlagsProvider>().FeatureFlagsUpdated += (_, _) =>
            {
                IEnumerable<IFeatureFlagsAware> instances = Resolve<IEnumerable<IFeatureFlagsAware>>();
                foreach (IFeatureFlagsAware instance in instances)
                {
                    instance.OnFeatureFlagsChanged();
                }
            };

            Resolve<IServerUpdater>().ServersUpdated += (sender, e) =>
            {
                IEnumerable<IServersAware> instances = Resolve<IEnumerable<IServersAware>>();
                foreach (IServersAware instance in instances)
                {
                    instance.OnServersUpdated();
                }
            };

            Resolve<IUserLocationService>().UserLocationChanged += (_, location) =>
            {
                IEnumerable<IUserLocationAware> instances = Resolve<IEnumerable<IUserLocationAware>>();
                foreach (IUserLocationAware instance in instances)
                {
                    instance.OnUserLocationChanged(location);
                }
            };

            Resolve<IAnnouncementService>().AnnouncementsChanged += (_, _) =>
            {
                IEnumerable<IAnnouncementsAware> instances = Resolve<IEnumerable<IAnnouncementsAware>>();
                foreach (IAnnouncementsAware instance in instances)
                {
                    instance.OnAnnouncementsChanged();
                }
            };

            userAuthenticator.UserLoggingIn += (_, _) => OnUserLoggingIn();

            userAuthenticator.UserLoggedIn += async (_, e) =>
            {
                await Resolve<IUserLocationService>().Update();
                await Resolve<IServerUpdater>().Update();
                await Resolve<IClientConfig>().Update();
                await Resolve<StreamingServicesUpdater>().Update();

                GuestHoleState guestHoleState = Resolve<GuestHoleState>();
                if (guestHoleState.Active)
                {
                    await Resolve<IVpnServiceManager>().Disconnect(VpnError.NoneKeepEnabledKillSwitch);
                    guestHoleState.SetState(false);
                }

                IEnumerable<ILoggedInAware> instances = Resolve<IEnumerable<ILoggedInAware>>();
                foreach (ILoggedInAware instance in instances)
                {
                    instance.OnUserLoggedIn();
                }

                await SwitchToAppWindow(e.IsAutoLogin);
            };

            userAuthenticator.UserLoggedOut += (_, _) =>
            {
                Resolve<IModals>().CloseAll();

                IEnumerable<ILogoutAware> instances = Resolve<IEnumerable<ILogoutAware>>();
                foreach (ILogoutAware instance in instances)
                {
                    instance.OnUserLoggedOut();
                }

                ShowLoginForm();
                Resolve<AppWindow>().Hide();
            };

            Resolve<IUserStorage>().UserDataChanged += (_, _) =>
            {
                IEnumerable<IUserDataAware> instances = Resolve<IEnumerable<IUserDataAware>>();
                foreach (IUserDataAware instance in instances)
                {
                    instance.OnUserDataChanged();
                }
            };

            Resolve<IUserStorage>().VpnPlanChanged += async (_, e) =>
            {
                IEnumerable<IVpnPlanAware> instances = Resolve<IEnumerable<IVpnPlanAware>>();
                foreach (IVpnPlanAware instance in instances)
                {
                    await instance.OnVpnPlanChangedAsync(e);
                }
            };

            Resolve<PinFactory>().PinsChanged += (_, _) =>
            {
                IEnumerable<IPinChangeAware> instances = Resolve<IEnumerable<IPinChangeAware>>();
                foreach (IPinChangeAware instance in instances)
                {
                    instance.OnPinsChanged();
                }
            };

            vpnServiceManager.RegisterVpnStateCallback(async (e) =>
            {
                Resolve<IVpnManager>().OnVpnStateChanged(e);
                await Resolve<LoginViewModel>().OnVpnStateChanged(e);
                await Resolve<ReportBugModalViewModel>().OnVpnStateChanged(e);
                await Resolve<GuestHoleConnector>().OnVpnStateChanged(e);
                await Resolve<AlternativeHostHandler>().OnVpnStateChanged(e);
            });

            vpnServiceManager.RegisterPortForwardingStateCallback((state) =>
            {
                IEnumerable<IPortForwardingStateAware> instances = Resolve<IEnumerable<IPortForwardingStateAware>>();
                foreach (IPortForwardingStateAware instance in instances)
                {
                    instance.OnPortForwardingStateChanged(state);
                }
            });

            vpnServiceManager.RegisterConnectionDetailsChangeCallback(ipAddressInfo =>
            {
                IEnumerable<IConnectionDetailsAware> instances = Resolve<IEnumerable<IConnectionDetailsAware>>();
                foreach (IConnectionDetailsAware instance in instances)
                {
                    instance.OnConnectionDetailsChanged(ipAddressInfo);
                }
            });

            vpnServiceManager.RegisterNetShieldStatisticChangeCallback(stats =>
            {
                IEnumerable<INetShieldStatisticAware> instances = Resolve<IEnumerable<INetShieldStatisticAware>>();
                foreach (INetShieldStatisticAware instance in instances)
                {
                    instance.OnNetShieldStatisticChanged(stats);
                }
            });

            Resolve<IAppController>().OnOpenWindowInvoked += (_, _) =>
            {
                IEnumerable<IOpenMainWindowAware> instances = Resolve<IEnumerable<IOpenMainWindowAware>>();
                foreach (IOpenMainWindowAware instance in instances)
                {
                    instance.OnOpenMainWindow();
                }
            };

            Resolve<IVpnManager>().VpnStateChanged += (_, e) =>
            {
                IEnumerable<IVpnStateAware> instances = Resolve<IEnumerable<IVpnStateAware>>();
                foreach (IVpnStateAware instance in instances)
                {
                    instance.OnVpnStateChanged(e);
                }

                Resolve<IEventAggregator>().PublishOnCurrentThreadAsync(e);
            };

            Resolve<UpdateService>().UpdateStateChanged += (_, e) =>
            {
                IEnumerable<IUpdateStateAware> instances = Resolve<IEnumerable<IUpdateStateAware>>();
                foreach (IUpdateStateAware instance in instances)
                {
                    instance.OnUpdateStateChanged(e);
                }
            };

            Resolve<SidebarManager>().ManualSidebarModeChangeRequested += appWindow.OnManualSidebarModeChangeRequested;

            appSettings.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(IAppSettings.Language))
                {
                    TranslationSource.Instance.CurrentCulture = new CultureInfo(appSettings.Language);
                }

                IEnumerable<ISettingsAware> instances = Resolve<IEnumerable<ISettingsAware>>();
                foreach (ISettingsAware instance in instances)
                {
                    instance.OnAppSettingsChanged(e);
                }
            };

            Resolve<Onboarding.Onboarding>().StepChanged += (_, e) =>
            {
                IEnumerable<IOnboardingStepAware> instances = Resolve<IEnumerable<IOnboardingStepAware>>();
                foreach (IOnboardingStepAware instance in instances)
                {
                    instance.OnStepChanged(e);
                }
            };

            Resolve<GuestHoleState>().GuestHoleStateChanged += (_, active) =>
            {
                IEnumerable<IGuestHoleStateAware> instances = Resolve<IEnumerable<IGuestHoleStateAware>>();
                foreach (IGuestHoleStateAware instance in instances)
                {
                    instance.OnGuestHoleStateChanged(active);
                }
            };

            Resolve<ITokenClient>().RefreshTokenExpired += (_, _) =>
            {
                Resolve<ExpiredSessionHandler>().Execute();
            };

            Resolve<OutdatedAppNotification>();
            Resolve<IModals>();
            Resolve<InsecureNetworkNotification>();
            Resolve<ActionableFailureApiResultEventHandler>();
            Resolve<IAuthCertificateUpdater>();
            Resolve<VpnAuthCertificateUpdater>();
        }

        private void OnUserLoggingIn()
        {
            Resolve<LoginWindowViewModel>().CurrentPageViewModel = Resolve<LoadingViewModel>();
        }

        private void ShowLoginForm()
        {
            SwitchToLoginWindow();
            CheckAuthenticationServerStatus();
        }

        private void SwitchToLoginWindow()
        {
            LoginWindowViewModel loginWindowViewModel = Resolve<LoginWindowViewModel>();
            LoginWindow loginWindow = Resolve<LoginWindow>();
            loginWindowViewModel.CurrentPageViewModel = Resolve<LoginViewModel>();
            loginWindow.DataContext = loginWindowViewModel;
            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();
            loginWindow.Activate();
        }

        private void CheckAuthenticationServerStatus()
        {
            try
            {
                Resolve<IApiClient>().CheckAuthenticationServerStatusAsync().IgnoreExceptions();
            }
            catch (Exception e)
            {
                Resolve<ILogger>().Error<AppLog>("The app failed to check auth server status.", e);
            }
        }

        private async Task SwitchToAppWindow(bool autoLogin)
        {
            if (!Resolve<IUserAuthenticator>().IsLoggedIn)
            {
                return;
            }

            if (Resolve<IAppSettings>().StartMinimized != StartMinimizedMode.Disabled)
            {
                StartVpnServiceAsync();
            }

            await Resolve<ISettingsServiceClientManager>().UpdateServiceSettings();

            Resolve<PinFactory>().BuildPins();
            LoadViewModels();
            Resolve<IP2PDetector>();
            Resolve<IVpnInfoUpdater>();
            Resolve<IUserSettingsUpdater>();

            AppWindow appWindow = Resolve<AppWindow>();
            appWindow.DataContext = Resolve<MainViewModel>();
            Application.Current.MainWindow = appWindow;
            if (!autoLogin || Resolve<IAppSettings>().StartMinimized != StartMinimizedMode.ToSystray)
            {
                appWindow.AllowWindowHiding = autoLogin;
                appWindow.Show();
            }

            Resolve<LoginWindow>().Hide();

            Resolve<PlanChangeHandler>();
            await Resolve<IAnnouncementService>().UpdateAsync();
            Resolve<WelcomeModalManager>().Load();
            await Resolve<SystemTimeValidator>().Validate();
            await Resolve<AutoConnect>().LoadAsync(autoLogin);
            Resolve<INetworkClient>().CheckForInsecureWiFi();
        }

        private void LoadViewModels()
        {
            Resolve<CountriesViewModel>().Load();
            Resolve<QuickLaunchViewModel>().Load();
            Resolve<MapViewModel>().Load();
            Resolve<SidebarProfilesViewModel>().Load();
            Resolve<ConnectionStatusViewModel>().Load();
        }

        private async Task InitializeStateFromServiceAsync()
        {
            try
            {
                await Resolve<IVpnManager>().GetStateAsync();
            }
            catch (Exception ex) when (ex is CommunicationException || ex is TimeoutException || ex is TaskCanceledException)
            {
                Resolve<ILogger>().Error<AppServiceLog>("Failed to get initial state from VPN service.", ex);
            }
        }

        private async Task StartService(IConcurrentService service)
        {
            Result result = await service.StartAsync();
            if (result.Failure && result.Exception != null)
            {
                Resolve<ILogger>().Error<AppServiceStartFailedLog>($"Failed to start {service.Name} service.", result.Exception);
                FatalErrorHandler fatalErrorHandler = new();
                fatalErrorHandler.Exit("Failed to start ProtonVPN Service.");
            }
        }

        private void RegisterMigrations(ISupportsMigration subject, IEnumerable<IMigration> migrations)
        {
            foreach (IMigration migration in migrations)
            {
                subject.RegisterMigration(migration);
            }
        }
    }
}