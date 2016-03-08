using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace VroomJs.Tests
{
    public class Objects : IDisposable
    {
        JsEngine _engine;
        JsContext _js;
        
        public Objects()
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
        public void GetManagedIntegerProperty()
        {
            var v = 42;
            var t = new TestClass { Int32Property = v };
            _js.SetVariable("o", t);
            _js.Execute("o.Int32Property").Should().Be(v);
        }

        [Fact]
        public void GetManagedStringProperty()
        {
            var v = "The lazy dog bla bla bla...";
            var t = new TestClass { StringProperty = v };
            _js.SetVariable("o", t);
            _js.Execute("o.StringProperty").Should().Be(v);
        }

        [Fact]
        public void GetManagedNestedProperty()
        {
            var v = "The lazy dog bla bla bla...";
            var t = new TestClass { NestedObject = new TestClass { StringProperty = v } };
            _js.SetVariable("o", t);
            _js.Execute("o.NestedObject.StringProperty").Should().Be(v);
        }

        [Fact]
        public void SetManagedIntegerProperty()
        {
            var t = new TestClass();
            _js.SetVariable("o", t);
            _js.Execute("o.Int32Property = 42");
            t.Int32Property.Should().Be(42);
        }

        [Fact]
        public void SetManagedStringProperty()
        {
            var t = new TestClass();
            _js.SetVariable("o", t);
            _js.Execute("o.StringProperty = 'This was set from Javascript!'");
            t.StringProperty.Should().Be("This was set from Javascript!");
        }

        [Fact]
        public void SetManagedNestedProperty()
        {
            var t = new TestClass();
            var n = new TestClass();
            _js.SetVariable("o", t);
            _js.SetVariable("n", n);
            _js.Execute("o.NestedObject = n; o.NestedObject.Int32Property = 42");
            t.NestedObject.Should().Be(n);
            n.Int32Property.Should().Be(42);
        }

        [Fact]
        public void CallManagedMethod()
        {
            var t = new TestClass { Int32Property = 13, StringProperty = "Wow" };
            _js.SetVariable("o", t);
            _js.Execute("var r = o.Method1(29, '!')");
            object r = _js.GetVariable("r");
            r.Should().BeOfType<TestClass>();
            TestClass c = (TestClass)r;
            c.Int32Property.Should().Be(42);
            c.StringProperty.Should().Be("Wow!");
        }

        [Fact]
        public void GetJsIntegerProperty()
        {
            _js.Execute("var x = { the_answer: 42 }");
            dynamic x = _js.GetVariable("x");
            Assert.True(x.the_answer == 42);
        }

        [Fact]
        public void GetJsStringProperty()
        {
            _js.Execute("var x = { a_string: 'This was set from Javascript!' }");
            dynamic x = _js.GetVariable("x");
            Assert.True(x.a_string == "This was set from Javascript!");
        }

        [Fact]
        public void GetMixedProperty1()
        {
            var t = new TestClass();
            _js.SetVariable("o", t);
            _js.Execute("var x = { nested: o }; x.nested.Int32Property = 42");
            dynamic x = _js.GetVariable("x");
            Assert.True(x.nested == t);
            Assert.True(t.Int32Property == 42);
        }

        [Fact]
        public void SetJsIntegerProperty()
        {
            var v = 42;
            _js.Execute("var x = {}");
            dynamic x = _js.GetVariable("x");
            x.the_answer = v;
            _js.Execute("x.the_answer").Should().Be(v);
        }

        [Fact]
        public void SetJsStringProperty()
        {
            var v = "This was set from managed code!";
            _js.Execute("var x = {}");
            dynamic x = _js.GetVariable("x");
            x.a_string = v;
            _js.Execute("x.a_string").Should().Be(v);
        }

        [Fact]
        public void CallJsProperty()
        {
            _js.Execute("var x = { f: function (a, b) { return [a*2, b+'!']; }}");
            dynamic x = _js.GetVariable("x");
            object r = x.f(21, "Can't believe this worked");
            r.Should().BeAssignableTo<object[]>();
            object[] a = (object[])r;
            a.Length.Should().Be(2);
            a[0].Should().Be(42);
            a[1].Should().Be("Can't believe this worked!");
        }
    }
}
