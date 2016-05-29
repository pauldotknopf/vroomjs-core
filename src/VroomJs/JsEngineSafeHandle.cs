using System;
using System.Runtime.InteropServices;

namespace VroomJs
{
    public class JsEngineSafeHandle : SafeHandle
    {
        readonly Native.KeepaliveRemoveDelegate _keepalive_remove;
        readonly Native.KeepAliveGetPropertyValueDelegate _keepalive_get_property_value;
        readonly Native.KeepAliveSetPropertyValueDelegate _keepalive_set_property_value;
        readonly Native.KeepAliveValueOfDelegate _keepalive_valueof;
        readonly Native.KeepAliveInvokeDelegate _keepalive_invoke;
        readonly Native.KeepAliveDeletePropertyDelegate _keepalive_delete_property;
        readonly Native.KeepAliveEnumeratePropertiesDelegate _keepalive_enumerate_properties;

        internal JsEngineSafeHandle(Native.KeepaliveRemoveDelegate keepaliveRemove,
            Native.KeepAliveGetPropertyValueDelegate keepaliveGetPropertyValue,
            Native.KeepAliveSetPropertyValueDelegate keepaliveSetPropertyValue,
            Native.KeepAliveValueOfDelegate keepaliveValueOf,
            Native.KeepAliveInvokeDelegate keepaliveInvoke,
            Native.KeepAliveDeletePropertyDelegate keepaliveDeleteProperty,
            Native.KeepAliveEnumeratePropertiesDelegate keepaliveEnumerateProperties,
            int maxYoungSpace, 
            int maxOldSpace)
            :base(IntPtr.Zero, true)
        {
            _keepalive_remove = keepaliveRemove;
            _keepalive_get_property_value = keepaliveGetPropertyValue;
            _keepalive_set_property_value = keepaliveSetPropertyValue;
            _keepalive_valueof = keepaliveValueOf;
            _keepalive_invoke = keepaliveInvoke;
            _keepalive_delete_property = keepaliveDeleteProperty;
            _keepalive_enumerate_properties = keepaliveEnumerateProperties;

            SetHandle(Native.jsengine_new(
                _keepalive_remove,
                _keepalive_get_property_value,
                _keepalive_set_property_value,
                _keepalive_valueof,
                _keepalive_invoke,
                _keepalive_delete_property,
                _keepalive_enumerate_properties,
                maxYoungSpace,
                maxOldSpace));
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
            }
            return true;
        }
    }
}
