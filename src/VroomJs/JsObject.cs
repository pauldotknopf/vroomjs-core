using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace VroomJs
{
    public class JsObject : DynamicObject, IDisposable
    {
        public JsObject(JsContext context, IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                throw new ArgumentException("can't wrap an empty object (ptr is Zero)", "ptr");

            _context = context;
            _handle = ptr;
        }

        readonly JsContext _context;
        readonly IntPtr _handle;

        public IntPtr Handle
        {
            get { return _handle; }
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = _context.InvokeProperty(this, binder.Name, args);
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = _context.GetPropertyValue(this, binder.Name);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _context.SetPropertyValue(this, binder.Name, value);
            return true;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _context.GetMemberNames(this);
        }

        #region IDisposable implementation

        bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                throw new ObjectDisposedException("JsObject:" + _handle);

            _disposed = true;

            _context.Engine.DisposeObject(this.Handle);
        }

        ~JsObject()
        {
            if (!_disposed)
                Dispose(false);
        }

        #endregion
    }
}
