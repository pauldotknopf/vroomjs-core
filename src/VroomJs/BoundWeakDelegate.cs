using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VroomJs
{
    class BoundWeakDelegate : WeakDelegate
    {
        public BoundWeakDelegate(object target, string name)
            : base(target, name)
        {
        }

        public BoundWeakDelegate(Type type, string name)
            : base(type, name)
        {
        }
    }
}
