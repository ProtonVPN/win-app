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
using System.Text;
using System.Xml;

namespace ProtonVPN.MarkupValidator
{
    internal class ResourceFile
    {
        public string Error { get; private set; }

        private readonly string _path;

        public ResourceFile(string path)
        {
            _path = path;
        }

        public bool Valid()
        {
            Error = string.Empty;

            var valid = true;
            var xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.Load(XmlReader.Create(new StreamReader(_path, Encoding.GetEncoding("UTF-8"))));
                var itemNodes = xmlDoc.SelectNodes("//root/data/value");
                if (itemNodes != null)
                {
                    foreach (XmlNode itemNode in itemNodes)
                    {
                        var value = new DictionaryValue(itemNode.InnerText);
                        if (value.Valid())
                        {
                            continue;
                        }

                        var name = itemNode.ParentNode?.Attributes?.GetNamedItem("name").InnerText;
                        if (name != null)
                        {
                            Error = $"Element name: {name}\n";
                        }

                        Error += value.Error;
                        valid = false;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Error += e.Message;
                valid = false;
            }

            return valid;
        }
    }
}
