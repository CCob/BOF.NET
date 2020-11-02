namespace BOFNET.Bofs {
    public class HelloWorld : BeaconObject {
        public HelloWorld(BeaconApi api) : base(api) { }
        public override void Go(string[] args) {
            BeaconConsole.WriteLine($"[+] Welcome to BOFNET { (args.Length > 0 ? args[0] : "anonymous" )}");
        }
    }
}
