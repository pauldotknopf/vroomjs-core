using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace VroomJs
{
    public class JsScriptSafeHandle : SafeHandle
    {
        public JsScriptSafeHandle()
            :base(IntPtr.Zero, true)
        {
        }

        public override bool IsInvalid
        {
            get
            {
                return handle == IntPtr.Zero;
            }
        }

        protected override bool ReleaseHandle()
        {
            return true;
        }
    }
}
