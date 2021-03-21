using SPB.Windowing;

namespace SPB.Platform.GLX
{
    public sealed class GLXWindow : NativeWindowBase
    {
        public override NativeHandle DisplayHandle { get; }
        public override NativeHandle WindowHandle { get; }

        private uint _swapInterval;

        public bool IsDisposed { get; private set; }

        public GLXWindow(NativeHandle displayHandle, NativeHandle windowHandle)
        {
            DisplayHandle = displayHandle;
            WindowHandle = windowHandle;

            _swapInterval = 1;
        }

        public override uint SwapInterval
        {
            get
            {
                return _swapInterval;
            }
            set
            {
                // TODO: exception here
                GLX.SwapInterval(DisplayHandle.RawHandle, WindowHandle.RawHandle, (int)_swapInterval);
                _swapInterval = value;
            }
        }

        public override void SwapBuffers()
        {
            GLX.SwapBuffers(DisplayHandle.RawHandle, WindowHandle.RawHandle);
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    X11.X11.UnmapWindow(DisplayHandle.RawHandle, WindowHandle.RawHandle);
                    X11.X11.DestroyWindow(DisplayHandle.RawHandle, WindowHandle.RawHandle);
                }

                IsDisposed = true;
            }
        }
    }
}