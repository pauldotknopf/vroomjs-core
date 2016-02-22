using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace VroomJs
{
    public static class AssemblyLoader
    {
        public static object _lock = new object();
        public static bool _isLoaded = false;

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibrary(string lpFileName);

        private static void LoadDll(string dllName, string architecture)
        {
            var dirName = Path.Combine(Path.GetTempPath(), "VroomJs." + typeof(AssemblyLoader).Assembly.GetName().Version.ToString());

            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);

            dirName = Path.Combine(dirName, architecture);

            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);

            var dllPath = Path.Combine(dirName, dllName + ".dll");

            using (Stream stm = typeof(JsContext).Assembly.GetManifestResourceStream("VroomJs." + dllName + "-" + architecture + ".dll"))
            {
                try
                {
                    using (Stream outFile = File.Create(dllPath))
                    {
                        const int sz = 4096;
                        byte[] buf = new byte[sz];
                        while (true)
                        {
                            int nRead = stm.Read(buf, 0, sz);
                            if (nRead < 1)
                                break;
                            outFile.Write(buf, 0, nRead);
                        }
                    }
                }
                catch
                {
                    // This may happen if another process has already created and loaded the file.
                    // Since the directory includes the version number of this assembly we can
                    // assume that it's the same bits, so we just ignore the excecption here and
                    // load the DLL.
                }
            }
            
            IntPtr h = LoadLibrary(dllPath);
            if (h == IntPtr.Zero)
                throw new Exception("Couldn't load native assembly at " + dllPath);
        }

        public static void EnsureLoaded()
        {
            if (_isLoaded) return;
            lock (_lock)
            {
                if (_isLoaded) return;
                _isLoaded = true;

                LoadDll("v8", "x86");
                LoadDll("VroomJsNative", "x86");
            }
        }
    }
}
