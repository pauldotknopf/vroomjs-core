using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using VroomJs;

namespace Sandbox
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AssemblyLoader.EnsureLoaded();
            
            while (true)
            {
                using (JsEngine js = new JsEngine(4, 32))
                {
                    using (JsContext context = js.CreateContext())
                    {
                        // Create a global variable on the JS side.
                        context.Execute("var x = {'answer':42, 'tellme':function (x) { return x+' '+this.answer; }}");
                        // Get it and use "dynamic" to tell the compiler to use runtime binding.
                        dynamic x = context.GetVariable("x");
                        // Call the method and print the result. This will print:
                        // "What is the answer to ...? 42"
                        Console.WriteLine(x.tellme("What is the answer to ...?"));
                    }
                    GC.Collect();
                    js.DumpHeapStats();
                }
            }
        }
    }
}
