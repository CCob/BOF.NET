using System;
using System.Linq;

namespace BOFNET.Bofs {

    public class List : BeaconObject {
        public List(BeaconApi api) : base(api) {
        }

        public override void Go(string[] args) {  
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(BeaconObject).IsAssignableFrom(p) && !p.IsAbstract && !p.IsInterface)                
                .ToList()
                .ForEach(type => BeaconConsole.WriteLine($"{type.FullName}"));
        }
    }
}
