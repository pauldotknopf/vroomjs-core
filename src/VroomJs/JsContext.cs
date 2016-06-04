using System;
using System.Collections;
using System.Collections.Generic;
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

        public JsEngine Engine
        {
            get { return _engine; }
        }

        internal JsValue KeepAliveValueOf(int slot)
        {
            throw new NotImplementedException();
        }

        internal JsValue KeepAliveInvoke(int slot, JsValue args)
        {
            var obj = KeepAliveGet(slot);
            if (obj != null)
            {
                Type constructorType = obj as Type;
                if (constructorType != null)
                {
                    object[] constructorArgs = (object[])_convert.FromJsValue(args);
                    return _convert.ToJsValue(Activator.CreateInstance(constructorType, constructorArgs));
                }

                WeakDelegate func = obj as WeakDelegate;
                if (func == null)
                {
                    throw new Exception("not a function.");
                }

                Type type = func.Target != null ? func.Target.GetType() : func.Type;
                object[] a = (object[])_convert.FromJsValue(args);

                BindingFlags flags = BindingFlags.Public | BindingFlags.FlattenHierarchy;

                if (func.Target != null)
                {
                    flags |= BindingFlags.Instance;
                }
                else
                {
                    flags |= BindingFlags.Static;
                }

                if (obj is BoundWeakDelegate)
                {
                    flags |= BindingFlags.NonPublic;
                }

                // need to convert methods from JsFunction's into delegates?
                if (a.Any(z => z != null && z.GetType() == typeof(JsFunction)))
                {
                    CheckAndResolveJsFunctions(type, func.MethodName, flags, a);
                }

                try
                {
                    var result = type.GetMethod(func.MethodName, flags).Invoke(func.Target, a);
                    return _convert.ToJsValue(result);
                }
                catch (TargetInvocationException e)
                {
                    return JsValue.Error(KeepAliveAdd(e.InnerException));
                }
                catch (Exception e)
                {
                    return JsValue.Error(KeepAliveAdd(e));
                }
            }

            return JsValue.Error(KeepAliveAdd(new IndexOutOfRangeException("invalid keepalive slot: " + slot)));
        }

        internal JsValue KeepAliveSetPropertyValue(int slot, string name, JsValue value)
        {
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
                        if (TrySetMemberValue(type, obj, upperCamelCase, value))
                        {
                            return JsValue.Null;
                        }
                        if (TrySetMemberValue(type, obj, name, value))
                        {
                            return JsValue.Null;
                        }
                    }

                    return JsValue.Error(KeepAliveAdd(new InvalidOperationException(string.Format("property not found on {0}: {1} ", type, name))));
                }
                catch (Exception e)
                {
                    return JsValue.Error(KeepAliveAdd(e));
                }
            }

            return JsValue.Error(KeepAliveAdd(new IndexOutOfRangeException("invalid keepalive slot: " + slot)));
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

            // Then with an instance method: the problem is that we don't have a list of
            // parameter types so we just check if any method with the given name exists
            // and then keep alive a "weak delegate", i.e., just a name and the target.
            // The real method will be resolved during the invokation itself.
            BindingFlags mFlags = flags | BindingFlags.FlattenHierarchy;

            // TODO: This is probably slooow.
            if (type.GetMethods(mFlags).Any(x => x.Name == name))
            {
                if (type == obj)
                {
                    result = new WeakDelegate(type, name);
                }
                else
                {
                    result = new WeakDelegate(obj, name);
                }
                value = _convert.ToJsValue(result);
                return true;
            }

            value = JsValue.Null;
            return false;
        }

        public IEnumerable<string> GetMemberNames(JsObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            CheckDisposed();

            if (obj.Handle == IntPtr.Zero)
                throw new JsInteropException("wrapped V8 object is empty (IntPtr is Zero)");

            JsValue v = Native.jscontext_get_property_names(_context, obj.Handle);
            object res = _convert.FromJsValue(v);
            Native.jsvalue_dispose(v);

            Exception e = res as JsException;
            if (e != null)
                throw e;

            object[] arr = (object[])res;
            return arr.Cast<string>();
        }


        public object GetPropertyValue(JsObject obj, string name)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            if (obj.Handle == IntPtr.Zero)
                throw new JsInteropException("wrapped V8 object is empty (IntPtr is Zero)");

            JsValue v = Native.jscontext_get_property_value(_context, obj.Handle, name);
            object res = _convert.FromJsValue(v);
            Native.jsvalue_dispose(v);

            Exception e = res as JsException;
            if (e != null)
                throw e;
            return res;
        }

        public void SetPropertyValue(JsObject obj, string name, object value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            if (obj.Handle == IntPtr.Zero)
                throw new JsInteropException("wrapped V8 object is empty (IntPtr is Zero)");

            JsValue a = _convert.ToJsValue(value);
            JsValue v = Native.jscontext_set_property_value(_context, obj.Handle, name, a);
            object res = _convert.FromJsValue(v);
            Native.jsvalue_dispose(v);
            Native.jsvalue_dispose(a);

            Exception e = res as JsException;
            if (e != null)
                throw e;
        }

        public object InvokeProperty(JsObject obj, string name, object[] args)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            if (obj.Handle == IntPtr.Zero)
                throw new JsInteropException("wrapped V8 object is empty (IntPtr is Zero)");

            JsValue a = JsValue.Null; // Null value unless we're given args.
            if (args != null)
                a = _convert.ToJsValue(args);

            JsValue v = Native.jscontext_invoke_property(_context, obj.Handle, name, a);
            object res = _convert.FromJsValue(v);
            Native.jsvalue_dispose(v);
            Native.jsvalue_dispose(a);

            Exception e = res as JsException;
            if (e != null)
                throw e;
            return res;
        }

        internal bool TrySetMemberValue(Type type, object obj, string name, JsValue value)
        {
            // dictionaries.
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                IDictionary dictionary = (IDictionary)obj;
                dictionary[name] = _convert.FromJsValue(value);
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

            PropertyInfo pi = type.GetProperty(name, flags);
            if (pi != null && pi.CanWrite)
            {
                pi.SetValue(obj, _convert.FromJsValue(value), null);
                return true;
            }

            return false;
        }

        private static void CheckAndResolveJsFunctions(Type type, string methodName, BindingFlags flags, object[] args)
        {
            MethodInfo mi = type.GetMethod(methodName, flags);
            ParameterInfo[] paramTypes = mi.GetParameters();

            for (int i = 0; i < args.Length; i++)
            {
                if (i >= paramTypes.Length)
                {
                    continue;
                }
                if (args[i] != null && args[i].GetType() == typeof(JsFunction))
                {
                    JsFunction function = (JsFunction)args[i];
                    args[i] = function.MakeDelegate(paramTypes[i].ParameterType, args);
                }
            }
        }

        public object Invoke(IntPtr funcPtr, IntPtr thisPtr, object[] args)
        {
            CheckDisposed();

            if (funcPtr == IntPtr.Zero)
                throw new JsInteropException("wrapped V8 function is empty (IntPtr is Zero)");

            JsValue a = JsValue.Null; // Null value unless we're given args.
            if (args != null)
                a = _convert.ToJsValue(args);

            JsValue v = Native.jscontext_invoke(_context, funcPtr, thisPtr, a);
            object res = _convert.FromJsValue(v);
            Native.jsvalue_dispose(v);
            Native.jsvalue_dispose(a);

            Exception e = res as JsException;
            if (e != null)
                throw e;
            return res;
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