using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using ProtonVPN.OperatingSystems.WebAuthn.Serialization;

namespace ProtonVPN.OperatingSystems.WebAuthn.Enums;

/// <summary>
/// This enumeration’s values describe authenticators' attachment modalities.
/// </summary>
/// <see>https://www.w3.org/TR/webauthn-2/#enum-attachment</see>
[JsonConverter(typeof(JsonCustomEnumConverter<AuthenticatorAttachment>))]
public enum AuthenticatorAttachment : uint
{
    /// <summary>
    /// No authenticator attachment filters are applied.
    /// </summary>
    /// <remarks>
    /// Corresponds to WEBAUTHN_AUTHENTICATOR_ATTACHMENT_ANY.
    /// </remarks>
    [EnumMember(Value = "NULL")]
    Any = PInvoke.WEBAUTHN_AUTHENTICATOR_ATTACHMENT_ANY,

    /// <summary>
    /// This value indicates platform attachment.
    /// </summary>
    /// <remarks>
    /// Corresponds to WEBAUTHN_AUTHENTICATOR_ATTACHMENT_PLATFORM.
    /// </remarks>
    [EnumMember(Value = "platform")]
    Platform = PInvoke.WEBAUTHN_AUTHENTICATOR_ATTACHMENT_PLATFORM,

    /// <summary>
    /// This value indicates cross-platform attachment.
    /// </summary>
    /// <remarks>
    /// Corresponds to WEBAUTHN_AUTHENTICATOR_ATTACHMENT_CROSS_PLATFORM.
    /// </remarks>
    [EnumMember(Value = "cross-platform")]
    CrossPlatform = PInvoke.WEBAUTHN_AUTHENTICATOR_ATTACHMENT_CROSS_PLATFORM,

    /// <remarks>
    /// Corresponds to WEBAUTHN_AUTHENTICATOR_ATTACHMENT_CROSS_PLATFORM_U2F_V2.
    /// </remarks>
    CrossPlatformU2F = PInvoke.WEBAUTHN_AUTHENTICATOR_ATTACHMENT_CROSS_PLATFORM_U2F_V2
}
