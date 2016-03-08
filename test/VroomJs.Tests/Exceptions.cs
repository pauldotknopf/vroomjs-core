using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace VroomJs.Tests
{
    public class Exceptions
    {
        JsEngine _engine;
        JsContext _js;

        public Exceptions()
        {
            _engine = new JsEngine();
            _js = _engine.CreateContext();
        }

        public void Dispose()
        {
            _js.Dispose();
            _engine.Dispose();
        }

        [Fact]
        public void SimpleExpressionException()
        {
            Assert.Throws<JsException>(() => _js.Execute("throw 'xxx'"));
        }

        [Fact]
        public void JsObjectException()
        {
            Assert.Throws<JsException>(() => _js.Execute("throw {msg:'Error!'}"));
        }

        [Fact]
        public void CompilationException()
        {
            Assert.Throws<JsSyntaxError>(() => _js.Execute("a+§"));
        }
    }
}
