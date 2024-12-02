/*
 * Copyright (c) 2024 Proton AG
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

using System.Collections.Specialized;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Helpers;
using ProtonVPN.Client.Core.Models.ReportIssue;

namespace ProtonVPN.Client.UI.Main.Components;

public sealed partial class TitleBarMenuComponent : IContextAware
{
    public TitleBarMenuViewModel ViewModel { get; }

    public TitleBarMenuComponent()
    {
        ViewModel = App.GetService<TitleBarMenuViewModel>();
        InitializeComponent();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public object GetContext()
    {
        return ViewModel;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.ReportIssueCategories.CollectionChanged += OnReportIssueCategoriesChanged;
        ViewModel.Activate();
    }

    private void OnReportIssueCategoriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        HelpMenu.Items.Clear();

        foreach (IssueCategory category in ViewModel.ReportIssueCategories)
        {
            bool isPlaceholder = category.Key == TitleBarMenuViewModel.REPORT_ISSUE_CATEGORY_PLACEHOLDER;

            HelpMenu.Items.Add(new MenuFlyoutItem()
            {
                Text = category.Name,
                Icon = category.Icon,
                IsEnabled = !isPlaceholder,
                Style = ResourceHelper.GetFlyoutStyle("TitleBarMenuFlyoutItemStyle"),
                Command = isPlaceholder ? null : ViewModel.HandleCategoryClickCommand,
                CommandParameter = isPlaceholder ? null : category
            });
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.ReportIssueCategories.CollectionChanged -= OnReportIssueCategoriesChanged;
        ViewModel.Deactivate();
    }
}