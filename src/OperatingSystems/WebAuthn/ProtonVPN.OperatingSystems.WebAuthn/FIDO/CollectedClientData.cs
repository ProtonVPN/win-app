using System.Text.Json.Serialization;
using ProtonVPN.OperatingSystems.WebAuthn.Serialization;

namespace ProtonVPN.OperatingSystems.WebAuthn.FIDO;

public class CollectedClientData
{
    /// <summary>
    /// This member contains the string "webauthn.create" when creating new credentials, and "webauthn.get" when getting an assertion from an existing credential.
    /// </summary>
    /// <remarks>
    /// The purpose of this member is to prevent certain types of signature confusion attacks (where an attacker substitutes one legitimate signature for another).
    /// </remarks>
    /// <see>https://www.w3.org/TR/webauthn-2/#dom-collectedclientdata-type</see>
    [JsonPropertyName("type")]
    [JsonRequired()]
    [JsonPropertyOrder(0)]
    public string Type
    {
        get;
        set;
    }

    /// <summary>
    /// This member contains the base64url encoding of the challenge provided by the Relying Party.
    /// </summary>
    /// <see>https://www.w3.org/TR/webauthn-2/#dom-collectedclientdata-challenge</see>
    [JsonPropertyName("challenge")]
    [JsonRequired()]
    [JsonConverter(typeof(Base64UrlConverter))]
    [JsonPropertyOrder(1)]
    public byte[] Challenge
    {
        get;
        set;
    }

    /// <summary>
    /// This member contains the fully qualified origin of the requester, as provided to the authenticator by the client, in the syntax defined by RFC6454.
    /// </summary>
    /// <see>https://www.w3.org/TR/webauthn-2/#dom-collectedclientdata-origin</see>
    [JsonPropertyName("origin")]
    [JsonRequired()]
    [JsonPropertyOrder(2)]
    public string Origin
    {
        get;
        set;
    }

    /// <summary>
    /// This member contains the inverse of the sameOriginWithAncestors argument value that was passed into the public method.
    /// </summary>
    [JsonPropertyName("crossOrigin")]
    [JsonPropertyOrder(3)]
    public bool CrossOrigin
    {
        get;
        set;
    }
}
