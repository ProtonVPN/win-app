using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using ProtonVPN.OperatingSystems.WebAuthn.Serialization;

namespace ProtonVPN.OperatingSystems.WebAuthn.Enums;

/// <summary>
/// Defines the credential protection policy.
/// </summary>
[JsonConverter(typeof(JsonCustomEnumConverter<UserVerification>))]
public enum UserVerification : uint
{
    /// <summary>
    /// Extension is not set.
    /// </summary>
    [EnumMember(Value = "NULL")]
    Any = PInvoke.WEBAUTHN_USER_VERIFICATION_ANY,

    /// <summary>
    /// This reflects "FIDO_2_0" semantics. In this configuration, user verification is optional with or without credentialID list. This is the default state of the credential if the extension is not specified and the authenticator does not report a defaultCredProtect value in the authenticatorGetInfo response.
    /// </summary>
    [EnumMember(Value = "userVerificationOptional")]
    Optional = PInvoke.WEBAUTHN_USER_VERIFICATION_OPTIONAL,

    /// <summary>
    /// In this configuration, credential is discovered only when its credentialID is provided by the platform or when user verification is performed.
    /// </summary>
    [EnumMember(Value = "userVerificationOptionalWithCredentialIDList")]
    OptionalWithCredentialIDList = PInvoke.WEBAUTHN_USER_VERIFICATION_OPTIONAL_WITH_CREDENTIAL_ID_LIST,

    /// <summary>
    /// This reflects that discovery and usage of the credential MUST be preceeded by user verification.
    /// </summary>
    [EnumMember(Value = "userVerificationRequired")]
    Required = PInvoke.WEBAUTHN_USER_VERIFICATION_REQUIRED
}
