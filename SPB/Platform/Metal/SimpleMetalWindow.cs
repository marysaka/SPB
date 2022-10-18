using SPB.Windowing;
using System.Runtime.Versioning;

using static SPB.Platform.Metal.MacOS;

namespace SPB.Platform.Metal
{
    [SupportedOSPlatform("macos")]
    public class SimpleMetalWindow : NativeWindowBase
    {
        public override NativeHandle DisplayHandle { get; }

        public override NativeHandle WindowHandle { get; }

        public bool IsDisposed { get; private set; }

        public SimpleMetalWindow(NativeHandle nsView, NativeHandle layer)
        {
            DisplayHandle = nsView;
            WindowHandle = layer;
        }

        public override void Hide()
        {
            objc_msgSend(DisplayHandle.RawHandle, "setHidden:", 1);
        }

        public override void Show()
        {
            objc_msgSend(DisplayHandle.RawHandle, "setHidden:", 0);
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // Layer is provided by the user, it's up to them to dispose it right now.
                    // TODO: Should we handle dispose ourself at some point?
                }

                IsDisposed = true;
            }
        }
    }
}
