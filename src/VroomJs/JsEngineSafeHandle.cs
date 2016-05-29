using System;
using System.Runtime.InteropServices;

namespace VroomJs
{
    public class JsEngineSafeHandle : SafeHandle
    {
        public JsEngineSafeHandle(IntPtr engine)
            :base(IntPtr.Zero, true)
        {
            SetHandle(engine);
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
            if(!IsInvalid)
            {
                IntPtr h = handle;
                SetHandle(IntPtr.Zero);
                Native.jsengine_dispose(h);
                return true;
            }
            return false;
        }
    }
}
