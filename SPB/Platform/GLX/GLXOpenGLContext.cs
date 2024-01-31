using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using SPB.Graphics;
using SPB.Graphics.Exceptions;
using SPB.Graphics.OpenGL;
using SPB.Windowing;

using static SPB.Platform.X11.X11;

namespace SPB.Platform.GLX
{
    [SupportedOSPlatform("linux")]
    public class GLXOpenGLContext : OpenGLContextBase
    {
        private IntPtr _display;

        private NativeWindowBase _window;

        public GLXOpenGLContext(FramebufferFormat framebufferFormat, int major, int minor, OpenGLContextFlags flags = OpenGLContextFlags.Default, bool directRendering = true, GLXOpenGLContext shareContext = null) : base(framebufferFormat, major, minor, flags, directRendering, shareContext)
        {
            _display = IntPtr.Zero;
            _window = null;
        }

        public override bool IsCurrent => GLX.GetCurrentContext() == ContextHandle;

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    MakeCurrent(null);

                    GLX.DestroyContext(_display, ContextHandle);
                }

                IsDisposed = true;
            }
        }

        public override IntPtr GetProcAddress(string procName)
        {
            return GLX.ARB.GetProcAddress(procName);
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

            IntPtr fbConfig = GLXHelper.SelectFBConfig(display, FramebufferFormat);

            if (fbConfig == IntPtr.Zero)
            {
                // TODO: fall back to legacy API
                throw new NotImplementedException("Framebuffer configuration couldn't be selected and fallback not implemented!");
            }

            List<int> contextAttribute = GLXHelper.GetContextCreationARBAttribute(this);

            IntPtr shareContextHandle = ShareContext == null ? IntPtr.Zero : ShareContext.ContextHandle;

            IntPtr context = GLXHelper.CreateContextAttribs(display, fbConfig, shareContextHandle, DirectRendering, contextAttribute.ToArray());

            if (context == IntPtr.Zero)
            {
                context = GLXHelper.CreateContextAttribs(display, fbConfig, shareContextHandle, !DirectRendering, contextAttribute.ToArray());

                DirectRendering = !DirectRendering;
            }

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
                if (!(window is GLXWindow))
                {
                    throw new InvalidOperationException($"MakeCurrent() should be used with a {typeof(GLXWindow).Name}.");
                }
                if (_display != window.DisplayHandle.RawHandle)
                {
                    throw new InvalidOperationException("MakeCurrent() should be used with a window originated from the same display.");
                }

                success = GLX.MakeCurrent(_display, window.WindowHandle.RawHandle, ContextHandle);
            }
            else
            {
                success = GLX.MakeCurrent(_display, IntPtr.Zero, IntPtr.Zero);
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