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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Core.Modals;
using ProtonVPN.Modals.Dialogs;

namespace ProtonVPN.App.Tests.Modals.Dialogs
{
    [TestClass]
    public class DialogsTest
    {
        private IModals _modals;
        private IDialogSettings _settings;
        private readonly WarningModalViewModel _warningViewModel = new WarningModalViewModel();
        private readonly QuestionModalViewModel _questionViewModel = new QuestionModalViewModel();

        [TestInitialize]
        public void TestInitialize()
        {
            _modals = Substitute.For<IModals>();
            _settings = Substitute.For<IDialogSettings>();
        }

        [TestMethod]
        public void ShowWarning_ShouldCall_Modals_Show()
        {
            ProtonVPN.Modals.Dialogs.Dialogs dialogs = new ProtonVPN.Modals.Dialogs.Dialogs(_modals, _warningViewModel, _questionViewModel);

            dialogs.ShowWarning("");

            _modals.Received(1).Show<WarningModalViewModel>();
        }

        [TestMethod]
        public void ShowWarning_ShouldSet_WarningModalViewModel_Message()
        {
            const string message = "Message test text";
            ProtonVPN.Modals.Dialogs.Dialogs dialogs = new ProtonVPN.Modals.Dialogs.Dialogs(_modals, _warningViewModel, _questionViewModel);

            dialogs.ShowWarning(message);

            _warningViewModel.Message.Should().Be(message);
        }

        [TestMethod]
        public void ShowQuestion_ShouldCall_Modals_Show()
        {
            ProtonVPN.Modals.Dialogs.Dialogs dialogs = new ProtonVPN.Modals.Dialogs.Dialogs(_modals, _warningViewModel, _questionViewModel);

            dialogs.ShowQuestion("");

            _modals.Received(1).Show<QuestionModalViewModel>();
        }

        [TestMethod]
        public void ShowQuestion_ShouldSet_QuestionModalViewModel_Message()
        {
            const string message = "Message test text";
            ProtonVPN.Modals.Dialogs.Dialogs dialogs = new ProtonVPN.Modals.Dialogs.Dialogs(_modals, _warningViewModel, _questionViewModel);

            dialogs.ShowQuestion(message);

            _questionViewModel.Message.Should().Be(message);
        }

        [TestMethod]
        public void ShowQuestion_Override_ShouldCall_Modals_Show()
        {
            ProtonVPN.Modals.Dialogs.Dialogs dialogs = new ProtonVPN.Modals.Dialogs.Dialogs(_modals, _warningViewModel, _questionViewModel);

            dialogs.ShowQuestion(_settings);

            _modals.Received(1).Show<QuestionModalViewModel>();
        }

        [TestMethod]
        public void ShowQuestion_Override_ShouldSet_QuestionModalViewModel_Message()
        {
            const string message = "Message text";
            _settings.Message.Returns(message);

            ProtonVPN.Modals.Dialogs.Dialogs dialogs = new ProtonVPN.Modals.Dialogs.Dialogs(_modals, _warningViewModel, _questionViewModel);

            dialogs.ShowQuestion(_settings);

            _questionViewModel.Message.Should().Be(message);
        }

        [TestMethod]
        public void ShowQuestion_Override_ShouldSet_QuestionModalViewModel_PrimaryButtonText()
        {
            const string primaryText = "Primary button text";
            _settings.PrimaryButtonText.Returns(primaryText);

            ProtonVPN.Modals.Dialogs.Dialogs dialogs = new ProtonVPN.Modals.Dialogs.Dialogs(_modals, _warningViewModel, _questionViewModel);

            dialogs.ShowQuestion(_settings);

            _questionViewModel.PrimaryButtonText.Should().Be(primaryText);
        }

        [TestMethod]
        public void ShowQuestion_Override_ShouldSet_QuestionModalViewModel_SecondaryButtonText()
        {
            const string secondaryText = "Secondary button text";
            _settings.SecondaryButtonText.Returns(secondaryText);

            ProtonVPN.Modals.Dialogs.Dialogs dialogs = new ProtonVPN.Modals.Dialogs.Dialogs(_modals, _warningViewModel, _questionViewModel);

            dialogs.ShowQuestion(_settings);

            _questionViewModel.SecondaryButtonText.Should().Be(secondaryText);
        }
    }
}
