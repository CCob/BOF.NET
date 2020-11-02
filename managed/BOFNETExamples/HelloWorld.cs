using BOFNET;

namespace BOFNETExamples {
    public class HelloWorld : BeaconObject {
        public HelloWorld(BeaconApi api) : base(api) {}

        public override void Go(string[] args) {
            BeaconConsole.WriteLine($"[+] Hello from BOF.NET. Arguments:");
            foreach(string arg in args) {
                BeaconConsole.WriteLine($"{arg}");
            }
        }
    }
}
