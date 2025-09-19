using System.Runtime.InteropServices;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Marshalers;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.StructVersions;

namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.Structs;

/// <summary>
/// Information about linked devices
/// </summary>
/// <remarks>Corresponds to CTAPCBOR_HYBRID_STORAGE_LINKED_DATA.</remarks>
[StructLayout(LayoutKind.Sequential)]
public sealed class HybridStorageLinkedData : IDisposable
{
    /// <summary>
    /// // Version of this structure, to allow for modifications in the future.
    /// </summary>
    private HybridStorageLinkedDataVersion _version = HybridStorageLinkedDataVersion.Version1;

    // Contact Id
    private int _contactIdLength;
    private ByteArrayIn _contactId;

    // Link Id
    private int _linkIdLength;
    private ByteArrayIn _linkId;

    // Link secret
    private int _linkSecretLength;
    private ByteArrayIn _linkSecret;

    // Authenticator Public Key
    private int _publicKeyLength;
    private ByteArrayIn _publicKey;

    /// <summary>
    /// Authenticator Name
    /// </summary>
    public string AuthenticatorName { get; set; }

    /// <summary>
    /// Tunnel server domain
    /// </summary>
    public short EncodedTunnelServerDomain { get; set; }

    /// <summary>
    /// Contact Id
    /// </summary>
    public byte[] ContactId
    {
        get
        {
            return _contactId?.Read(_contactIdLength);
        }
        set
        {
            _contactId?.Dispose();
            _contactIdLength = value?.Length ?? 0;
            _contactId = new ByteArrayIn(value);
        }
    }

    /// <summary>
    /// Link Id
    /// </summary>
    public byte[] LinkId
    {
        get
        {
            return _linkId?.Read(_linkIdLength);
        }
        set
        {
            _linkId?.Dispose();
            _linkIdLength = value?.Length ?? 0;
            _linkId = new ByteArrayIn(value);
        }
    }

    /// <summary>
    /// Link secret
    /// </summary>
    public byte[] LinkSecret
    {
        get
        {
            return _linkSecret?.Read(_linkSecretLength);
        }
        set
        {
            _linkSecret?.Dispose();
            _linkSecretLength = value?.Length ?? 0;
            _linkSecret = new ByteArrayIn(value);
        }
    }

    /// <summary>
    /// Authenticator Public Key
    /// </summary>
    public byte[] PublicKey
    {
        get
        {
            return _publicKey?.Read(_publicKeyLength);
        }
        set
        {
            _publicKey?.Dispose();
            _publicKeyLength = value?.Length ?? 0;
            _publicKey = new ByteArrayIn(value);
        }
    }

    /// <summary>
    /// Deallocates unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        _contactId?.Dispose();
        _contactId = null;

        _linkId?.Dispose();
        _linkId = null;

        _linkSecret?.Dispose();
        _linkSecret = null;

        _publicKey?.Dispose();
        _publicKey = null;
    }
}
