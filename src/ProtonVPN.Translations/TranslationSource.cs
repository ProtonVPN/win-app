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
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Huyn.PluralNet;

namespace ProtonVPN.Translations
{
    public class TranslationSource : INotifyPropertyChanged
    {
        public static TranslationSource Instance { get; } = new TranslationSource();

        public string this[string key] => Properties.Resources.ResourceManager.GetString(GetStringName(key), _currentCulture);

        private CultureInfo _currentCulture = CultureInfo.CurrentUICulture;

        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set
            {
                if (_currentCulture != value)
                {
                    _currentCulture = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static string GetStringName(string name)
        {
            var idx = name.LastIndexOf('.');
            return name.Substring(idx + 1);
        }

        public string GetPlural(string key, decimal number)
        {
            if (!Thread.CurrentThread.CurrentUICulture.Equals(_currentCulture))
            {
                Thread.CurrentThread.CurrentUICulture = _currentCulture;
                ResetPluralProvider();
            }

            return Properties.Resources.ResourceManager.GetPlural(GetStringName(key), number);
        }

        private void ResetPluralProvider()
        {
            //This should be replaced to a better solution
            var bindAttr = BindingFlags.Static | BindingFlags.NonPublic;
            var lockObj = typeof(ResourceLoaderExtension).GetField("_objLock", bindAttr);
            if (lockObj != null)
            {
                lock (lockObj)
                {
                    typeof(ResourceLoaderExtension)
                        .GetField("_pluralProvider", bindAttr)
                        ?.SetValue(null, null);
                }
            }
        }
    }

    public class LocExtension : MarkupExtension
    {
        public LocExtension(string stringName)
        {
            StringName = stringName;
        }

        public string StringName { get; }

        public IValueConverter Converter { get; set;  }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var binding = new Binding
            {
                Mode = BindingMode.OneWay,
                Path = new PropertyPath($"[{ Properties.Resources.ResourceManager.BaseName}.{StringName}]"),
                Source = TranslationSource.Instance,
                FallbackValue = StringName,
                Converter = Converter
            };

            return binding.ProvideValue(serviceProvider);
        }
    }
}
