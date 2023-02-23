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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Service.Validation;
using ProtonVPN.Crypto;
using ProtonVPN.Service.Contract.Crypto;

namespace ProtonVPN.Service.Contract.Vpn
{
    [DataContract]
    public class VpnHostContract : IValidatableObject
    {
        [DataMember] public string Name { get; set; }

        [DataMember] public string Ip { get; set; }

        [DataMember] public string Label { get; set; }

        [DataMember] public ServerPublicKeyContract X25519PublicKey { get; set; }

        [DataMember] public string Signature { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Ip.IsValidIpAddress())
            {
                yield return new ValidationResult($"Invalid server IP address: {Ip} for server {Name}");
            }

            ValidationResult signatureValidationResult = ValidateSignature(validationContext);
            if (signatureValidationResult != null)
            {
                yield return signatureValidationResult;
            }

            ValidationResult publicKeyValidationResult = X25519PublicKey?.Validate(validationContext).FirstOrDefault();
            if (publicKeyValidationResult != null)
            {
                yield return publicKeyValidationResult;
            }
        }

        private ValidationResult ValidateSignature(ValidationContext validationContext)
        {
            if (Signature.IsNullOrEmpty())
            {
                return new ValidationResult(
                    $"Missing signature for server with name {Name} and IP {Ip}.");
            }

            string publicKey = GetPublicKey(validationContext);
            if (publicKey != null)
            {
                Ed25519SignatureValidator validator = new();
                if (!validator.IsValid(GetServerValidationData(), Signature, publicKey))
                {
                    return new ValidationResult(
                        $"Incorrect signature {Signature} for server with name {Name} and IP {Ip}.");
                }
            }
            else
            {
                return new ValidationResult("Missing public key for validating server object.");
            }

            return null;
        }

        private string GetPublicKey(ValidationContext validationContext)
        {
            bool result = validationContext.Items.TryGetValue(ValidatableObjectValidator.ServerValidationPublicKeyValue,
                out object publicKeyValue);
            return result ? (string)publicKeyValue : null;
        }

        private string GetServerValidationData()
        {
            try
            {
                ServerValidationObject server = new() { Server = new PhysicalServer { EntryIP = Ip, Label = Label } };
                return JsonConvert.SerializeObject(server);
            }
            catch (JsonException)
            {
                return string.Empty;
            }
        }
    }
}