using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace VroomJs {
	public class JsEngine : IDisposable {

		delegate void KeepaliveRemoveDelegate(int context, int slot);
		delegate JsValue KeepAliveGetPropertyValueDelegate(int context, int slot, [MarshalAs(UnmanagedType.LPWStr)] string name);
		delegate JsValue KeepAliveSetPropertyValueDelegate(int context, int slot, [MarshalAs(UnmanagedType.LPWStr)] string name, JsValue value);
		delegate JsValue KeepAliveValueOfDelegate(int context, int slot);
		delegate JsValue KeepAliveInvokeDelegate(int context, int slot, JsValue args);
		delegate JsValue KeepAliveDeletePropertyDelegate(int context, int slot, [MarshalAs(UnmanagedType.LPWStr)] string name);
		delegate JsValue KeepAliveEnumeratePropertiesDelegate(int context, int slot);

		[DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
		static extern void js_set_object_marshal_type(JsObjectMarshalType objectMarshalType);

		[DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
		static extern void js_dump_allocated_items();

		[DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
		static extern IntPtr jsengine_new(
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
		static extern void jsengine_terminate_execution(JsEngineSafeHandle engine);
			
		[DllImport("VroomJsNative", CallingConvention = CallingConvention.StdCall)]
		static extern void jsengine_dump_heap_stats(JsEngineSafeHandle engine);
        
		[DllImport("VroomJsNative")]
		static extern void jsengine_dispose_object(JsEngineSafeHandle engine, IntPtr obj);
		
		// Make sure the delegates we pass to the C++ engine won't fly away during a GC.
		readonly KeepaliveRemoveDelegate _keepalive_remove;
		readonly KeepAliveGetPropertyValueDelegate _keepalive_get_property_value;
		readonly KeepAliveSetPropertyValueDelegate _keepalive_set_property_value;
		readonly KeepAliveValueOfDelegate _keepalive_valueof;
		readonly KeepAliveInvokeDelegate _keepalive_invoke;
		readonly KeepAliveDeletePropertyDelegate _keepalive_delete_property;
		readonly KeepAliveEnumeratePropertiesDelegate _keepalive_enumerate_properties;

		//private readonly Dictionary<int, JsContext> _aliveContexts = new Dictionary<int, JsContext>();
		//private readonly Dictionary<int, JsScript> _aliveScripts = new Dictionary<int, JsScript>();

		private int _currentContextId = 0;
		private int _currentScriptId = 0;

		public static void DumpAllocatedItems() {
			js_dump_allocated_items();
		}

		static JsEngine() {
			js_set_object_marshal_type(JsObjectMarshalType.Dynamic);
		}

		readonly JsEngineSafeHandle _engine;

		public JsEngine(int maxYoungSpace = -1, int maxOldSpace = -1) {
			_keepalive_remove = new KeepaliveRemoveDelegate(KeepAliveRemove);
			_keepalive_get_property_value = new KeepAliveGetPropertyValueDelegate(KeepAliveGetPropertyValue);
			_keepalive_set_property_value = new KeepAliveSetPropertyValueDelegate(KeepAliveSetPropertyValue);
			_keepalive_valueof = new KeepAliveValueOfDelegate(KeepAliveValueOf);
			_keepalive_invoke = new KeepAliveInvokeDelegate(KeepAliveInvoke);
			_keepalive_delete_property = new KeepAliveDeletePropertyDelegate(KeepAliveDeleteProperty);
			_keepalive_enumerate_properties = new KeepAliveEnumeratePropertiesDelegate(KeepAliveEnumerateProperties);
            
            _engine = new JsEngineSafeHandle(jsengine_new(
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

        public void TerminateExecution() {
			jsengine_terminate_execution(_engine);
		}

		public void DumpHeapStats() {
			jsengine_dump_heap_stats(_engine);
		}

		private JsValue KeepAliveValueOf(int contextId, int slot) {
            return JsValue.Null;
        }
		
		private JsValue KeepAliveInvoke(int contextId, int slot, JsValue args) {
            return JsValue.Null;
        }

		private JsValue KeepAliveSetPropertyValue(int contextId, int slot, string name, JsValue value) {
            return JsValue.Null;
        }

		
		private JsValue KeepAliveGetPropertyValue(int contextId, int slot, string name) {
            return JsValue.Null;
        }

		private JsValue KeepAliveDeleteProperty(int contextId, int slot, string name) {
            return JsValue.Null;
        }

		private JsValue KeepAliveEnumerateProperties(int contextId, int slot) {
            return JsValue.Null;
		}

		private void KeepAliveRemove(int contextId, int slot) {

		}
        
		#region IDisposable implementation

        public void Dispose()
        {
            _engine.Dispose();
        }
        
        #endregion
	}
}
