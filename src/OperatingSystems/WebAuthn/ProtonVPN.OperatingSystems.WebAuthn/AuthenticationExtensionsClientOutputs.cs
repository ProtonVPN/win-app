using System.Text.Json.Serialization;
using ProtonVPN.OperatingSystems.WebAuthn.Enums;

namespace ProtonVPN.OperatingSystems.WebAuthn;

/// <summary>
/// Contains WebAuthn extensions that are actually supported by Windows 10.
/// These are currently defined in CTAP 2.1 Draft.
/// </summary>
public class AuthenticationExtensionsClientOutputs
{
    /// <summary>
    /// This extension is used by the platform to retrieve a symmetric secret from the authenticator when it needs to encrypt or decrypt data using that symmetric secret. This symmetric secret is scoped to a credential. The authenticator and the platform each only have the part of the complete secret to prevent offline attacks. This extension can be used to maintain different secrets on different machines.
    /// </summary>
    /// <see>https://fidoalliance.org/specs/fido2/fido-client-to-authenticator-protocol-v2.1-rd-20191217.html#sctn-hmac-secret-extension</see>
    [JsonPropertyName("hmacCreateSecret")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool HmacSecret { get; set; }

    /// <summary>
    /// This extension indicates that the authenticator supports enhanced protection mode for the credentials created on the authenticator.
    /// If present, verify that the credentialProtectionPolicy value is one of following values: userVerificationOptional, userVerificationOptionalWithCredentialIDList, userVerificationRequired
    /// </summary>
    /// <see>https://fidoalliance.org/specs/fido2/fido-client-to-authenticator-protocol-v2.1-rd-20191217.html#sctn-credProtect-extension</see>
    [JsonPropertyName("credentialProtectionPolicy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public UserVerification CredProtect { get; set; }

    /// <summary>
    /// HMAC Secret Extension
    /// </summary>
    /// <remarks>This extension is only applicable during credential creation.</remarks>
    /// <see>https://fidoalliance.org/specs/fido-v2.1-ps-20210615/fido-client-to-authenticator-protocol-v2.1-ps-20210615.html#sctn-hmac-secret-extension</see>
    [JsonPropertyName("hmacGetSecret")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public HMACGetSecretOutput HmacGetSecret { get; set; }

    [JsonPropertyName("minPinLength")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public uint? MinimumPinLength { get; set; }

    [JsonPropertyName("credBlob")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]

    public bool CredentialBlobCreated { get; set; }
}
