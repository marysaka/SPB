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

        private EGLHelper _helper;
        private IntPtr _surface = IntPtr.Zero;

        public EGLWindow(NativeHandle displayHandle, NativeHandle windowHandle) {
            DisplayHandle = displayHandle;
            WindowHandle = windowHandle;

            _swapInterval = 1;
        }

        public IntPtr EGLSurface(EGLHelper helper, IntPtr fbConfig) {
            if (_surface != IntPtr.Zero) {
                return _surface;
            }
            _helper = helper;
            _surface = _helper.eglWindowSurface(WindowHandle.RawHandle, fbConfig);
            return _surface;
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
                EGL.Ext.SwapInterval(_helper.eglDisplay, (int)_swapInterval);
                _swapInterval = value;
            }
        }

        public override void SwapBuffers()
        {
            EGL.SwapBuffers(_helper.eglDisplay, _surface);
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (_surface != IntPtr.Zero) {
                        EGL.DestroySurface(_helper.eglDisplay, _surface);
                    }
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