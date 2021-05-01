using SPB.Windowing;

namespace SPB.Platform.GLX
{
    public sealed class GLXWindow : SwapableNativeWindowBase
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
            // TODO: check extension support
            // TODO: support MESA and SGI
            // TODO: use glXQueryDrawable to query swap interval when GLX_EXT_swap_control is supported.
            get
            {
                return _swapInterval;
            }
            set
            {
                // TODO: exception here
                GLX.Ext.SwapInterval(DisplayHandle.RawHandle, WindowHandle.RawHandle, (int)_swapInterval);
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

        public override void Show()
        {
            X11.X11.MapWindow(DisplayHandle.RawHandle, WindowHandle.RawHandle);
        }

        public override void Hide()
        {
            X11.X11.UnmapWindow(DisplayHandle.RawHandle, WindowHandle.RawHandle);
        }
    }
}