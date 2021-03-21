using SPB.Windowing;
using System;

using static SPB.SPB.Platform.Win32.Win32;

namespace SPB.SPB.Platform.WGL
{
    public class WGLWindow : NativeWindowBase
    {
        public override NativeHandle DisplayHandle { get; }
        public override NativeHandle WindowHandle { get; }

        public bool IsDisposed { get; private set; }

        public WGLWindow(NativeHandle windowHandle)
        {
            DisplayHandle = new NativeHandle(GetDC(windowHandle.RawHandle));
            WindowHandle = windowHandle;
        }

        public override uint SwapInterval
        {
            get
            {
                return (uint)WGLHelper.GetSwapInterval();
            }
            set
            {
                bool success = WGLHelper.SwapInterval((int)value);

                if (!success)
                {
                    // TODO: exception
                }
            }
        }

        public override void SwapBuffers()
        {
            bool success = Win32.Win32.SwapBuffers(DisplayHandle.RawHandle);

            if (!success)
            {
                // TODO: exception
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    DestroyWindow(WindowHandle.RawHandle);
                }

                IsDisposed = true;
            }
        }

        public override void Show()
        {
            ShowWindow(WindowHandle.RawHandle, ShowWindowFlag.SW_SHOWNOACTIVATE);
        }

        public override void Hide()
        {
            ShowWindow(WindowHandle.RawHandle, ShowWindowFlag.SW_HIDE);
        }
    }
}
