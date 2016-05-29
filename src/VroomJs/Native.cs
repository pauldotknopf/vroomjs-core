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
        internal static extern void js_set_object_marshal_type(JsObjectMarshalType objectMarshalType);

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
        internal static extern void js_dump_allocated_items();

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr jsengine_new(
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
        internal static extern void jsengine_terminate_execution(JsEngineSafeHandle engine);

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
        internal static extern void jsengine_dump_heap_stats(JsEngineSafeHandle engine);

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
        internal static extern void jsengine_dispose(IntPtr engine);

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
        internal static extern void jsengine_dispose_object(JsEngineSafeHandle engine, IntPtr obj);

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr jscontext_new(int id, JsEngineSafeHandle engine);

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
        internal static extern void jscontext_dispose(IntPtr context);

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
        internal static extern void jscontext_force_gc();

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        internal static extern JsValue jscontext_execute(JsContextSafeHandle context, [MarshalAs(UnmanagedType.LPWStr)] string str, [MarshalAs(UnmanagedType.LPWStr)] string name);

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        internal static extern JsValue jscontext_execute_script(JsContextSafeHandle context, JsScriptSafeHandle script);

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
        internal static extern JsValue jscontext_get_global(JsContextSafeHandle context);

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
        internal static extern JsValue jscontext_get_variable(JsContextSafeHandle context, [MarshalAs(UnmanagedType.LPWStr)] string name);

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
        internal static extern JsValue jscontext_set_variable(JsContextSafeHandle context, [MarshalAs(UnmanagedType.LPWStr)] string name, JsValue value);

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
        internal static extern JsValue jsvalue_alloc_string([MarshalAs(UnmanagedType.LPWStr)] string str);

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
        internal static extern JsValue jsvalue_alloc_array(int length);

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
        internal static extern void jsvalue_dispose(JsValue value);

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
        internal static extern JsValue jscontext_invoke(JsContextSafeHandle context, IntPtr funcPtr, IntPtr thisPtr, JsValue args);

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr jsscript_new(JsEngineSafeHandle engine);

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        internal static extern JsValue jsscript_compile(JsScriptSafeHandle script, [MarshalAs(UnmanagedType.LPWStr)] string str, [MarshalAs(UnmanagedType.LPWStr)] string name);

        [DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
        internal static extern IntPtr jsscript_dispose(JsScriptSafeHandle script);
    }
}
