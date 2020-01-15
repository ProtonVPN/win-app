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

using GalaSoft.MvvmLight.Command;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Core.Language;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Settings;
using ProtonVPN.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows.Input;
using ProtonVPN.Core.Settings.Contracts;

namespace ProtonVPN.Settings
{
    public abstract class IpListViewModel : ViewModel, INotifyDataErrorInfo, ISettingsAware, ILanguageAware
    {
        private bool _saving;

        public event EventHandler<BringItemIntoViewEventArgs> BringItemIntoView;

        private RelayCommand _addCommand;
        public ICommand AddCommand => _addCommand ?? (_addCommand =
            new RelayCommand(AddAction));

        private RelayCommand<SelectableItemWrapper<IpViewModel>> _removeCommand;
        public ICommand RemoveCommand => _removeCommand ?? (_removeCommand =
            new RelayCommand<SelectableItemWrapper<IpViewModel>>(RemoveAction));

        private ObservableCollection<SelectableItemWrapper<IpViewModel>> _items;
        public ObservableCollection<SelectableItemWrapper<IpViewModel>> Items => _items ?? (_items = 
            new ObservableCollection<SelectableItemWrapper<IpViewModel>>(WrappedItems(GetItemsInner())));

        private string _ip = "";
        public string Ip
        {
            get => _ip;
            set
            {
                Set(ref _ip, value);
                _addCommand?.RaiseCanExecuteChanged();
                SetError(nameof(Ip), null);
            }
        }

        public void OnActivate()
        {
            Clear();
        }

        public void OnAppSettingsChanged(PropertyChangedEventArgs e)
        {
            if (_saving) return;

            if (e.PropertyName == GetSettingsPropertyName())
            {
                _items = null;
                RaisePropertyChanged(nameof(Items));
            }
        }

        public void OnLanguageChanged(string lang)
        {
            Validate();
        }

        protected abstract IpContract[] GetItems();

        protected abstract string GetSettingsPropertyName();

        private void Clear()
        {
            ClearErrors();
            Ip = "";
            _items = null;
            RaisePropertyChanged(nameof(Items));
        }

        private IEnumerable<SelectableItemWrapper<IpViewModel>> WrappedItems(IEnumerable<IpViewModel> items)
        {
            return items.Select(WrappedItem);
        }

        private SelectableItemWrapper<IpViewModel> WrappedItem(IpViewModel item)
        {
            return new SelectableItemWrapper<IpViewModel>
            (
                item,
                GetItemSelected,
                SetItemSelected
            );
        }

        private IEnumerable<IpViewModel> GetItemsInner()
        {
            return GetItems().Select(a => new IpViewModel(a)).OrderBy(item => item.Ip);
        }

        private bool GetItemSelected(IpViewModel item)
        {
            return item.Enabled;
        }

        private void SetItemSelected(IpViewModel item, bool value)
        {
            if (item.Enabled != value)
            {
                item.Enabled = value;
                Save();
            }
        }

        private void AddAction()
        {
            Validate();
            if (HasErrors)
                return;

            AddIp();
        }

        private void AddIp()
        {
            if (string.IsNullOrEmpty(Ip))
                return;

            var newIp = new IpViewModel(Ip);
            var item = Items.FirstOrDefault(i => i.Item.Equals(newIp));
            if (item == null)
            {
                item = WrappedItem(newIp);
                Items.Add(item);
            }
            item.Item.Enabled = true;
            item.Selected = true;
            RaiseBringItemIntoView(item);

            Save();
            Ip = "";
        }

        private void RemoveAction(SelectableItemWrapper<IpViewModel> item)
        {
            if (item == null)
                return;

            Items.Remove(item);
            Save();
        }

        private void Save()
        {
            _saving = true;
            try
            {
                SaveData(Items.Select(i => i.Item.DataSource()).ToArray());
            }
            finally
            {
                _saving = false;
            }
        }

        protected abstract void SaveData(IpContract[] ips);

        private void Validate()
        {
            var error = IpError();
            SetError(nameof(Ip), error);
        }

        private string IpError()
        {
            if (string.IsNullOrEmpty(Ip))
                return string.Empty;

            Ip = Ip.Trim();

            if (string.IsNullOrEmpty(Ip))
                return string.Empty;

            if (IPAddress.TryParse(Ip, out var parsedIp)
                && Ip.EqualsIgnoringCase(parsedIp.ToString()))
            {
                return string.Empty;
            }

            return StringResources.Get("Settings_Advanced_SplitTunnel_msg_IpInvalid");
        }

        private void RaiseBringItemIntoView(object item)
        {
            BringItemIntoView?.Invoke(this, new BringItemIntoViewEventArgs(item));
        }

        #region INotifyDataErrorInfo

        private readonly Dictionary<string, string> _errors = new Dictionary<string, string>();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public bool HasErrors => _errors.Any();

        public IEnumerable GetErrors(string propertyName)
        {
            if (_errors.TryGetValue(propertyName, out var value))
                return new[] { value };

            return null;
        }

        private void SetError(string propertyName, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                if (_errors.TryGetValue(propertyName, out var oldValue) && value == oldValue)
                    return;

                _errors[propertyName] = value;
                RaiseErrorsChanged(propertyName);
            }
            else
            {
                if (_errors.Remove(propertyName))
                    RaiseErrorsChanged(propertyName);
            }
        }

        private void ClearErrors()
        {
            var propertyNames = _errors.Keys.ToArray();
            _errors.Clear();
            foreach (var propertyName in propertyNames)
            {
                RaiseErrorsChanged(propertyName);
            }
        }

        private void RaiseErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        #endregion
    }
}
