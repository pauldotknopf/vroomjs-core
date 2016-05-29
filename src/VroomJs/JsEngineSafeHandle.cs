using System;
using System.Runtime.InteropServices;

namespace VroomJs
{
    delegate void KeepaliveRemoveDelegate(int context, int slot);
    delegate JsValue KeepAliveGetPropertyValueDelegate(int context, int slot, [MarshalAs(UnmanagedType.LPWStr)] string name);
    delegate JsValue KeepAliveSetPropertyValueDelegate(int context, int slot, [MarshalAs(UnmanagedType.LPWStr)] string name, JsValue value);
    delegate JsValue KeepAliveValueOfDelegate(int context, int slot);
    delegate JsValue KeepAliveInvokeDelegate(int context, int slot, JsValue args);
    delegate JsValue KeepAliveDeletePropertyDelegate(int context, int slot, [MarshalAs(UnmanagedType.LPWStr)] string name);
    delegate JsValue KeepAliveEnumeratePropertiesDelegate(int context, int slot);

    public class JsEngineSafeHandle : SafeHandle
    {
        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
        static extern void jsengine_dispose(IntPtr engine);

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
                jsengine_dispose(h);
                return true;
            }
            return false;
        }
    }
}
