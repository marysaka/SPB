using SPB.Graphics;
using SPB.Graphics.Exceptions;
using SPB.Graphics.OpenGL;
using SPB.Windowing;
using System;
using System.Collections.Generic;

namespace SPB.Platform.EGL
{
    internal class EGLOpenGLContext : OpenGLContextBase
    {
        private IntPtr _display;

        private NativeWindowBase _window;

        public EGLOpenGLContext(FramebufferFormat framebufferFormat, int major, int minor, OpenGLContextFlags flags = OpenGLContextFlags.Default, bool directRendering = true, EGLOpenGLContext shareContext = null) : base(framebufferFormat, major, minor, flags, directRendering, shareContext)
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
            return EGL.GetProcAddress(procName);
        }

        public override void Initialize(NativeWindowBase window = null)
        {
            IntPtr display;

            if (window != null)
            {
                display = window.DisplayHandle.RawHandle;
            }
            else
            {
                // TODO: check for "EGL_KHR_surfaceless_context" support

                display = EGL.GetDisplay(UIntPtr.Zero);
            }

            IntPtr config = EGLHelper.SelectConfig(display, FramebufferFormat);

            if (config == IntPtr.Zero)
            {
                throw new ContextException("CreateContext() failed. EGL configuration couldn't be selected!");
            }

            List<int> contextAttribute = EGLHelper.GetContextCreationAttribute(this);

            IntPtr shareContextHandle = ShareContext == null ? IntPtr.Zero : ShareContext.ContextHandle;

            // By spec, eglBindAPI must be called before any context related API calls on a per thread basis.
            if (EGL.BindAPI(EGL.ApiType.OPENGL_API))
            {
                ContextHandle = EGL.CreateContext(display, config, shareContextHandle, contextAttribute.ToArray());
            }

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
            if (_window != null && _window.WindowHandle.RawHandle == window.WindowHandle.RawHandle && IsCurrent)
            {
                return;
            }

            bool success = false;

            // By spec, eglBindAPI must be called before eglMakeCurrent on a per thread basis.
            if (EGL.BindAPI(EGL.ApiType.OPENGL_API))
            {
                if (window != null)
                {
                    if (window is not EGLWindow eglWindow)
                    {
                        throw new InvalidOperationException($"MakeCurrent() should be used with a {typeof(EGLWindow).Name}.");
                    }
                    if (_display != window.DisplayHandle.RawHandle)
                    {
                        throw new InvalidOperationException("MakeCurrent() should be used with a window originated from the same display.");
                    }

                    success = EGL.MakeCurrent(_display, eglWindow.SurfaceHandle.RawHandle, eglWindow.SurfaceHandle.RawHandle, ContextHandle);
                }
                else
                {
                    success = EGL.MakeCurrent(_display, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                }
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
