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
                        var global = (dynamic)context.GetGlobal();
                        global.test = true;
                        object result = context.Execute("test");
                    }
                    GC.Collect();
                    js.DumpHeapStats();
                }
            }
        }
    }
}
