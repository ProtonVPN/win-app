using System.Text.Json.Serialization;
using ProtonVPN.OperatingSystems.WebAuthn.Serialization;

namespace ProtonVPN.OperatingSystems.WebAuthn;

/// <summary>
/// Information about a user rntity.
/// </summary>
/// <remarks>Corresponds to WEBAUTHN_USER_ENTITY_INFORMATION.</remarks>
/// <see>https://www.w3.org/TR/webauthn-2/#dictdef-publickeycredentialuserentity</see>
public sealed class UserInformation
{
    /// <summary>
    /// Identifier for the user.
    /// </summary>
    [JsonPropertyName("id")]
    [JsonConverter(typeof(Base64UrlConverter))]
    public byte[] Id { get; set; }

    /// <summary>
    /// Contains a detailed name for this account, such as "john.p.smith@example.com".
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Optional URL that can be used to retrieve an image containing the user's current avatar, or a data URI that contains the image data.
    /// </summary>
    [JsonPropertyName("icon")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Icon { get; set; }

    /// <summary>
    /// Contains the friendly name associated with the user account by the Relying Party, such as "John P. Smith".
    /// </summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }
}
