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

using System.Collections.Generic;
using System.Windows.Input;
using Caliburn.Micro;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Api.Contracts.ReportAnIssue;
using ProtonVPN.BugReporting.Actions;
using ProtonVPN.Core.ReportAnIssue;

namespace ProtonVPN.BugReporting.Steps
{
    public class CategorySelectionViewModel : Screen
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IReportAnIssueFormDataProvider _reportAnIssueFormDataProvider;

        public CategorySelectionViewModel(IEventAggregator eventAggregator, IReportAnIssueFormDataProvider reportAnIssueFormDataProvider)
        {
            _eventAggregator = eventAggregator;
            _reportAnIssueFormDataProvider = reportAnIssueFormDataProvider;
            SelectCategoryCommand = new RelayCommand<string>(SelectCategoryAction);
        }

        public List<IssueCategoryResponse> Categories => _reportAnIssueFormDataProvider.GetCategories();

        public ICommand SelectCategoryCommand { get; }

        public void SelectCategoryAction(string category)
        {
            _eventAggregator.PublishOnUIThread(new SelectCategoryAction(category));
        }
    }
}