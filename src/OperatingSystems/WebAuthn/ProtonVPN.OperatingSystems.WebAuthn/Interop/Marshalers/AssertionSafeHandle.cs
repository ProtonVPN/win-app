using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using ProtonVPN.OperatingSystems.WebAuthn.Interop;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Structs.GetAssertion;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.Structs.PreviousVersions;
using ProtonVPN.OperatingSystems.WebAuthn.Interop.StructVersions;

namespace ProtonVPN.OperatingSystems.WebAuthn.Interop.Marshalers;

public sealed class AssertionSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private AssertionSafeHandle() : base(true) { }

    protected override bool ReleaseHandle()
    {
        NativeMethods.FreeAssertion(handle);
        return true;
    }

    public Assertion ToManaged()
    {
        if (IsInvalid)
        {
            return null;
        }

        // Handle possible older structure versions
        AssertionVersion version = (AssertionVersion)Marshal.ReadInt32(handle);
        int sourceStructSize;

        switch (version)
        {
            case AssertionVersion.Version1:
                sourceStructSize = Marshal.SizeOf<AssertionV1>();
                break;
            case AssertionVersion.Version2:
                sourceStructSize = Marshal.SizeOf<AssertionV2>();
                break;
            case AssertionVersion.Version3:
                sourceStructSize = Marshal.SizeOf<AssertionV3>();
                break;
            case AssertionVersion.Version4:
                sourceStructSize = Marshal.SizeOf<AssertionV4>();
                break;
            case AssertionVersion.Version5:
            default:
                sourceStructSize = Marshal.SizeOf<Assertion>();
                break;
        }

        return VersionedStructMarshaler.PtrToStructure<Assertion>(handle, sourceStructSize);
    }
}
