using System;

namespace VroomJs
{
    public class JsException : Exception
    {

        internal static JsException Create(JsConvert convert, JsError error)
        {
            string type = (string)convert.FromJsValue(error.Type);
            string resource = (string)convert.FromJsValue(error.Resource);
            string message = (string)convert.FromJsValue(error.Message);
            int line = error.Line;
            int column = error.Column + 1; // because zero based.
                                           //

            JsException exception;

            if (error.Exception.Type == JsValueType.String)
            {
                exception = new JsException(type, resource, message, line, column, null);
            }
            else if (type == "SyntaxError")
            {
                exception = new JsSyntaxError(type, resource, message, line, column);
            }
            else
            {
                JsObject nativeException = (JsObject)convert.FromJsValue(error.Exception);
                exception = new JsException(type, resource, message, line, column, nativeException);
            }
            return exception;
        }

        public JsException()
        {
        }

        public JsException(string message) : base(message)
        {
        }

        public JsException(string message, Exception inner) : base(message, inner)
        {

        }

        internal JsException(string type, string resource, string message, int line, int col, JsObject error)
            : base(string.Format("{0}: {1} at line: {2} column: {3}.", resource, message, line, col))
        {
            _type = type;
            _resource = resource;
            _line = line;
            _column = col;
            _nativeException = error;
        }

        // Native V8 exception objects are wrapped by special instances of JsException.

        public JsException(JsObject nativeException)
        {
            _nativeException = nativeException;
        }

        readonly JsObject _nativeException;

        public JsObject NativeException
        {
            get { return _nativeException; }
        }


        protected string _type;
        public string Type { get { return _type; } }

        protected string _resource;
        public string Resource { get { return _resource; } }

        protected int _line;
        public int Line { get { return _line; } }

        protected int _column;
        public int Column { get { return _column; } }
    }

    public class JsSyntaxError : JsException
    {
        internal JsSyntaxError(string type, string resource, string message, int line, int col)
            : base(type, resource, message, line, col, null)
        {
        }
    }


    public class JsInteropException : JsException
    {
        public JsInteropException(string message) : base(message)
        {

        }

        public JsInteropException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
