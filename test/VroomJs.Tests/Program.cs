using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VroomJs.Tests
{
    public class Program
    {
        IServiceProvider _serviceProvider;

        public Program(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Main(string[] args)
        {
            VroomJs.AssemblyLoader.EnsureLoaded();
            new Xunit.Runner.Dnx.Program(_serviceProvider).Main(args);
        }
    }
}
