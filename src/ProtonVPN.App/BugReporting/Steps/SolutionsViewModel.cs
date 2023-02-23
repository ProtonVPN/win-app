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
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.ReportAnIssue;

namespace ProtonVPN.BugReporting.Steps
{
    public class SolutionsViewModel : Screen, IHandle<SelectCategoryAction>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IReportAnIssueFormDataProvider _reportAnIssueFormDataProvider;
        private readonly IOsProcesses _osProcesses;
        private string _categorySubmitName;

        public SolutionsViewModel(IEventAggregator eventAggregator, IReportAnIssueFormDataProvider reportAnIssueFormDataProvider, IOsProcesses osProcesses)
        {
            eventAggregator.Subscribe(this);
            _eventAggregator = eventAggregator;
            _reportAnIssueFormDataProvider = reportAnIssueFormDataProvider;
            _osProcesses = osProcesses;
            FillTheFormCommand = new RelayCommand(FillTheFormAction);
            LearnMoreCommand = new RelayCommand<string>(LearnMoreAction);
        }

        public ICommand FillTheFormCommand { get; }
        public ICommand LearnMoreCommand { get; }

        private List<IssueSuggestionResponse> _suggestions = new();

        public List<IssueSuggestionResponse> Suggestions
        {
            get => _suggestions;
            set => Set(ref _suggestions, value);
        }

        public void Handle(SelectCategoryAction message)
        {
            _categorySubmitName = message.Category;
            Suggestions = _reportAnIssueFormDataProvider.GetSuggestions(message.Category);
        }

        private void FillTheFormAction()
        {
            _eventAggregator.PublishOnUIThread(new FillTheFormAction(_categorySubmitName));
        }

        private void LearnMoreAction(string url)
        {
            ActiveUrl activeUrl = new(_osProcesses, url);
            activeUrl.Open();
        }
    }
}