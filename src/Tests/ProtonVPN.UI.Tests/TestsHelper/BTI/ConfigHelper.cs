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

using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ProtonVPN.UI.Tests.TestsHelper.BTI
{
    public class ConfigHelper
    {
        public static void Set(string desiredKey, string newValue)
        {
            string json = File.ReadAllText(TestData.TestConfigPath);
            JObject jsonObject = JObject.Parse(json);
            string updatedJsonString = UpdateJsonValue(jsonObject, desiredKey, newValue);

            File.WriteAllText(TestData.TestConfigPath, updatedJsonString);
        }

        public static void SetInterval(string desiredKey, TimeSpan interval)
        {
            string formatedInterval = string.Format("{0:D2}:{1:D2}:{2:D2}", interval.Hours, interval.Minutes, interval.Seconds);
            Set(desiredKey, formatedInterval);
        }

        private static string UpdateJsonValue(JToken token, string desiredKey, string newValue)
        {
            foreach (JProperty prop in token.Children<JProperty>().ToList())
            {
                if (prop.Name == desiredKey)
                {
                    prop.Value = newValue;
                }
                else
                {
                    UpdateJsonValue(prop.Value, desiredKey, newValue);
                }
            }

            return token.ToString(Formatting.None);
        }
    }
}
