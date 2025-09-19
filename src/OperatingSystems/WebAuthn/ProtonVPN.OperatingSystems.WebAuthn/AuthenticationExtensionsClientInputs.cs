using System.Text.Json.Serialization;
using ProtonVPN.OperatingSystems.WebAuthn.Enums;

namespace ProtonVPN.OperatingSystems.WebAuthn;

/// <summary>
/// Contains WebAuthn extensions that are actually supported by Windows 10.
/// These are currently defined in CTAP 2.1 Draft.
/// </summary>
public class AuthenticationExtensionsClientInputs
{
    private bool _enforceCredProtect;

    /// <summary>
    /// This extension is used by the platform to retrieve a symmetric secret from the authenticator when it needs to encrypt or decrypt data using that symmetric secret. This symmetric secret is scoped to a credential. The authenticator and the platform each only have the part of the complete secret to prevent offline attacks. This extension can be used to maintain different secrets on different machines.
    /// </summary>
    /// <see>https://fidoalliance.org/specs/fido2/fido-client-to-authenticator-protocol-v2.1-rd-20191217.html#sctn-hmac-secret-extension</see>
    [JsonPropertyName("hmacCreateSecret")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool HmacCreateSecret { get; set; }

    /// <summary>
    /// This extension indicates that the authenticator supports enhanced protection mode for the credentials created on the authenticator.
    /// If present, verify that the credentialProtectionPolicy value is one of following values: userVerificationOptional, userVerificationOptionalWithCredentialIDList, userVerificationRequired
    /// </summary>
    /// <see>https://fidoalliance.org/specs/fido2/fido-client-to-authenticator-protocol-v2.1-rd-20191217.html#sctn-credProtect-extension</see>
    [JsonPropertyName("credentialProtectionPolicy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public UserVerification CredProtect { get; set; }

    /// <summary>
    /// Controls whether it is better to fail to create a credential rather than ignore the protection policy. When enforceCredentialProtectionPolicy is true, and credentialProtectionPolicy is either userVerificationOptionalWithCredentialIDList or userVerificationRequired, the platform SHOULD NOT create the credential in a way that does not implement the requested protection policy.
    /// </summary>
    /// <see>https://fidoalliance.org/specs/fido2/fido-client-to-authenticator-protocol-v2.1-rd-20191217.html#sctn-credProtect-extension</see>
    [JsonPropertyName("enforceCredentialProtectionPolicy")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool EnforceCredProtect
    {
        get
        {
            // Do not serialize if CredProtect is not set.
            return _enforceCredProtect && CredProtect != UserVerification.Any;
        }
        set
        {
            _enforceCredProtect = value == true;
        }
    }

    /// <summary>
    /// This extension allows WebAuthn Relying Parties that have previously registered a credential using the legacy FIDO JavaScript APIs to request an assertion.
    /// </summary>
    /// <see>https://www.w3.org/TR/webauthn/#sctn-appid-extension</see>
    [JsonPropertyName("appid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string AppID { get; set; }

    /// <summary>
    /// This extension returns the current minimum PIN length value. This value does not decrease unless the authenticator is reset, in which case, all the credentials are reset. This extension is only applicable during credential creation.
    /// </summary>
    /// <see>https://fidoalliance.org/specs/fido-v2.1-ps-20210615/fido-client-to-authenticator-protocol-v2.1-ps-20210615.html#sctn-minpinlength-extension</see>
    [JsonPropertyName("minPinLength")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool MinimumPinLength { get; set; }

    [JsonPropertyName("credBlob")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public byte[] CredentialBlob { get; set; }


    [JsonPropertyName("getCredBlob")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool GetCredentialBlob { get; set; }

    [JsonPropertyName("hmacGetSecret")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public HMACGetSecretInput HmacGetSecret { get; set; }
}
