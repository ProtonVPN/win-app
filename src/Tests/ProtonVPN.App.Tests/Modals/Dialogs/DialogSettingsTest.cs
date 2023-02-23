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
using ProtonVPN.Modals.Dialogs;

namespace ProtonVPN.App.Tests.Modals.Dialogs
{
    [TestClass]
    public class DialogSettingsTest
    {
        [TestMethod]
        public void FromMessage_ShouldSet_Message()
        {
            const string message = "Message text";

            DialogSettings settings = DialogSettings.FromMessage(message);

            settings.Message.Should().Be(message);
        }

        [TestMethod]
        public void FromMessage_ShouldSet_PrimaryButtonText()
        {
            DialogSettings settings = DialogSettings.FromMessage("");

            settings.PrimaryButtonText.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public void FromMessage_ShouldSet_SecondaryButtonText()
        {
            DialogSettings settings = DialogSettings.FromMessage("");

            settings.SecondaryButtonText.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public void WithPrimaryButtonText_ShouldSet_PrimaryButtonText()
        {
            const string text = "Primary button text";
            DialogSettings settings = DialogSettings.FromMessage("");

            settings = settings.WithPrimaryButtonText(text);

            settings.PrimaryButtonText.Should().Be(text);
        }

        [TestMethod]
        public void WithSecondaryButtonText_ShouldSet_SecondaryButtonText()
        {
            const string text = "Secondary button text";
            DialogSettings settings = DialogSettings.FromMessage("");

            settings = settings.WithSecondaryButtonText(text);

            settings.SecondaryButtonText.Should().Be(text);
        }

        [TestMethod]
        public void ChainedCalls_ShouldSet_AllProperties()
        {
            const string message = "Message text";
            const string primaryText = "Secondary button text";
            const string secondaryText = "Secondary button text";

            DialogSettings settings = DialogSettings.FromMessage(message)
                                                    .WithPrimaryButtonText(primaryText)
                                                    .WithSecondaryButtonText(secondaryText);

            settings.Message.Should().Be(message);
            settings.PrimaryButtonText.Should().Be(primaryText);
            settings.SecondaryButtonText.Should().Be(secondaryText);
        }
    }
}
