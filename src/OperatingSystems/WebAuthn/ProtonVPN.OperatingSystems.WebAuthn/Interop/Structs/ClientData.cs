using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using ProtonVPN.OperatingSystems.WebAuthn.FIDO;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Marshalers;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.StructVersions;

namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.Structs;

/// <summary>
/// Information about client data.
/// </summary>
/// <remarks>Corresponds to WEBAUTHN_CLIENT_DATA.</remarks>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public class ClientData : IDisposable
{
    /// <summary>
    /// Version of this structure, to allow for modifications in the future.
    /// </summary>
    private ClientDataVersion _version = ClientDataVersion.Current;

    private int _clientDataLength;

    private ByteArrayIn _clientData;

    /// <summary>
    /// Hash algorithm ID used to hash the ClientDataJSON field.
    /// </summary>
    public string HashAlgId { get; set; }

    /// <summary>
    /// JSON serialization of the client data.
    /// </summary>
    public string ClientDataJson
    {
        set
        {
            ClientDataRaw = value != null ? Encoding.UTF8.GetBytes(value) : null;
        }
    }

    /// <summary>
    /// UTF-8 encoded JSON serialization of the client data.
    /// </summary>
    public byte[] ClientDataRaw
    {
        get
        {
            return _clientData?.Read(_clientDataLength);
        }
        set
        {
            // Get rid of any previous data first
            _clientData?.Dispose();

            // Now replace the previous value with a new one
            _clientDataLength = value?.Length ?? 0;
            _clientData = new ByteArrayIn(value);
        }
    }

    public ClientData(CollectedClientData clientData)
    {
        ClientDataJson = JsonSerializer.Serialize(clientData);
        // Note that SHA-256 is currently hardcoded in Chromium and Firefox.
        HashAlgId = ApiConstants.HashAlgorithmSha256;
    }

    public ClientData(byte[] clientDataJson)
    {
        ClientDataRaw = clientDataJson;
        // Note that SHA-256 is currently hardcoded in Chromium and Firefox.
        HashAlgId = ApiConstants.HashAlgorithmSha256;
    }

    public ClientData(string clientDataJson)
    {
        ClientDataJson = clientDataJson;
        // Note that SHA-256 is currently hardcoded in Chromium and Firefox.
        HashAlgId = ApiConstants.HashAlgorithmSha256;
    }

    public void Dispose()
    {
        if (_clientData != null)
        {
            _clientData.Dispose();
            _clientData = null;
        }
    }
}
