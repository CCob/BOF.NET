using System;
using System.IO;
using System.Runtime.InteropServices;

namespace BOFNET {

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool BeaconUseToken(IntPtr handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool BeaconRevertToken();


    public interface BeaconApi {
        BeaconUseToken BeaconUseToken { get; }
        BeaconRevertToken BeaconRevertToken { get; }
        TextWriter Console { get; }
        Runtime.LoadAssembyInAppDomainDelegate LoadAssemblyInAppDomain { get; }
    }
}
