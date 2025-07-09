/*
 * Copyright (c) 2025 Proton AG
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

using Mapsui;
using ProtonVPN.Client.Common.UI.Controls.Map.Animations;

namespace ProtonVPN.Client.Common.UI.Controls.Map;

public static class FeatureExtensions
{
    private const string TRUE_STRING = "true";
    private const string FALSE_STRING = "false";

    private const string IS_CONNECTING = "IsConnecting";
    private const string IS_CONNECTED = "IsConnected";
    private const string IS_ON_HOVER = "IsOnHover";
    private const string IS_CURRENT_COUNRY = "IsCurrentCountry";
    private const string COUNTRY_CODE = "CountryCode";
    private const string IS_UNDER_MAINTENANCE = "IsUnderMaintenance";

    public static bool IsConnecting(this IFeature feature)
    {
        return feature[IS_CONNECTING] as string == TRUE_STRING;
    }

    public static void SetIsConnecting(this IFeature feature, bool isConnecting)
    {
        if (isConnecting)
        {
            feature[IS_CONNECTING] = TRUE_STRING;
        }
        else
        {
            feature[IS_CONNECTING] = null;
        }
    }

    public static bool IsConnected(this IFeature feature)
    {
        return feature[IS_CONNECTED] as string == TRUE_STRING;
    }

    public static void SetIsConnected(this IFeature feature, bool isConnected)
    {
        if (isConnected)
        {
            feature[IS_CONNECTED] = TRUE_STRING;
        }
        else
        {
            feature[IS_CONNECTED] = null;
        }
    }

    public static bool IsCurrentCountry(this IFeature feature)
    {
        return feature[IS_CURRENT_COUNRY] as string == TRUE_STRING;
    }

    public static void SetIsCurrentCountry(this IFeature feature, bool isCurrentCountry)
    {
        if (isCurrentCountry)
        {
            feature[IS_CURRENT_COUNRY] = TRUE_STRING;
        }
        else
        {
            feature[IS_CURRENT_COUNRY] = null;
        }
    }

    public static void SetCountryCode(this IFeature feature, string countryCode)
    {
        feature[COUNTRY_CODE] = countryCode;
    }

    public static string? GetCountryCode(this IFeature feature)
    {
        return feature[COUNTRY_CODE] as string;
    }

    public static void SetIsUnderMaintenance(this IFeature feature, bool isUnderMaintenance)
    {
        feature[IS_UNDER_MAINTENANCE] = isUnderMaintenance ? TRUE_STRING : FALSE_STRING;
    }

    public static bool IsUnderMaintenance(this IFeature feature)
    {
        return feature[IS_UNDER_MAINTENANCE] as string == TRUE_STRING;
    }

    public static bool IsOnHover(this IFeature feature)
    {
        return feature[IS_ON_HOVER] as string == TRUE_STRING;
    }

    public static void SetIsOnHover(this IFeature feature, bool isOnHover)
    {
        if (isOnHover)
        {
            feature[IS_ON_HOVER] = TRUE_STRING;
        }
        else
        {
            feature[IS_ON_HOVER] = null;
        }
    }

    public static PinAnimationType? GetHoverLostAnimationType(this IFeature feature)
    {
        if (feature.IsConnected())
        {
            return PinAnimationType.OnHoverToConnected;
        }
        else if (feature.IsConnecting())
        {
            return null;
        }
        else
        {
            return feature.IsCurrentCountry()
                ? PinAnimationType.CurrentLocationOnHoverToDisconnected
                : PinAnimationType.NormalLocationOnHoverToDisconnected;
        }
    }

    public static PinAnimationType GetOnHoverAnimationType(this IFeature feature)
    {
        if (feature.IsConnected())
        {
            return PinAnimationType.ConnectedToOnHover;
        }

        return feature.IsCurrentCountry()
            ? PinAnimationType.CurrentLocationDisconnectedToOnHover
            : PinAnimationType.NormalLocationDisconnectedToOnHover;
    }

    public static PinAnimationType GetConnectingAnimationType(this IFeature feature)
    {
        if (feature.IsOnHover())
        {
            return PinAnimationType.OnHoverToConnecting;
        }

        if (feature.IsCurrentCountry())
        {
            return feature.IsConnected()
                ? PinAnimationType.CurrentLocationConnectedToConnecting
                : PinAnimationType.CurrentLocationDisconnectedToConnecting;
        }
        else
        {
            return feature.IsConnected()
                ? PinAnimationType.NormalLocationConnectedToConnecting
                : PinAnimationType.NormalLocationDisconnectedToConnecting;
        }
    }

    public static PinAnimationType? GetDisconnectedAnimationType(this IFeature feature)
    {
        if (feature.IsConnecting())
        {
            return feature.IsCurrentCountry()
                ? PinAnimationType.CurrentLocationConnectingToDisconnected
                : PinAnimationType.NormalLocationConnectingToDisconnected;
        }

        if (feature.IsConnected())
        {
            if (!feature.IsCurrentCountry())
            {
                return PinAnimationType.NormalLocationConnectedToDisconnected;
            }
        }

        if (feature.IsCurrentCountry())
        {
            return PinAnimationType.NormalLocationConnectedToDisconnected;
        }

        return null;
    }
}