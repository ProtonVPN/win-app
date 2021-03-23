/*
 * Copyright (c) 2020 Proton Technologies AG
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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using ProtonVPN.Account;
using ProtonVPN.BugReporting;
using ProtonVPN.Common.Abstract;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Common.OS.Services;
using ProtonVPN.Common.Storage;
using ProtonVPN.Common.Vpn;
using ProtonVPN.Core.Abstract;
using ProtonVPN.Core.Announcements;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Api.Handlers;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Config;
using ProtonVPN.Core.Events;
using ProtonVPN.Core.Ioc;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Network;
using ProtonVPN.Core.OS.Net;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Service;
using ProtonVPN.Core.Service.Settings;
using ProtonVPN.Core.Service.Vpn;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Startup;
using ProtonVPN.Core.Update;
using ProtonVPN.Core.User;
using ProtonVPN.Core.Vpn;
using ProtonVPN.Login;
using ProtonVPN.Login.ViewModels;
using ProtonVPN.Login.Views;
using ProtonVPN.Map;
using ProtonVPN.Map.ViewModels;
using ProtonVPN.Notifications;
using ProtonVPN.Onboarding;
using ProtonVPN.P2PDetection;
using ProtonVPN.PlanDowngrading;
using ProtonVPN.QuickLaunch;
using ProtonVPN.Settings;
using ProtonVPN.Settings.Migrations;
using ProtonVPN.Sidebar;
using ProtonVPN.Sidebar.Announcements;
using ProtonVPN.Translations;
using ProtonVPN.Trial;
using ProtonVPN.ViewModels;
using ProtonVPN.Vpn.Connectors;
using ProtonVPN.Windows;
using Sentry;
using Sentry.Protocol;
using AppConfig = ProtonVPN.Common.Configuration.Config;

namespace ProtonVPN.Core
{
    internal class Bootstrapper : BootstrapperBase
    {
        private IContainer _container;

        private T Resolve<T>() => _container.Resolve<T>();

        private readonly string[] _args;

        public Bootstrapper(string[] args)
        {
            _args = args;
        }

        protected override void Configure()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterModule<CoreModule>()
                .RegisterModule<UiModule>()
                .RegisterModule<AppModule>()
                .RegisterModule<BugReportingModule>()
                .RegisterModule<LoginModule>()
                .RegisterModule<P2PDetectionModule>()
                .RegisterModule<ProfilesModule>()
                .RegisterModule<TrialModule>();

            _container = builder.Build();
        }

        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            base.OnStartup(sender, e);

            UnhandledExceptionLogging logging = Resolve<UnhandledExceptionLogging>();
            logging.CaptureUnhandledExceptions();
            logging.CaptureTaskExceptions();

            Resolve<ServicePointConfiguration>().Apply();

            AppConfig appConfig = Resolve<AppConfig>();

            Resolve<ILogger>().Info($"= Booting ProtonVPN version: {appConfig.AppVersion} os: {Environment.OSVersion.VersionString} {appConfig.OsBits} bit =");
            Resolve<LogCleaner>().Clean(appConfig.AppLogFolder, 30);
            LoadServersFromCache();

            RegisterMigrations(Resolve<AppSettingsStorage>(), Resolve<IEnumerable<IAppSettingsMigration>>());
            RegisterMigrations(Resolve<UserSettings>(), Resolve<IEnumerable<IUserSettingsMigration>>());
            Resolve<SyncedAutoStartup>().Sync();

            IncreaseAppStartCount();
            RegisterEvents();
            Resolve<Language>().Initialize(_args);
            await ShowInitialWindow();

            if (Resolve<IUserStorage>().User().Empty() || !await IsUserValid() || await SessionExpired())
            {
                ShowLoginForm();
                return;
            }

            Resolve<UserAuth>().InvokeAutoLoginEvent();
        }

        public void OnExit()
        {
            Resolve<TrayIcon>().Hide();
            Resolve<VpnSystemService>().StopAsync();
            Resolve<AppUpdateSystemService>().StopAsync();
        }

        private async Task StartVpnService()
        {
            if (!Resolve<BaseFilteringEngineService>().Running())
            {
                return;
            }

            MonitoredVpnService service = Resolve<MonitoredVpnService>();
            if (!service.Enabled())
            {
                return;
            }

            await StartService(service);
        }

        private async Task<bool> SessionExpired()
        {
            if (string.IsNullOrEmpty(Resolve<ITokenStorage>().AccessToken))
            {
                return true;
            }

            try
            {
                ApiResponseResult<VpnInfoResponse> result = await Resolve<UserAuth>().RefreshVpnInfo();
                return result.Failure;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        private void IncreaseAppStartCount()
        {
            Resolve<IAppSettings>().AppStartCounter++;
        }

        private void LoadServersFromCache()
        {
            IReadOnlyCollection<LogicalServerContract> servers = Resolve<ICollectionStorage<LogicalServerContract>>().GetAll();
            if (servers.Any())
                Resolve<ServerManager>().Load(servers);
        }

        private async Task<bool> IsUserValid()
        {
            try
            {
                ApiResponseResult<BaseResponse> validateResult = await Resolve<UserValidator>().GetValidateResult();
                if (validateResult.Failure)
                {
                    Resolve<LoginErrorViewModel>().SetError(validateResult.Error);
                    ShowLoginForm();
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                Resolve<LoginErrorViewModel>().SetError(ex.Message);
                ShowLoginForm();
                return false;
            }

            return true;
        }

        private async Task StartAllServices()
        {
            await StartVpnService();
            await StartService(Resolve<AppUpdateSystemService>());
            await InitializeStateFromService();
        }

        private async Task ShowInitialWindow()
        {
            if (Resolve<IAppSettings>().StartMinimized != StartMinimizedMode.Disabled)
            {
                return;
            }

            LoginWindow loginWindow = Resolve<LoginWindow>();
            LoginWindowViewModel loginWindowViewModel = Resolve<LoginWindowViewModel>();
            Application.Current.MainWindow = loginWindow;
            loginWindowViewModel.CurrentPageViewModel = Resolve<LoadingViewModel>();
            loginWindow.DataContext = loginWindowViewModel;
            loginWindow.Show();

            await StartAllServices();
        }

        private void RegisterEvents()
        {
            IVpnServiceManager vpnServiceManager = Resolve<IVpnServiceManager>();
            UserAuth userAuth = Resolve<UserAuth>();
            AppWindow appWindow = Resolve<AppWindow>();
            IAppSettings appSettings = Resolve<IAppSettings>();
            Resolve<ISettingsServiceClientManager>();

            Resolve<IServerUpdater>().ServersUpdated += (sender, e) =>
            {
                IEnumerable<IServersAware> instances = Resolve<IEnumerable<IServersAware>>();
                foreach (IServersAware instance in instances)
                {
                    instance.OnServersUpdated();
                }
            };

            Resolve<IUserLocationService>().UserLocationChanged += (sender, location) =>
            {
                IEnumerable<IUserLocationAware> instances = Resolve<IEnumerable<IUserLocationAware>>();
                foreach (IUserLocationAware instance in instances)
                {
                    instance.OnUserLocationChanged(location);
                }
            };

            Resolve<IAnnouncements>().AnnouncementsChanged += (sender, e) =>
            {
                IEnumerable<IAnnouncementsAware> instances = Resolve<IEnumerable<IAnnouncementsAware>>();
                foreach (IAnnouncementsAware instance in instances)
                {
                    instance.OnAnnouncementsChanged();
                }
            };

            userAuth.UserLoggingIn += (sender, e) => OnUserLoggingIn();

            userAuth.UserLoggedIn += async (sender, e) =>
            {
                GuestHoleState guestHoleState = Resolve<GuestHoleState>();
                await Resolve<IServerUpdater>().Update();
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

                await SwitchToAppWindow(e.AutoLogin);
            };

            userAuth.UserLoggedOut += (sender, e) =>
            {
                Resolve<IModals>().CloseAll();
                SwitchToLoginWindow();
                Resolve<AppWindow>().Hide();
                IEnumerable<ILogoutAware> instances = Resolve<IEnumerable<ILogoutAware>>();
                foreach (ILogoutAware instance in instances)
                {
                    instance.OnUserLoggedOut();
                }
            };

            Resolve<IUserStorage>().UserDataChanged += (sender, e) =>
            {
                IEnumerable<IUserDataAware> instances = Resolve<IEnumerable<IUserDataAware>>();
                foreach (IUserDataAware instance in instances)
                {
                    instance.OnUserDataChanged();
                }
            };

            Resolve<IUserStorage>().VpnPlanChanged += async (sender, e) =>
            {
                IEnumerable<IVpnPlanAware> instances = Resolve<IEnumerable<IVpnPlanAware>>();
                foreach (IVpnPlanAware instance in instances)
                {
                    await instance.OnVpnPlanChangedAsync(e);
                }
            };

            Resolve<SyncProfiles>().SyncStatusChanged += (sender, e) =>
            {
                IEnumerable<IProfileSyncStatusAware> instances = Resolve<IEnumerable<IProfileSyncStatusAware>>();
                foreach (IProfileSyncStatusAware instance in instances)
                {
                    instance.OnProfileSyncStatusChanged(e.Status, e.ErrorMessage, e.ChangesSyncedAt);
                }
            };

            Resolve<PinFactory>().PinsChanged += (sender, e) =>
            {
                IEnumerable<IPinChangeAware> instances = Resolve<IEnumerable<IPinChangeAware>>();
                foreach (IPinChangeAware instance in instances)
                {
                    instance.OnPinsChanged();
                }
            };

            vpnServiceManager.RegisterCallback(async(e) =>
            {
                Resolve<IVpnManager>().OnVpnStateChanged(e);
                await Resolve<LoginViewModel>().OnVpnStateChanged(e);
                await Resolve<GuestHoleConnector>().OnVpnStateChanged(e);
            });

            Resolve<IVpnManager>().VpnStateChanged += (sender, e) =>
            {
                IEnumerable<IVpnStateAware> instances = Resolve<IEnumerable<IVpnStateAware>>();
                foreach (IVpnStateAware instance in instances)
                {
                    instance.OnVpnStateChanged(e);
                }

                Resolve<IEventAggregator>().PublishOnCurrentThread(e);
            };

            Resolve<UpdateService>().UpdateStateChanged += (sender, e) =>
            {
                IEnumerable<IUpdateStateAware> instances = Resolve<IEnumerable<IUpdateStateAware>>();
                foreach (IUpdateStateAware instance in instances)
                {
                    instance.OnUpdateStateChanged(e);
                }
            };

            Resolve<P2PDetector>().TrafficForwarded += (sender, ip) =>
            {
                IEnumerable<ITrafficForwardedAware> instances = Resolve<IEnumerable<ITrafficForwardedAware>>();
                foreach (ITrafficForwardedAware instance in instances)
                {
                    instance.OnTrafficForwarded(ip);
                }
            };

            Resolve<SidebarManager>().ManualSidebarModeChangeRequested += appWindow.OnManualSidebarModeChangeRequested;

            appSettings.PropertyChanged += (sender, e) =>
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

            Resolve<Onboarding.Onboarding>().StepChanged += (sender, e) =>
            {
                IEnumerable<IOnboardingStepAware> instances = Resolve<IEnumerable<IOnboardingStepAware>>();
                foreach (IOnboardingStepAware instance in instances)
                {
                    instance.OnStepChanged(e);
                }
            };

            Resolve<TrialTimer>().TrialTimerTicked += (sender, e) =>
            {
                IEnumerable<ITrialDurationAware> instances = Resolve<IEnumerable<ITrialDurationAware>>();
                foreach (ITrialDurationAware instance in instances)
                {
                    instance.OnTrialSecondElapsed(e);
                }
            };

            Resolve<Trial.Trial>().StateChanged += async (sender, e) =>
            {
                IEnumerable<ITrialStateAware> instances = Resolve<IEnumerable<ITrialStateAware>>();
                foreach (ITrialStateAware instance in instances)
                {
                    await instance.OnTrialStateChangedAsync(e);
                }
            };

            Resolve<GuestHoleState>().GuestHoleStateChanged += (sender, active) =>
            {
                IEnumerable<IGuestHoleStateAware> instances = Resolve<IEnumerable<IGuestHoleStateAware>>();
                foreach (IGuestHoleStateAware instance in instances)
                {
                    instance.OnGuestHoleStateChanged(active);
                }
            };

            Resolve<EventClient>().ApiDataChanged += async (sender, e) =>
            {
                IEnumerable<IApiDataChangeAware> instances = Resolve<IEnumerable<IApiDataChangeAware>>();
                foreach (IApiDataChangeAware instance in instances)
                {
                    await instance.OnApiDataChanged(e);
                }
            };

            Resolve<UnauthorizedResponseHandler>().SessionExpired += (sender, e) =>
            {
                Resolve<ExpiredSessionHandler>().Execute();
            };

            Resolve<OutdatedAppHandler>().AppOutdated += Resolve<OutdatedAppNotification>().OnAppOutdated;
            Resolve<IModals>();
            Resolve<InsecureNetworkNotification>();
        }

        private void OnUserLoggingIn()
        {
            Resolve<LoginWindowViewModel>().CurrentPageViewModel = Resolve<LoadingViewModel>();
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

        private void ShowLoginForm()
        {
            SwitchToLoginWindow();
        }

        private async Task SwitchToAppWindow(bool autoLogin)
        {
            if (!Resolve<UserAuth>().LoggedIn)
            {
                return;
            }

            if (Resolve<IAppSettings>().StartMinimized != StartMinimizedMode.Disabled)
            {
                await StartAllServices();
            }

            await Resolve<ISettingsServiceClientManager>().UpdateServiceSettings();

            Resolve<PinFactory>().BuildPins();

            LoadViewModels();
            Resolve<P2PDetector>();
            Resolve<VpnInfoChecker>();

            AppWindow appWindow = Resolve<AppWindow>();
            appWindow.DataContext = Resolve<MainViewModel>();
            Application.Current.MainWindow = appWindow;
            if (Resolve<IAppSettings>().StartMinimized != StartMinimizedMode.ToSystray)
            {
                appWindow.Show();
            }

            Resolve<LoginWindow>().Hide();

            Resolve<PlanDowngradeHandler>();
            await Resolve<Trial.Trial>().Load();
            await Resolve<IUserLocationService>().Update();
            await Resolve<IClientConfig>().Update();
            await Resolve<IAnnouncements>().Update();
            await Resolve<AutoConnect>().Load(autoLogin);
            Resolve<SyncProfiles>().Sync();
            Resolve<INetworkClient>().CheckForInsecureWiFi();
            await Resolve<EventClient>().StoreLatestEvent();
            Resolve<EventTimer>().Start();
        }

        private void LoadViewModels()
        {
            Resolve<MainViewModel>().Load();
            Resolve<CountriesViewModel>().Load();
            Resolve<QuickLaunchViewModel>().Load();
            Resolve<MapViewModel>().Load();
            Resolve<SidebarProfilesViewModel>().Load();
            Resolve<ConnectionStatusViewModel>().Load();
        }

        private async Task InitializeStateFromService()
        {
            try
            {
                await Resolve<IVpnManager>().GetState();
            }
            catch (Exception ex) when (ex is CommunicationException || ex is TimeoutException || ex is TaskCanceledException)
            {
                Resolve<ILogger>().Error(ex.CombinedMessage());
            }
        }

        private async Task StartService(IConcurrentService service)
        {
            Result result = await service.StartAsync();

            if (result.Failure)
            {
                ReportException(result.Exception);

                AppConfig config = Resolve<AppConfig>();
                string filename = config.ErrorMessageExePath;
                string error = GetServiceErrorMessage(service.Name, result.Exception);
                try
                {
                    Resolve<IOsProcesses>().Process(filename, error).Start();
                }
                catch (Exception e)
                {
                    string serviceName = Path.GetFileNameWithoutExtension(filename);
                    Resolve<ILogger>().Error($"Failed to start {serviceName} process: {e.CombinedMessage()}");
                    ReportException(e);
                }
            }
        }

        private void ReportException(Exception e)
        {
            SentrySdk.WithScope(scope =>
            {
                scope.Level = SentryLevel.Error;
                scope.SetTag("captured_in", "App_Bootstrapper_StartService");
                SentrySdk.CaptureException(e);
            });
        }

        private string GetServiceErrorMessage(string serviceName, Exception e)
        {
            string error = e.InnerException?.Message ?? e.Message;
            string failedToStart = string.Format(Translation.Get("Dialogs_ServiceStart_msg_FailedToStart"), serviceName);

            return $"\"{failedToStart}\" \"{error}\"";
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
