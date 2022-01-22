using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using SPB.Graphics;
using SPB.Graphics.Exceptions;
using SPB.Graphics.OpenGL;
using SPB.Windowing;

using static SPB.Platform.X11.X11;

namespace SPB.Platform.EGL
{
    [SupportedOSPlatform("linux")]
    public class EGLOpenGLContext : OpenGLContextBase
    {
        private IntPtr _display;

        private NativeWindowBase _window;

        public EGLOpenGLContext(FramebufferFormat framebufferFormat, int major, int minor, OpenGLContextFlags flags = OpenGLContextFlags.Default, bool directRendering = true, OpenGLContextBase shareContext = null) : base(framebufferFormat, major, minor, flags, directRendering, shareContext)
        {
            _display = IntPtr.Zero;
            _window = null;
        }

        public override bool IsCurrent => EGL.GetCurrentContext() == ContextHandle;

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    MakeCurrent(null);

                    EGL.DestroyContext(_display, ContextHandle);
                }

                IsDisposed = true;
            }
        }

        public override IntPtr GetProcAddress(string procName)
        {
            return EGL.ARB.GetProcAddress(procName);
        }

        public override void Initialize(NativeWindowBase window = null)
        {
            IntPtr display = IntPtr.Zero;

            if (window != null)
            {
                // TODO: Do we want to handle the window providing us a framebuffer format?
                display = window.DisplayHandle.RawHandle;
            }

            if (display == IntPtr.Zero)
            {
                display = DefaultDisplay;
            }

            IntPtr fbConfig = EGLHelper.SelectFBConfig(display, FramebufferFormat);

            if (fbConfig == IntPtr.Zero)
            {
                // TODO: fall back to legacy API
                throw new NotImplementedException("Framebuffer configuration couldn't be selected and fallback not implemented!");
            }

            List<int> contextAttribute = EGLHelper.GetContextCreationARBAttribute(this);

            IntPtr shareContextHandle = ShareContext == null ? IntPtr.Zero : ShareContext.ContextHandle;

            IntPtr context = EGL.ARB.CreateContext(display, fbConfig, shareContextHandle, contextAttribute.ToArray());

            ContextHandle = context;

            if (ContextHandle != IntPtr.Zero)
            {
                _display = display;
            }

            if (ContextHandle == IntPtr.Zero)
            {
                throw new ContextException("CreateContext() failed.");
            }
        }

        public override void MakeCurrent(NativeWindowBase window)
        {
            if (_window != null && window != null && _window.WindowHandle.RawHandle == window.WindowHandle.RawHandle && IsCurrent)
            {
                return;
            }

            bool success;

            if (window != null)
            {
                if (!(window is EGLWindow))
                {
                    throw new InvalidOperationException($"MakeCurrent() should be used with a {typeof(EGLWindow).Name}.");
                }
                if (_display != window.DisplayHandle.RawHandle)
                {
                    throw new InvalidOperationException("MakeCurrent() should be used with a window originated from the same display.");
                }

                success = EGL.MakeCurrent(_display, window.WindowHandle.RawHandle, window.WindowHandle.RawHandle, ContextHandle);
            }
            else

            {
                success = EGL.MakeCurrent(_display, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            }

            if (success)
            {
                _window = window;
            }
            else
            {
                throw new ContextException("MakeCurrent() failed.");
            }
        }
    }
}
