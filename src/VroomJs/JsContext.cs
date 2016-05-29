using System;
using System.Collections;
using System.Linq;
using System.Reflection;

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
        readonly JsConvert _convert;

        internal JsContext(int id, JsEngine engine, JsEngineSafeHandle engineHandle, Action<int> notifyDispose)
        {
            _id = id;
            _engine = engine;

            _keepalives = new KeepAliveDictionaryStore();
            _context = new JsContextSafeHandle(engineHandle, id);
            _convert = new JsConvert(this);

            _notifyDispose = notifyDispose;
        }

        internal JsValue KeepAliveValueOf(int slot)
        {
            throw new NotImplementedException();
        }

        internal JsValue KeepAliveInvoke(int slot, JsValue args)
        {
            throw new NotImplementedException();
        }

        internal JsValue KeepAliveSetPropertyValue(int slot, string name, JsValue value)
        {
            throw new NotImplementedException();
        }

        internal JsValue KeepAliveGetPropertyValue(int slot, string name)
        {
            // we need to fall back to the prototype verison we set up because v8 won't call an object as a function, it needs
            // to be from a proper FunctionTemplate.
            if (!string.IsNullOrEmpty(name) && name.Equals("valueOf", StringComparison.CurrentCultureIgnoreCase))
            {
                return JsValue.Empty;
            }

            // TODO: This is pretty slow: use a cache of generated code to make it faster.
            var obj = KeepAliveGet(slot);
            if (obj != null)
            {
                Type type;
                if (obj is Type)
                {
                    type = (Type)obj;
                }
                else
                {
                    type = obj.GetType();
                }

                try
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        var upperCamelCase = Char.ToUpper(name[0]) + name.Substring(1);
                        JsValue value;
                        if (TryGetMemberValue(type, obj, upperCamelCase, out value))
                        {
                            return value;
                        }
                        if (TryGetMemberValue(type, obj, name, out value))
                        {
                            return value;
                        }
                    }

                    // Else an error.
                    return JsValue.Error(KeepAliveAdd(
                        new InvalidOperationException(String.Format("property not found on {0}: {1} ", type, name))));
                }
                catch (TargetInvocationException e)
                {
                    // Client code probably isn't interested in the exception part related to
                    // reflection, so we unwrap it and pass to V8 only the real exception thrown.
                    if (e.InnerException != null)
                        return JsValue.Error(KeepAliveAdd(e.InnerException));
                    throw;
                }
                catch (Exception e)
                {
                    return JsValue.Error(KeepAliveAdd(e));
                }
            }

            return JsValue.Error(KeepAliveAdd(new IndexOutOfRangeException("invalid keepalive slot: " + slot)));
        }

        internal JsValue KeepAliveDeleteProperty(int slot, string name)
        {
            // TODO: This is pretty slow: use a cache of generated code to make it faster.
            var obj = KeepAliveGet(slot);
            if (obj != null)
            {
                if (typeof(IDictionary).IsAssignableFrom(obj.GetType()))
                {
                    IDictionary dictionary = (IDictionary)obj;
                    if (dictionary.Contains(name))
                    {
                        dictionary.Remove(name);
                        return _convert.ToJsValue(true);
                    }
                }
                return _convert.ToJsValue(false);
            }
            return JsValue.Error(KeepAliveAdd(new IndexOutOfRangeException("invalid keepalive slot: " + slot)));
        }

        internal JsValue KeepAliveEnumerateProperties(int slot)
        {
            // TODO: This is pretty slow: use a cache of generated code to make it faster.
            var obj = KeepAliveGet(slot);
            if (obj != null)
            {
                if (typeof(IDictionary).IsAssignableFrom(obj.GetType()))
                {
                    IDictionary dictionary = (IDictionary)obj;
                    string[] keys = dictionary.Keys.Cast<string>().ToArray();
                    return _convert.ToJsValue(keys);
                }

                string[] values = obj.GetType().GetMembers(
                    BindingFlags.Public |
                    BindingFlags.Instance).Where(m => {
                        var method = m as MethodBase;
                        return method == null || !method.IsSpecialName;
                    }).Select(z => z.Name).ToArray();
                return _convert.ToJsValue(values);
            }
            return JsValue.Error(KeepAliveAdd(new IndexOutOfRangeException("invalid keepalive slot: " + slot)));
        }

        public object GetVariable(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            JsValue v = Native.jscontext_get_variable(_context, name);
            object res = _convert.FromJsValue(v);

            Native.jsvalue_dispose(v);

            Exception e = res as JsException;
            if (e != null)
                throw e;
            return res;
        }

        public void SetVariable(string name, object value)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            JsValue a = _convert.ToJsValue(value);
            JsValue b = Native.jscontext_set_variable(_context, name, a);

            Native.jsvalue_dispose(a);
            Native.jsvalue_dispose(b);
            // TODO: Check the result of the operation for errors.
        }

        public object Execute(string code, string name = null, TimeSpan? executionTimeout = null)
        {
            if (code == null)
                throw new ArgumentNullException("code");

            CheckDisposed();
            
            var v = Native.jscontext_execute(_context, code, name ?? "<Unnamed Script>");
            var result = _convert.FromJsValue(v);
            Native.jsvalue_dispose(v);

            return result;
        }

        internal bool TryGetMemberValue(Type type, object obj, string name, out JsValue value)
        {
            object result;

            // dictionaries.
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                IDictionary dictionary = (IDictionary)obj;
                if (dictionary.Contains(name))
                {
                    result = dictionary[name];
                    value = _convert.ToJsValue(result);
                }
                else
                {
                    value = JsValue.Null;
                }
                return true;
            }

            BindingFlags flags;
            if (type == obj)
            {
                flags = BindingFlags.Public | BindingFlags.Static;
            }
            else
            {
                flags = BindingFlags.Public | BindingFlags.Instance;
            }

            // First of all try with a public property (the most common case).
            PropertyInfo pi = type.GetProperty(name, flags);
            if (pi != null)
            {
                result = pi.GetValue(obj, null);
                value = _convert.ToJsValue(result);
                return true;
            }

            // try field.
            FieldInfo fi = type.GetField(name, flags);
            if (fi != null)
            {
                result = fi.GetValue(obj);
                value = _convert.ToJsValue(result);
                return true;
            }

            //// Then with an instance method: the problem is that we don't have a list of
            //// parameter types so we just check if any method with the given name exists
            //// and then keep alive a "weak delegate", i.e., just a name and the target.
            //// The real method will be resolved during the invokation itself.
            //BindingFlags mFlags = flags | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy;

            //// TODO: This is probably slooow.
            //if (type.GetMethods(mFlags).Any(x => x.Name == name))
            //{
            //    if (type == obj)
            //    {
            //        result = new WeakDelegate(type, name);
            //    }
            //    else
            //    {
            //        result = new WeakDelegate(obj, name);
            //    }
            //    value = _convert.ToJsValue(result);
            //    return true;
            //}

            value = JsValue.Null;
            return false;
        }

        #region Keep-alive management and callbacks.

        internal int KeepAliveAdd(object obj)
        {
            return _keepalives.Add(obj);
        }

        internal object KeepAliveGet(int slot)
        {
            return _keepalives.Get(slot);
        }

        internal void KeepAliveRemove(int slot)
        {
            _keepalives.Remove(slot);
        }

        #endregion

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