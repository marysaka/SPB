using SPB.Windowing;

namespace SPB.Platform.X11
{
    public sealed class SimpleX11Window : NativeWindowBase
    {
        public override NativeHandle DisplayHandle { get; }
        public override NativeHandle WindowHandle { get; }

        public bool IsDisposed { get; private set; }

        public SimpleX11Window(NativeHandle displayHandle, NativeHandle windowHandle)
        {
            DisplayHandle = displayHandle;
            WindowHandle = windowHandle;
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    X11.UnmapWindow(DisplayHandle.RawHandle, WindowHandle.RawHandle);
                    X11.DestroyWindow(DisplayHandle.RawHandle, WindowHandle.RawHandle);
                }

                IsDisposed = true;
            }
        }

        public override void Show()
        {
            X11.MapWindow(DisplayHandle.RawHandle, WindowHandle.RawHandle);
        }

        public override void Hide()
        {
            X11.UnmapWindow(DisplayHandle.RawHandle, WindowHandle.RawHandle);
        }
    }
}
