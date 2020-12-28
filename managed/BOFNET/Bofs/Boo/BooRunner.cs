using BOFNET;
using BOFNET.Bofs.Boo;
using System;
using System.Linq;
using System.Reflection;

namespace BOFNET.Bofs.Boo {
    public class BooRunner : BeaconObject {

        public BooRunner(BeaconApi api) : base(api) {
        }

        private static Random random = new Random();
        public static string RandomString(int length) {
            const string chars = "abcdefghijklmnopqrstuvwxysABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public override void Go(string[] args) {

            if(args.Length == 0) {
                BeaconConsole.WriteLine("[!] No source code given to run, provide your BooLang code as the first and only argument");
                return;
            }

            string temporaryAppDomainName = RandomString(10);
            AppDomain temporaryAppDomain = null;
            Runtime.AssemblyInfo runtimeAssembly = null;

            foreach (var assembly in Runtime.LoadedAssemblies) {
                temporaryAppDomain = LoadAssembyInAppDomain(temporaryAppDomainName, assembly.Value.AssemblyData, assembly.Value.AssemblyData.Length);
                if (assembly.Key == "BOFNET") {
                    runtimeAssembly = assembly.Value;
                }
            }

            try {

                if (temporaryAppDomain == null) {
                    BeaconConsole.WriteLine("[!] Failed to setup temporary AppDomain for Boo exection");
                    return;
                }

                foreach (Assembly assembly in temporaryAppDomain.GetAssemblies()) {
                    if (assembly.GetName().Name == "BOFNET") {
                        Type runtime = assembly.GetType("BOFNET.Runtime");
                        runtime.GetMethod("SetupAssemblyResolver", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
                        runtime.GetMethod("RegisterRuntimeAssembly", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { runtimeAssembly.AssemblyData });
                    }
                }

                BooExecutor booExecutor = temporaryAppDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName,
                                                                                      "BOFNET.Bofs.Boo.BooExecutorImpl") as BooExecutor;

                booExecutor.Execute(args[0]);

            } finally {
                if(temporaryAppDomain != null)
                    AppDomain.Unload(temporaryAppDomain);
            }
        }
    }
}
