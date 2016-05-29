using System;
using System.Runtime.InteropServices;

namespace VroomJs
{
    internal class Native
    {
        public delegate void KeepaliveRemoveDelegate(int context, int slot);
        public delegate JsValue KeepAliveGetPropertyValueDelegate(int context, int slot, [MarshalAs(UnmanagedType.LPWStr)] string name);
        public delegate JsValue KeepAliveSetPropertyValueDelegate(int context, int slot, [MarshalAs(UnmanagedType.LPWStr)] string name, JsValue value);
        public delegate JsValue KeepAliveValueOfDelegate(int context, int slot);
        public delegate JsValue KeepAliveInvokeDelegate(int context, int slot, JsValue args);
        public delegate JsValue KeepAliveDeletePropertyDelegate(int context, int slot, [MarshalAs(UnmanagedType.LPWStr)] string name);
        public delegate JsValue KeepAliveEnumeratePropertiesDelegate(int context, int slot);

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
        public static extern void js_set_object_marshal_type(JsObjectMarshalType objectMarshalType);

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
        public static extern void js_dump_allocated_items();

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr jsengine_new(
            KeepaliveRemoveDelegate keepaliveRemove,
            KeepAliveGetPropertyValueDelegate keepaliveGetPropertyValue,
            KeepAliveSetPropertyValueDelegate keepaliveSetPropertyValue,
            KeepAliveValueOfDelegate keepaliveValueOf,
            KeepAliveInvokeDelegate keepaliveInvoke,
            KeepAliveDeletePropertyDelegate keepaliveDeleteProperty,
            KeepAliveEnumeratePropertiesDelegate keepaliveEnumerateProperties,
            int maxYoungSpace, int maxOldSpace
        );

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
        public static extern void jsengine_terminate_execution(JsEngineSafeHandle engine);

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
        public static extern void jsengine_dump_heap_stats(JsEngineSafeHandle engine);

        [DllImport("VroomJsNative")]
        public static extern void jsengine_dispose_object(JsEngineSafeHandle engine, IntPtr obj);

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
        public static extern void jsengine_dispose(IntPtr engine);
    }
}
