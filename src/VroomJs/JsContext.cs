using System;

namespace VroomJs
{
    public partial class JsContext : IDisposable
    {
        private bool _disposed;
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

        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(JsEngine));
        }

        ~JsContext()
        {
            if (!_disposed)
                Dispose(false);
        }

        public void Dispose()
        {
            CheckDisposed();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            CheckDisposed();
            
            _disposed = true;

            if (disposing)
            {
                _notifyDispose(_id);
                _context.Dispose();
            }
        }

        #endregion
    }
}