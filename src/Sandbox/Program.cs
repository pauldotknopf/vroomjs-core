
using System;

namespace Sandbox
{
    public class Program
    {
        public static void Main(string[] args)
        {
            VroomJs.AssemblyLoader.EnsureLoaded();

            while (true)
            {
                GC.Collect(GC.MaxGeneration);
                using (var engine = new VroomJs.JsEngine())
                {
                    using (var context = engine.CreateContext())
                    {
                        var r = context.Execute("345 + 3");
                        var result = context.GetVariable("test");
                        engine.DumpHeapStats();
                    }
                    engine.DumpHeapStats();
                }
            }
        }
    }
}
