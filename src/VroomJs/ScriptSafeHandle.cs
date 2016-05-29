using System;
using System.Runtime.InteropServices;

namespace VroomJs
{
    public class ScriptSafeHandle : SafeHandle
    {
        public ScriptSafeHandle(IntPtr invalidHandleValue, bool ownsHandle) : base(invalidHandleValue, ownsHandle)
        {
        }

        public override bool IsInvalid
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        protected override bool ReleaseHandle()
        {
            throw new NotImplementedException();
        }
    }
}
