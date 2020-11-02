using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace BOFNET {
    public class Runtime {

        public static Dictionary<string, Assembly> LoadedAssemblies { get; private set; } = new Dictionary<string, Assembly>();
                            
        static bool firstInit = true;

        [DllImport("shell32.dll", SetLastError = true)]
        static extern IntPtr CommandLineToArgvW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        private static Type FindType(string name) {
            var results = AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(s => s.GetTypes())
               .Where(p => p.Name == name || p.FullName == name);

            if(results.Count() > 1) {
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

        public static void LoadAssembly(byte[] assemblyData) {
            Assembly assembly = AppDomain.CurrentDomain.Load(assemblyData);
            AssemblyName assemblyName = new AssemblyName(assembly.FullName);
            LoadedAssemblies[assemblyName.Name] = assembly;
        }
  
        public static void InvokeBof(long consoleCallback, string bofName, object args) {

            if (firstInit) {
                AppDomain.CurrentDomain.AssemblyResolve += Runtime.CurrentDomain_AssemblyResolve;
                LoadedAssemblies["BOFNET"] = Assembly.GetExecutingAssembly();
                firstInit = false;
            }

            BeaconConsoleWriter.BeaconConsoleWriterDelegate nativeDelagte =
                (BeaconConsoleWriter.BeaconConsoleWriterDelegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(consoleCallback),
                                                                                                       typeof(BeaconConsoleWriter.BeaconConsoleWriterDelegate));
            using (BeaconConsoleWriter bcw = new BeaconConsoleWriter(nativeDelagte)) {

                Type bofType;

                if (String.IsNullOrEmpty(bofName)) {
                    bcw.WriteLine($"[!] BOF name not supplied, don't know what to execute, bailing!");
                    return;
                }

                try {
                    bofType = FindType(bofName);
                } catch (AmbiguousMatchException) {
                    bcw.WriteLine($"[!] Multiple BOFs found with name {bofName}, use fully qualifed type including namespace");
                    return;
                }
                
                if (bofType == null) {
                    bcw.WriteLine($"[!] Failed to find type {bofName} within BOFNET AppDomain, have you loaded the containing assembly yet?");
                    return;
                }

                BeaconObject bo = (BeaconObject)Activator.CreateInstance(bofType, new object[] { new DefaultBeaconApi(bcw) });

                try {

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
                }catch(Exception e) {
                    bcw.WriteLine($"[!] BOFNET executed but threw an unhandled exception: {e}");
                }
            }                                                           
        }

        public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) {

            AssemblyName assemblyName = new AssemblyName(args.Name);
            if (LoadedAssemblies.ContainsKey(assemblyName.Name)) {
                return LoadedAssemblies[assemblyName.Name];
            } else {
                return null;
            }           
        }
    }
}
