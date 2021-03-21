using System;

namespace SPB.Windowing
{
    public abstract class NativeWindowBase : IDisposable
    {
        public abstract NativeHandle DisplayHandle { get; }
        public abstract NativeHandle WindowHandle { get; }

        public abstract uint SwapInterval { get; set; }

        public abstract void SwapBuffers();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
    }
}