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

        private EGLHelper _helper;
        private IntPtr _nativeDisplay;
        private IntPtr _fbConfig;
        private NativeWindowBase _window;

        public EGLOpenGLContext(FramebufferFormat framebufferFormat, int major, int minor, OpenGLContextFlags flags = OpenGLContextFlags.Default, bool directRendering = true, OpenGLContextBase shareContext = null) : base(framebufferFormat, major, minor, flags, directRendering, shareContext)
        {
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

                    EGL.DestroyContext(_helper.eglDisplay, ContextHandle);
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

            _helper = new EGLHelper(display);
            EGLHelper.BindApi();

            IntPtr fbConfig = _helper.SelectFBConfig(FramebufferFormat);

            if (fbConfig == IntPtr.Zero)
            {
                // TODO: fall back to legacy API
                throw new NotImplementedException("Framebuffer configuration couldn't be selected and fallback not implemented!");
            }
            _fbConfig = fbConfig;

            List<int> contextAttribute = _helper.GetContextCreationARBAttribute(this);

            IntPtr shareContextHandle = ShareContext == null ? IntPtr.Zero : ShareContext.ContextHandle;

            IntPtr context = EGL.ARB.CreateContext(_helper.eglDisplay, fbConfig, shareContextHandle, contextAttribute.ToArray());

            ContextHandle = context;

            if (ContextHandle != IntPtr.Zero)
            {
                _nativeDisplay = display;
            }

            if (ContextHandle == IntPtr.Zero)
            {
                throw new ContextException(String.Format("CreateContext() failed: {0}", EGL.GetError()));
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
                if (_nativeDisplay != window.DisplayHandle.RawHandle)
                {
                    throw new InvalidOperationException("MakeCurrent() should be used with a window originated from the same display.");
                }

                IntPtr surface = ((EGLWindow) window).EGLSurface(_helper, _fbConfig);

                success = EGL.MakeCurrent(_helper.eglDisplay, surface, surface, ContextHandle) != 0;
            }
            else
            {
                success = EGL.MakeCurrent(_helper.eglDisplay, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) != 0;
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
