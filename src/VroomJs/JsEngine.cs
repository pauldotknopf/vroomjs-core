using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace VroomJs {
	public class JsEngine : IDisposable {

		private readonly Dictionary<int, JsContext> _aliveContexts = new Dictionary<int, JsContext>();
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
            _engine = new JsEngineSafeHandle(KeepAliveRemove,
                KeepAliveGetPropertyValue,
                KeepAliveSetPropertyValue,
                KeepAliveValueOf,
                KeepAliveInvoke,
                KeepAliveDeleteProperty,
                KeepAliveEnumerateProperties,
                maxYoungSpace,
                maxOldSpace);
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

        public JsContext CreateContext()
        {
            int id = Interlocked.Increment(ref _currentContextId);
            JsContext ctx = new JsContext(id, this, _engine, ContextDisposed);
            _aliveContexts.Add(id, ctx);
            return ctx;
        }

        private void ContextDisposed(int id)
        {
            _aliveContexts.Remove(id);
        }

        #region IDisposable implementation

        public void Dispose()
        {
            _engine.Dispose();
        }
        
        #endregion
	}
}
