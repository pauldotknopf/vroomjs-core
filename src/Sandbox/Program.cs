
namespace Sandbox
{
    public class Program
    {
        public static void Main(string[] args)
        {
            VroomJs.AssemblyLoader.EnsureLoaded();

            while (true)
            {
                using (var engine = new VroomJs.JsEngine())
                {
                    engine.DumpHeapStats();
                }
            }
        }
    }
}
