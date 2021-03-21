using SPB.Windowing;
using System;

using static SPB.Platform.Win32.Win32;

namespace SPB.Platform.Win32
{
    public class SimpleWin32Window : NativeWindowBase
    {
        public override NativeHandle WindowHandle { get; }

        public override NativeHandle DisplayHandle => throw new NotImplementedException();

        public bool IsDisposed { get; private set; }

        public override uint SwapInterval { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public SimpleWin32Window(NativeHandle windowHandle)
        {
            WindowHandle = windowHandle;
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

        public override void SwapBuffers()
        {
            throw new NotImplementedException();
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
