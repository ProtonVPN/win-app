using System.Text.Json.Serialization;
using ProtonVPN.OperatingSystems.WebAuthn.Serialization;

namespace ProtonVPN.OperatingSystems.WebAuthn;

/// <summary>
/// The AuthenticatorAssertionResponse interface represents an authenticator's response
/// to a client’s request for generation of a new authentication assertion given
/// the WebAuthn Relying Party's challenge and OPTIONAL list of credentials it is aware of.
/// This response contains a cryptographic signature proving possession of the credential private key,
/// and optionally evidence of user consent to a specific transaction.
/// </summary>
public class AuthenticatorAssertionResponse : AuthenticatorResponse
{
    /// <summary>
    /// This attribute contains the authenticator data returned by the authenticator.
    /// </summary>
    [JsonConverter(typeof(Base64UrlConverter))]
    public byte[] AuthenticatorData { get; set; }

    /// <summary>
    /// This attribute contains the raw signature returned from the authenticator.
    /// </summary>
    [JsonConverter(typeof(Base64UrlConverter))]
    public byte[] Signature { get; set; }

    /// <summary>
    /// This attribute contains the user handle returned from the authenticator, or null if the authenticator did not return a user handle.
    /// </summary>
    [JsonPropertyName("userHandle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(Base64UrlConverter))]
    public byte[] UserHandle { get; set; }

    /// <summary>
    /// This attribute contains the credential id returned by the authenticator.
    /// </summary>
    [JsonConverter(typeof(Base64UrlConverter))]
    public byte[] CredentialId { get; set; }
}
