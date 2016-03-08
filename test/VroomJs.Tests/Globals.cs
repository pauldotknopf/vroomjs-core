using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace VroomJs.Tests
{
    public class Globals : IDisposable
    {
        JsEngine _engine;
        JsContext _js;
        
        public Globals()
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
        public void SimpleExpressionNull()
        {
            _js.Execute("null").Should().BeNull();
        }

        [Fact]
        public void SimpleExpressionBoolean()
        {
            _js.Execute("0 == 0").Should().Be(true);
        }

        [Fact]
        public void SimpleExpressionInteger()
        {
            _js.Execute("1+1").Should().Be(2);
        }

        [Fact]
        public void SimpleExpressionNumber()
        {
            _js.Execute("3.14159+2.71828").Should().Be(5.85987);
        }

        [Fact]
        public void SimpleExpressionString()
        {
            _js.Execute("'paco'+'cico'").Should().Be("pacocico");
        }

        // TODO: This is failing. Timezone?
        //[Fact]
        //public void SimpleExpressionDate()
        //{
        //    _js.Execute("new Date(1971, 10, 19, 0, 42, 59)").Should().Be(new DateTime(1971, 10, 19, 0, 42, 59));
        //}

        [Fact]
        public void SimpleExpressionArray()
        {
            var res = (object[])_js.Execute("['foobar', 3.14159+2.71828, 42]");
            res.Length.Should().Be(3);
            res[0].Should().Be("foobar");
            res[1].Should().Be(5.85987);
            res[2].Should().Be(42);
        }

        [Fact]
        public void SimpleExpressionObject()
        {
            // Note that a simple "{answer:42}" at top level just returns "42", so we
            // have to use a function and a "return" statement.

            dynamic res = _js.Execute("(function () { return {answer:42}})()");
            Assert.True(res.answer == 42);
        }

        [Fact]
        public void ExpressionAndCall()
        {
            dynamic x = _js.Execute(@"
                (function () { 
                    return {'answer':42, 'tellme':function (x) { return x+' The answer is: '+this.answer; }}
                })()");
            object s = x.tellme("What is the answer to ...?");
            s.Should().Be("What is the answer to ...? The answer is: 42");
        }

        [Fact]
        public void UnicodeScript()
        {
            _js.Execute("var àbç = 12, $ùì = 30; àbç+$ùì;").Should().Be(42);
        }

        [Fact]
        public void SetGetVariableNull()
        {
            _js.SetVariable("foo", null);
            _js.GetVariable("foo").Should().BeNull();
        }

        [Fact]
        public void SetGetVariableBoolean()
        {
            _js.SetVariable("foo", true);
            _js.GetVariable("foo").Should().Be(true);
        }

        [Fact]
        public void SetGetVariableInteger()
        {
            _js.SetVariable("foo", 13);
            _js.GetVariable("foo").Should().Be(13);
        }

        [Fact]
        public void SetGetVariableNumber()
        {
            _js.SetVariable("foo", 3.14159);
            _js.GetVariable("foo").Should().Be(3.14159);
        }

        [Fact]
        public void SetGetVariableString()
        {
            _js.SetVariable("foo", "bar");
            _js.GetVariable("foo").Should().Be("bar");
        }

        [Fact]
        public void SetGetVariableDate()
        {
            var dt = new DateTime(1971, 10, 19, 0, 42, 59);
            _js.SetVariable("foo", dt);
            _js.GetVariable("foo").Should().Be(dt);
        }

        [Fact]
        public void SetGetVariableArray()
        {
            var v = new object[] { "foobar", 3.14159, 42 };
            _js.SetVariable("foo", v);
            _js.Execute("foo[1] += 2.71828");
            object r = _js.GetVariable("foo");
            r.Should().BeAssignableTo<object[]>();
            var a = (object[])r;
            a.Length.Should().Be(3);
            a[0].Should().Be("foobar");
            a[1].Should().Be(5.85987);
            a[2].Should().Be(42);
        }
    }
}
