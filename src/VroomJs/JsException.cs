using System;

namespace VroomJs
{
    public class JsException : Exception
    {
        public JsException(string message) : base(message)
        {
        }

        public JsException(string message, Exception inner) 
            : base(message, inner)
        {

        }
    }

    public class JsSyntaxError : JsException
    {
        public JsSyntaxError(string message) : base(message)
        {
        }

        public JsSyntaxError(string message, Exception inner) 
            : base(message, inner)
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
