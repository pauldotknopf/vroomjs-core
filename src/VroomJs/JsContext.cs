using System;

namespace VroomJs
{
    public partial class JsContext : IDisposable
    {
        private readonly int _id;
        private readonly JsEngine _engine;
        private readonly JsContextSafeHandle _context;
        private readonly Action<int> _notifyDispose;
        readonly IKeepAliveStore _keepalives;
        
        internal JsContext(int id, JsEngine engine, JsEngineSafeHandle engineHandle, Action<int> notifyDispose)
        {
            _id = id;
            _engine = engine;

            _keepalives = new KeepAliveDictionaryStore();
            _context = new JsContextSafeHandle(engineHandle, id);

            _notifyDispose = notifyDispose;
        }
        
        #region IDisposable implementation
        
        public void Dispose()
        {
            _notifyDispose(_id);
            _context.Dispose();
        }
        
        #endregion
    }
}