using System.Text.Json.Serialization;
using ProtonVPN.OperatingSystems.WebAuthn.Enums;
using ProtonVPN.OperatingSystems.WebAuthn.Interop;
using ProtonVPN.OperatingSystems.WebAuthn.Serialization;

namespace ProtonVPN.OperatingSystems.WebAuthn;

public class PublicKeyCredentialDescriptor
{
    /// <summary>
    /// This member contains the type of the public key credential the caller is referring to.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type
    {
        get;
    }

    /// <summary>
    /// This member contains the credential ID of the public key credential the caller is referring to.
    /// </summary>
    [JsonPropertyName("id")]
    [JsonConverter(typeof(Base64UrlConverter))]
    public byte[] Id
    {
        get;
    }

    /// <summary>
    /// This member contains a hint as to how the client might communicate with the managing authenticator of the public key credential the caller is referring to.
    /// </summary>
    [JsonPropertyName("transports")]
    public AuthenticatorTransport Transports
    {
        get;
    }

    [JsonConstructor]
    public PublicKeyCredentialDescriptor(
        byte[] id,
        AuthenticatorTransport transports = AuthenticatorTransport.NoRestrictions,
        string type = ApiConstants.CredentialTypePublicKey)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Transports = transports;
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }
}
