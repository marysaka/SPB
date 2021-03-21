using System;

namespace SPB.Graphics
{
    public interface IBindingsContext
    {
        IntPtr GetProcAddress(string procName);
    }
}
