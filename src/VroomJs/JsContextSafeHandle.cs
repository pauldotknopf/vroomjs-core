using System;
using System.Runtime.InteropServices;

namespace VroomJs
{
    public class JsContextSafeHandle : SafeHandle
    {
        public JsContextSafeHandle(JsEngineSafeHandle engine, int id)
            :base(IntPtr.Zero, true)
        {
            SetHandle(Native.jscontext_new(id, engine));
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
            if (!IsInvalid)
            {
                IntPtr h = handle;
                SetHandle(IntPtr.Zero);
                Native.jscontext_dispose(h);
            }
            return true;
        }
    }
}
