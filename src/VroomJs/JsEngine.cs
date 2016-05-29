using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace VroomJs {
	public class JsEngine : IDisposable {

		
		
		// Make sure the delegates we pass to the C++ engine won't fly away during a GC.
		readonly Native.KeepaliveRemoveDelegate _keepalive_remove;
		readonly Native.KeepAliveGetPropertyValueDelegate _keepalive_get_property_value;
		readonly Native.KeepAliveSetPropertyValueDelegate _keepalive_set_property_value;
		readonly Native.KeepAliveValueOfDelegate _keepalive_valueof;
		readonly Native.KeepAliveInvokeDelegate _keepalive_invoke;
		readonly Native.KeepAliveDeletePropertyDelegate _keepalive_delete_property;
		readonly Native.KeepAliveEnumeratePropertiesDelegate _keepalive_enumerate_properties;

		//private readonly Dictionary<int, JsContext> _aliveContexts = new Dictionary<int, JsContext>();
		//private readonly Dictionary<int, JsScript> _aliveScripts = new Dictionary<int, JsScript>();

		private int _currentContextId = 0;
		private int _currentScriptId = 0;

		public static void DumpAllocatedItems() {
            Native.js_dump_allocated_items();
		}

		static JsEngine() {
            Native.js_set_object_marshal_type(JsObjectMarshalType.Dynamic);
		}

		readonly JsEngineSafeHandle _engine;

		public JsEngine(int maxYoungSpace = -1, int maxOldSpace = -1) {
			_keepalive_remove = new Native.KeepaliveRemoveDelegate(KeepAliveRemove);
			_keepalive_get_property_value = new Native.KeepAliveGetPropertyValueDelegate(KeepAliveGetPropertyValue);
			_keepalive_set_property_value = new Native.KeepAliveSetPropertyValueDelegate(KeepAliveSetPropertyValue);
			_keepalive_valueof = new Native.KeepAliveValueOfDelegate(KeepAliveValueOf);
			_keepalive_invoke = new Native.KeepAliveInvokeDelegate(KeepAliveInvoke);
			_keepalive_delete_property = new Native.KeepAliveDeletePropertyDelegate(KeepAliveDeleteProperty);
			_keepalive_enumerate_properties = new Native.KeepAliveEnumeratePropertiesDelegate(KeepAliveEnumerateProperties);
            
            _engine = new JsEngineSafeHandle(Native.jsengine_new(
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
            Native.jsengine_terminate_execution(_engine);
		}

		public void DumpHeapStats() {
            Native.jsengine_dump_heap_stats(_engine);
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
