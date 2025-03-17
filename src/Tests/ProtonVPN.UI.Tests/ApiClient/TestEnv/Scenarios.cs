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

namespace ProtonVPN.UI.Tests.ApiClient.TestEnv;
internal class Scenarios
{
    public const string UNHARDJAIL_ALL = "enable/sessions_un_hardjail_all";
    public const string RESET = "reset";
    public const string HARDJAIL_86101 = "once/sessions_hardjail_all_86101";
    public const string HARDJAIL_86102 = "once/sessions_hardjail_all_86102";
    public const string HARDJAIL_86103 = "once/sessions_hardjail_all_86103";
    public const string HARDJAIL_86105 = "once/sessions_hardjail_all_86105";
    public const string HARDJAIL_86110 = "once/sessions_hardjail_all_86110";
    public const string HARDJAIL_86113 = "once/sessions_hardjail_all_86113";
    public const string HARDJAIL_86203 = "once/sessions_hardjail_all_86203";
    public const string HARDJAIL_86999 = "once/sessions_hardjail_all_86999";
    public const string PUT_NL_1_IN_MAINTENANCE = "enable/server_down_nl_01";
    public const string BLOCK_PROD_ENDPOINT = "enable/block_vpn_prod_api_endpoint";
    public const string BLOCK_DOH_ENDPOINT = "enable/block_vpn_prod_api_doh_endpoints";

    public const string MAINTENANCE_ONE_MINUTE = "GET /vpn/v2/clientconfig:\n " +
        "- body: {\"Code\":1000,\"DefaultPorts\":{\"OpenVPN\":{\"UDP\":[80,51820,4569,1194,5060],\"TCP\":[443,7770,8443]}," +
        "\"WireGuard\":{\"UDP\":[443,88,1224,51820,500,4500],\"TCP\":[443]}},\"HolesIPs\":[\"62.112.9.168\",\"104.245.144.186\"]," +
        "\"ServerRefreshInterval\":1," +
        "\"FeatureFlags\":{\"NetShield\":true,\"GuestHoles\":false,\"ServerRefresh\":true,\"StreamingServicesLogos\":true,\"PortForwarding\":true," +
        "\"ModerateNAT\":true,\"SafeMode\":false,\"StartConnectOnBoot\":true,\"PollNotificationAPI\":true,\"VpnAccelerator\":true,\"SmartReconnect\":true," +
        "\"PromoCode\":false,\"WireGuardTls\":true,\"Telemetry\":true,\"NetShieldStats\":true,\"BusinessEvents\":false,\"ShowNewFreePlan\":false}," +
        "\"SmartProtocol\":{\"OpenVPN\":true,\"IKEv2\":true,\"WireGuard\":true,\"WireGuardTCP\":true,\"WireGuardTLS\":true}," +
        "\"RatingSettings\":{\"EligiblePlans\":[\"vpn2022\",\"bundle2022\",\"family2022\",\"visionary2022\",\"vpnpass2023\"],\"SuccessConnections\":2," +
        "\"DaysLastReviewPassed\":100,\"DaysConnected\":999,\"DaysFromFirstConnection\":0},\"ChangeServerAttemptLimit\":4,\"ChangeServerShortDelayInSeconds\":90," +
        "\"ChangeServerLongDelayInSeconds\":1200}";
}
