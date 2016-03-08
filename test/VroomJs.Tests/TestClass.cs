using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VroomJs.Tests
{
    public class TestClass
    {
        public int Int32Property { get; set; }
        public string StringProperty { get; set; }
        public TestClass NestedObject { get; set; }

        public TestClass Method1(int i, string s)
        {
            return new TestClass { Int32Property = Int32Property + i, StringProperty = StringProperty + s };
        }
    }
}
