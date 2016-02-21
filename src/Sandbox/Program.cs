using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VroomJs;

namespace Sandbox
{
    public class debugtest
    {
        public int TestProp { get; set; }

        public static bool BoolTest(int a, int b)
        {
            return a == b;
        }

        public void Write(object v)
        {
            Console.WriteLine(v);
        }

        public void RunFunc(Func<int, string> callback)
        {
            for (int i = 1; i <= 3; i++)
            {
                string data = callback(i);
                Console.WriteLine(data);
            }
        }

        public double ValueOf()
        {
            return 31.7777;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            while (true)
            {
                using (JsEngine js = new JsEngine(4, 32))
                {
                    using (JsContext context = js.CreateContext())
                    {
                        context.SetVariable("Debug", typeof(debugtest));
                        object result = context.Execute("Debug.BoolTest(3,4);");

                    }
                    GC.Collect();
                    js.DumpHeapStats();
                }
            }

            Console.ReadLine();
        }
    }
}
