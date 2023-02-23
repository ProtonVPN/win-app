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

using System.Windows.Input;
using Caliburn.Micro;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.About;
using ProtonVPN.BugReporting.Actions;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.ReportAnIssue;

namespace ProtonVPN.BugReporting.Steps
{
    public class StepsContainerViewModel : Screen,
        ILoggedInAware,
        ILogoutAware,
        IHandle<SelectCategoryAction>,
        IHandle<FillTheFormAction>,
        IHandle<FormStateChange>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IReportAnIssueFormDataProvider _reportAnIssueFormDataProvider;
        private readonly CategorySelectionViewModel _categorySelectionViewModel;
        private readonly SolutionsViewModel _solutionsViewModel;
        private readonly FormViewModel _formViewModel;

        private Screen _screenViewModel;
        private int _step = 1;
        private string _category;

        public ICommand GoBackCommand { get; }

        public bool IsToShowBackButton => Step > 1;

        public Screen ScreenViewModel
        {
            get => _screenViewModel;
            set => Set(ref _screenViewModel, value);
        }

        public UpdateViewModel UpdateViewModel { get; }

        public int Step
        {
            get => _step;
            set
            {
                Set(ref _step, value);
                ScreenViewModel = GetScreen();
                NotifyOfPropertyChange(nameof(IsToShowBackButton));
            }
        }

        public StepsContainerViewModel(IEventAggregator eventAggregator,
            IReportAnIssueFormDataProvider reportAnIssueFormDataProvider,
            UpdateViewModel updateViewModel,
            CategorySelectionViewModel categorySelectionViewModel,
            SolutionsViewModel solutionsViewModel,
            FormViewModel formViewModel)
        {
            eventAggregator.Subscribe(this);

            _eventAggregator = eventAggregator;
            _reportAnIssueFormDataProvider = reportAnIssueFormDataProvider;
            _categorySelectionViewModel = categorySelectionViewModel;
            _solutionsViewModel = solutionsViewModel;
            _formViewModel = formViewModel;

            UpdateViewModel = updateViewModel;
            ScreenViewModel = categorySelectionViewModel;
            GoBackCommand = new RelayCommand(GoBackAction);
        }

        private Screen GetScreen()
        {
            switch (Step)
            {
                case 1:
                    return _categorySelectionViewModel;
                case 2:
                    return _solutionsViewModel;
                case 3:
                    return _formViewModel;
                default:
                    return _categorySelectionViewModel;
            }
        }

        public void OnUserLoggedIn()
        {
            ShowFirstStep();
        }

        public void OnUserLoggedOut()
        {
            ShowFirstStep();
        }

        public void Handle(FormStateChange message)
        {
            if (message.State == FormState.Sent)
            {
                ShowFirstStep();
            }
        }

        public void Handle(SelectCategoryAction message)
        {
            _category = message.Category;

            if (HasSuggestions(message.Category))
            {
                Step = 2;
            }
            else
            {
                _eventAggregator.PublishOnUIThread(new FillTheFormAction(message.Category));
            }
        }

        private bool HasSuggestions(string category)
        {
            return !_reportAnIssueFormDataProvider.GetSuggestions(category).IsNullOrEmpty();
        }

        public void Handle(FillTheFormAction message)
        {
            Step = 3;
        }

        private void GoBackAction()
        {
            switch (Step)
            {
                case <= 1:
                    return;
                case 3 when !HasSuggestions(_category):
                    Step = 1;
                    break;
                default:
                    Step--;
                    break;
            }
        }

        private void ShowFirstStep()
        {
            Step = 1;
        }
    }
}