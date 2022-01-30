﻿using SPB.Windowing;

namespace SPB.Platform.EGL
{
    public class EGLWindow : SwappableNativeWindowBase
    {
        public override NativeHandle DisplayHandle { get; }
        public override NativeHandle WindowHandle => _nativeWindow.WindowHandle;
        public NativeHandle SurfaceHandle { get; }

        private uint _swapInterval;
        private EGLNativeWindowHandle _nativeWindow;

        public bool IsDisposed { get; private set; }

        public EGLWindow(NativeHandle displayHandle, EGLNativeWindowHandle nativeWindow, NativeHandle surfaceHandle)
        {
            DisplayHandle = displayHandle;
            _nativeWindow = nativeWindow;
            SurfaceHandle = surfaceHandle;

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
                _swapInterval = value;

                // TODO: exception here
                EGL.SwapInterval(DisplayHandle.RawHandle, (int)_swapInterval);
            }
        }

        public override void Hide()
        {
            _nativeWindow?.Hide();
        }

        public override void Show()
        {
            _nativeWindow?.Show();
        }

        public override void SwapBuffers()
        {
            EGL.SwapBuffers(DisplayHandle.RawHandle, SurfaceHandle.RawHandle);
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    EGL.DestroySurface(DisplayHandle.RawHandle, SurfaceHandle.RawHandle);
                    EGL.Terminate(DisplayHandle.RawHandle);

                    _nativeWindow?.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}
