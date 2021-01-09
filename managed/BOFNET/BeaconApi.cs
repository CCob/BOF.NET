using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace BOFNET {

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool BeaconUseToken(IntPtr handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool BeaconRevertToken();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
    public delegate bool BeaconGetSpawnTo(bool x86, out StringBuilder path, int maxLen);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate bool BeaconInjectProcess(IntPtr processHandle, int pid, byte[] payload, int paylad_len, int payload_offset, byte[] arg, int arg_len);

    public interface BeaconApi {
        BeaconUseToken BeaconUseToken { get; }
        BeaconRevertToken BeaconRevertToken { get; }
        TextWriter Console { get; }
        Runtime.InitialiseChildBOFNETAppDomain InitialiseChildBOFNETAppDomain { get; }
    }
}
