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
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Config.Url;
using ProtonVPN.Modals.Dialogs;

namespace ProtonVPN.Modals.ApiActions
{
    public class ApiActionModalViewModel : QuestionModalViewModel
    {
        protected readonly IActiveUrls Urls;

        private readonly IOsProcesses _processes;

        public ApiActionModalViewModel(IActiveUrls urls, 
            IOsProcesses processes)
        {
            Urls = urls;
            _processes = processes;

            LinkActionCommand = new RelayCommand<string>(LinkAction);
            MainActionCommand = new RelayCommand(MainAction);
            SecondaryActionCommand = new RelayCommand(SecondaryAction);
        }

        public ICommand LinkActionCommand { get; }
        public ICommand MainActionCommand { get; }
        public ICommand SecondaryActionCommand { get; }
        public IList<BaseResponseDetailAction> LinkDetailActions { get; private set; }
        public BaseResponseDetailAction MainDetailAction { get; private set; }
        public BaseResponseDetailAction SecondaryDetailAction { get; private set; }
        public bool IsToShowMainCloseButton => MainDetailAction == null;
        public bool IsToShowSecondaryCloseButton => SecondaryDetailAction == null && MainDetailAction != null;
        public bool IsToShowMainActionButton => MainDetailAction != null;
        public bool IsToShowSecondaryActionButton => SecondaryDetailAction != null;

        public void SetView(string originalMessage, IList<BaseResponseDetailAction> detailActions)
        {
            PopulateDetailActions(detailActions);
            Message = CreateMessage(originalMessage);
        }

        private void PopulateDetailActions(IList<BaseResponseDetailAction> detailActions)
        {
            MainDetailAction = null;
            SecondaryDetailAction = null;
            IList<BaseResponseDetailAction> linkDetailActions = new List<BaseResponseDetailAction>();
            if (detailActions != null)
            {
                foreach (BaseResponseDetailAction detailAction in detailActions)
                {
                    switch (detailAction.Category)
                    {
                        case "link":
                            linkDetailActions.Add(detailAction);
                            break;
                        case "main_action":
                            MainDetailAction = detailAction;
                            break;
                        case "secondary_action":
                            SecondaryDetailAction = detailAction;
                            break;
                    }
                }
            }
            LinkDetailActions = linkDetailActions;
        }

        private string CreateMessage(string originalMessage)
        {
            string message = originalMessage;
            if (LinkDetailActions.Any())
            {
                message += "<LineBreak/>";
            }
            foreach (BaseResponseDetailAction linkDetailAction in LinkDetailActions)
            {
                message +=
                    "<LineBreak/>" +
                    "<Hyperlink " +
                    $"Command=\"{{Binding LinkActionCommand}}\" " +
                    $"CommandParameter=\"{linkDetailAction.Code}\">" +
                    $"{linkDetailAction.Name}." +
                    "</Hyperlink>";
            }

            return message;
        }

        private void LinkAction(string actionCode)
        {
            BaseResponseDetailAction detailAction = LinkDetailActions.FirstOrDefault(a => a.Code == actionCode);
            OpenDetailActionUrlIfExists(detailAction);
        }

        private void OpenDetailActionUrlIfExists(BaseResponseDetailAction detailAction)
        {
            OpenUrlIfNotNullOrEmpty(detailAction?.URL);
        }

        private void OpenUrlIfNotNullOrEmpty(string urlString)
        {
            if (!string.IsNullOrEmpty(urlString))
            {
                ActiveUrl url = new ActiveUrl(_processes, urlString)
                    .WithQueryParams(new Dictionary<string, string> { { "utm_source", "windowsvpn" } });
                url.Open();
            }
        }

        private void MainAction()
        {
            OpenDetailActionUrlIfExists(MainDetailAction);
            TryClose(true);
        }

        private void SecondaryAction()
        {
            OpenDetailActionUrlIfExists(SecondaryDetailAction);
            TryClose(true);
        }
    }
}
