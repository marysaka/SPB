using SPB.Windowing;
using System.Runtime.Versioning;
using System;

namespace SPB.Platform.EGL
{
    [SupportedOSPlatform("linux")]
    public sealed class EGLWindow : SwappableNativeWindowBase
    {
        public override NativeHandle DisplayHandle { get; }
        public override NativeHandle WindowHandle { get; }

        private uint _swapInterval;

        public bool IsDisposed { get; private set; }

        public EGLHelper helper;

        public EGLWindow(EGLHelper helper_, NativeHandle displayHandle, NativeHandle windowHandle) {
            helper = helper_;
            DisplayHandle = displayHandle;
            WindowHandle = windowHandle;

            _swapInterval = 1;
        }

        public EGLWindow(NativeHandle displayHandle, NativeHandle windowHandle)
        {
            helper = new EGLHelper(displayHandle.RawHandle);
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
                EGL.Ext.SwapInterval(helper.eglDisplay, (int)_swapInterval);
                _swapInterval = value;
            }
        }

        public override void SwapBuffers()
        {
            EGL.SwapBuffers(helper.eglDisplay, WindowHandle.RawHandle);
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