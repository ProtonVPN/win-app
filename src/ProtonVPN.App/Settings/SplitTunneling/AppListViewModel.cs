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

using GalaSoft.MvvmLight.Command;
using ProtonVPN.Common;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Settings.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace ProtonVPN.Settings.SplitTunneling
{
    public class AppListViewModel : ViewModel, ISettingsAware, ILoggedInAware
    {
        private readonly IAppSettings _appSettings;
        private readonly InstalledApps _installedApps;

        private bool _saving;

        public AppListViewModel(IAppSettings appSettings, InstalledApps installedApps)
        {
            _appSettings = appSettings;
            _installedApps = installedApps;
        }

        public event EventHandler<BringItemIntoViewEventArgs> BringItemIntoView;

        private RelayCommand _addCommand;
        public ICommand AddCommand => _addCommand ?? (_addCommand =
            new RelayCommand(AddAction));

        private RelayCommand<SelectableItemWrapper<AppViewModel>> _removeCommand;
        public ICommand RemoveCommand => _removeCommand ?? (_removeCommand =
            new RelayCommand<SelectableItemWrapper<AppViewModel>>(RemoveAction, CanRemove));

        private ObservableCollection<SelectableItemWrapper<AppViewModel>> _items;
        public ObservableCollection<SelectableItemWrapper<AppViewModel>> Items => _items ?? (_items = 
            new ObservableCollection<SelectableItemWrapper<AppViewModel>>(WrappedItems(GetItems())));

        public void OnUserLoggedIn()
        {
            LoadItems();
            Save();
        }

        public void OnActivate()
        {
            LoadItems();
        }

        public void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (_saving) return;

            if (e.PropertyName == nameof(IAppSettings.SplitTunnelingBlockApps) ||
                e.PropertyName == nameof(IAppSettings.SplitTunnelingAllowApps))
            {
                _items = null;
                OnPropertyChanged(nameof(Items));
            }

            if (e.PropertyName == nameof(IAppSettings.SplitTunnelMode))
            {
                LoadItems();
            }
        }

        private void LoadItems()
        {
            _items = null;
            RaisePropertyChanged(nameof(Items));
        }

        private IEnumerable<SelectableItemWrapper<AppViewModel>> WrappedItems(IEnumerable<AppViewModel> items)
        {
            return items.Select(WrappedItem);
        }

        private SelectableItemWrapper<AppViewModel> WrappedItem(AppViewModel item)
        {
            return new SelectableItemWrapper<AppViewModel>
            (
                item,
                GetItemSelected,
                SetItemSelected
            );
        }

        private IEnumerable<AppViewModel> GetItems()
        {
            var suggestedItems = SuggestedItems().ToArray();
            var userItems = UserItems().ToArray();
            suggestedItems.Intersect(userItems).ForEach(i => i.Enabled = true);

            return suggestedItems
                .Union(userItems)
                .OrderBy(item => item.Name);
        }

        private IEnumerable<AppViewModel> UserItems()
        {
            switch (_appSettings.SplitTunnelMode)
            {
                case SplitTunnelMode.Block:
                    return _appSettings.SplitTunnelingBlockApps
                        .Select(a => new AppViewModel(a));
                case SplitTunnelMode.Permit:
                    return _appSettings.SplitTunnelingAllowApps
                        .Select(a => new AppViewModel(a));
                default:
                    throw new NotSupportedException();
            }
        }

        private IEnumerable<AppViewModel> SuggestedItems()
        {
            return _installedApps.All()
                .Select(a => new AppViewModel(a) { Suggested = true });
        }

        private bool GetItemSelected(AppViewModel item)
        {
            return item.Enabled;
        }

        private void SetItemSelected(AppViewModel item, bool value)
        {
            if (item.Enabled != value)
            {
                item.Enabled = value;
                Save();
            }
        }

        private void AddAction()
        {
            var app = _installedApps.SelectApp();
            if (app == null)
                return;

            AddItem(new AppViewModel(app));
        }

        private void AddItem(AppViewModel app)
        {
            if (app == null)
                return;

            var item = Items.FirstOrDefault(i => i.Item.Equals(app));

            if (item != null && !item.Item.Suggested && !item.Item.Name.Equals(app.Name))
            {
                Items.Remove(item);
                item = null;
            }

            if (item == null)
            {
                item = WrappedItem(app);
                Items.Add(item);
            }
            item.Selected = true;
            item.Item.Enabled = true;
            RaiseBringItemIntoView(item);

            Save();
        }

        private void RemoveAction(SelectableItemWrapper<AppViewModel> item)
        {
            if (!CanRemove(item))
                return;

            Items.Remove(item);
            Save();
        }

        private bool CanRemove(SelectableItemWrapper<AppViewModel> item)
        {
            return item?.Item?.Suggested == false;
        }

        private void Save()
        {
            _saving = true;
            try
            {
                switch (_appSettings.SplitTunnelMode)
                {
                    case SplitTunnelMode.Block:
                        _appSettings.SplitTunnelingBlockApps = GetApps();
                        break;
                    case SplitTunnelMode.Permit:
                        _appSettings.SplitTunnelingAllowApps = GetApps();
                        break;
                }
            }
            finally
            {
                _saving = false;
            }
        }

        private SplitTunnelingApp[] GetApps()
        {
            return Items
                .Where(i => !i.Item.Suggested || i.Item.Enabled)
                .Select(i => i.Item.DataSource()).ToArray();
        }

        private void RaiseBringItemIntoView(object item)
        {
            BringItemIntoView?.Invoke(this, new BringItemIntoViewEventArgs(item));
        }
    }
}
