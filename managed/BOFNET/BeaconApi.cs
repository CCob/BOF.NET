using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace BOFNET {

    public enum OutputTypes : int {
        CALLBACK_OUTPUT = 0,
        CALLBACK_KEYSTROKES = 1,
        CALLBACK_FILE = 2,
        CALLBACK_SCREENSHOT = 3,
        CALLBACK_CLOSE = 4,
        CALLBACK_READ = 5,
        CALLBACK_CONNECT = 6,
        CALLBACK_PING = 7,
        CALLBACK_FILE_WRITE = 8,
        CALLBACK_FILE_CLOSE = 9,
        CALLBACK_PIPE_OPEN = 10,
        CALLBACK_PIPE_CLOSE = 11,
        CALLBACK_PIPE_READ = 12,
        CALLBACK_POST_ERROR = 13,
        CALLBACK_PIPE_PING = 14,
        CALLBACK_TOKEN_STOLEN = 15,
        CALLBACK_TOKEN_GETUID = 16,
        CALLBACK_PROCESS_LIST = 17,
        CALLBACK_POST_REPLAY_ERROR = 18,
        CALLBACK_PWD = 19,
        CALLBACK_JOBS = 20,
        CALLBACK_HASHDUMP = 21,
        CALLBACK_PENDING = 22,
        CALLBACK_ACCEPT = 23,
        CALLBACK_NETVIEW = 24,
        CALLBACK_PORTSCAN = 25,
        CALLBACK_DEAD = 26,
        CALLBACK_SSH_STATUS = 27,
        CALLBACK_CHUNK_ALLOCATE = 28,
        CALLBACK_CHUNK_SEND = 29,
        CALLBACK_OUTPUT_OEM = 30,
        CALLBACK_ERROR = 31,
        CALLBACK_OUTPUT_UTF8 = 32
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool BeaconUseToken(IntPtr handle);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool BeaconRevertToken();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
    public delegate bool BeaconGetSpawnTo(bool x86, out StringBuilder path, int maxLen);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate bool BeaconInjectProcess(IntPtr processHandle, int pid, byte[] payload, int paylad_len, int payload_offset, byte[] arg, int arg_len);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void BeaconCallbackWriter(OutputTypes type, [MarshalAs(UnmanagedType.LPArray)] byte[] data, int len);

    public interface BeaconApi {
        BeaconUseToken BeaconUseToken { get; }
        BeaconRevertToken BeaconRevertToken { get; }
        TextWriter Console { get; }
        Runtime.InitialiseChildBOFNETAppDomain InitialiseChildBOFNETAppDomain { get; }
        BeaconCallbackWriter BeaconCallbackWriter { get; }
    }
}
