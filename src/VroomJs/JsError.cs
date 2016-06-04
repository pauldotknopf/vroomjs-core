using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace VroomJs
{
    [StructLayout(LayoutKind.Sequential)]
    struct JsError
    {
        public JsValue Type;
        public int Line;
        public int Column;
        public JsValue Resource;
        public JsValue Message;
        public JsValue Exception;
    }
}
