using System.Runtime.InteropServices;

namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.Structs;

/// <summary>
/// Information about credential list with extra information.
/// </summary>
/// <remarks>
/// Corresponds to WEBAUTHN_CREDENTIAL_LIST.
/// Contain an array of pointers to target structures.
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public sealed class CredentialList : IDisposable
{
    private int _length;
    private nint _nativeArray = nint.Zero;

    public CredentialList(CredentialEx[] credentials) : base()
    {
        if ((credentials?.Length ?? 0) <= 0)
        {
            // Nothing to initialize
            return;
        }

        _length = credentials.Length;

        // Allocate memory (We save pointers to the beginning of the array, followed by the items themselves.)
        int itemSize = Marshal.SizeOf<CredentialEx>();
        int arraySize = _length * (itemSize + nint.Size);
        _nativeArray = Marshal.AllocHGlobal(arraySize);

        // Copy items
        nint itemsStart = _nativeArray + _length * nint.Size;
        for (int i = 0; i < _length; i++)
        {
            // Marshal item
            nint itemPosition = itemsStart + itemSize * i;
            Marshal.StructureToPtr(credentials[i], itemPosition, false);

            // Marshal item pointer
            nint pointerPosition = _nativeArray + nint.Size * i;
            Marshal.WriteIntPtr(pointerPosition, itemPosition);
        }
    }

    public void Dispose()
    {
        _length = 0;

        if (_nativeArray != nint.Zero)
        {
            Marshal.FreeHGlobal(_nativeArray);
            _nativeArray = nint.Zero;
        }
    }
}
