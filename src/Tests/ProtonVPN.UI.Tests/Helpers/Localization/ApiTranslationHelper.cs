/*
 * Copyright (c) 2026 Proton AG
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
using ProtonVPN.UI.Tests.Enums;

namespace ProtonVPN.UI.Tests.TestsHelper;

public static class ApiTranslationHelper
{
    private static readonly Dictionary<string, Dictionary<Language, string>> _translations = new()
    {
        ["Login_IncorrectUserError"] = new()
        {
            [Language.English] = "This username does not exist. Please try again with a different username.",
            [Language.German] = "Dieser Benutzername existiert nicht. Bitte versuche es erneut mit einem anderen Benutzernamen.",
            [Language.French] = "Ce nom d'utilisateur n'existe pas. Réessayez avec un autre nom d'utilisateur."
        },

        ["Login_IncorrectPasswordError"] = new()
        {
            [Language.English] = "The password is not correct. Please try again with a different password.",
            [Language.German] = "Das Passwort ist nicht korrekt. Bitte versuche es erneut mit einem anderen Passwort.",
            [Language.French] = "Le mot de passe n'est pas correct. Réessayez avec un autre mot de passe."
        },

        ["Login_IncorrectCredentialsError"] = new()
        {
            [Language.English] = "Incorrect login credentials. Please try again.",
            [Language.German] = "Falsche Anmeldedaten. Bitte versuche es erneut.",
            [Language.French] = "Identifiants de connexion incorrects. Veuillez réessayer."
        },

        ["Login_InvalidUsername"] = new()
        {
            [Language.English] = "Invalid username",
            [Language.German] = "Ungültiger Benutzername",
            [Language.French] = "Le nom d'utilisateur n'est pas valide."
        },

        ["Login_NoServersError"] = new()
        {
            [Language.English] = "To start your journey in Proton VPN please contact your organization administrator to assign VPN connections to your account.",
            [Language.German] = "Um deine ersten Schritte mit Proton VPN zu beginnen, wende dich bitte an den Administrator deines Unternehmens, um deinem Konto VPN-Verbindungen zuzuweisen.",
            [Language.French] = "Pour démarrer avec Proton VPN, contactez l'administrateur de votre organisation pour qu'il attribue des connexions VPN à votre compte."
        },

        ["LoginSso_SsoLoginError"] = new()
        {
            [Language.English] = "Email domain associated to an existing organization. Please sign in with SSO",
            [Language.German] = "Diese E-Mail-Domain gehört zu einer bestehenden Organisation. Bitte melde dich über SSO an.",
            [Language.French] = "Le domaine de messagerie est associé à une organisation existante. Connectez-vous avec une authentification unique (SSO)."
        },

        ["LoginSso_RegularLoginError"] = new()
        {
            [Language.English] = "Email domain not found, please sign in with a password",
            [Language.German] = "E-Mail-Domain konnte nicht gefunden werden, bitte melde dich mit einem Passwort an.",
            [Language.French] = "Le domaine de messagerie est introuvable. Connectez-vous avec un mot de passe."
        },

        ["ReportIssue_ConnectingToVpn"] = new()
        {
            [Language.English] = "Connecting to VPN",
            [Language.German] = "Verbindung mit VPN",
            [Language.French] = "Connexion au VPN"
        },

        ["ReportIssue_BrowsingSpeed"] = new()
        {
            [Language.English] = "Browsing speed",
            [Language.German] = "Surf-Geschwindigkeit",
            [Language.French] = "Vitesse de navigation"
        },

        ["ReportIssue_WeakConnection"] = new()
        {
            [Language.English] = "Weak or unstable connection",
            [Language.German] = "Schwache oder instabile Verbindung",
            [Language.French] = "La connexion est faible ou instable."
        },

        ["State_California"] = new()
        {
            [Language.English] = "California",
            [Language.German] = "Kalifornien",
            [Language.French] = "Californie"
        },

        ["City_Brussels"] = new()
        {
            [Language.English] = "Brussels",
            [Language.German] = "Brüssel",
            [Language.French] = "Bruxelles"
        }
    };

    public static string GetTranslatedString(string key, Language? language = null)
    {
        language ??= LanguageHelper.CurrentLanguage;

        return _translations[key][language.Value];
    }
}