namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.StructVersions;

/// <summary>
/// Hybrid Storage Linked Data Version Information.
/// </summary>
public enum HybridStorageLinkedDataVersion : uint
{
    /// <remarks>
    /// Corresponds to CTAPCBOR_HYBRID_STORAGE_LINKED_DATA_VERSION_1.
    /// </remarks>
    Version1 = PInvoke.CTAPCBOR_HYBRID_STORAGE_LINKED_DATA_VERSION_1,

    /// <summary>
    /// Current version
    /// </summary>
    /// <remarks>
    /// Corresponds to CTAPCBOR_HYBRID_STORAGE_LINKED_DATA_CURRENT_VERSION.
    /// </remarks>
    Current = PInvoke.CTAPCBOR_HYBRID_STORAGE_LINKED_DATA_CURRENT_VERSION
}
