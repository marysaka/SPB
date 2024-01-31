using SPB.Graphics;
using SPB.Platform.EGL;
using SPB.Platform.GLX;
using SPB.Windowing;
using System;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace SPB.Platform.X11
{
    [SupportedOSPlatform("linux")]
    public sealed class X11Helper
    {
        public static unsafe NativeHandle CreateWindow(NativeHandle display, X11.XVisualInfo* visualInfo, int x, int y, int width, int height)
        {
            // make screen configurable?
            int screen = X11.DefaultScreenLocked(display.RawHandle);

            IntPtr rootWindow = X11.RootWindow(display.RawHandle, screen);

            X11.XSetWindowAttributes attributes = new X11.XSetWindowAttributes
            {
                BackgroundPixel = IntPtr.Zero,
                BorderPixel = IntPtr.Zero,
                ColorMap = X11.CreateColormap(display.RawHandle, rootWindow, visualInfo->Visual, 0)
            };
            // TODO: events

            X11.SetWindowValueMask windowValueMask = X11.SetWindowValueMask.ColorMap | X11.SetWindowValueMask.EventMask | X11.SetWindowValueMask.BackPixel | X11.SetWindowValueMask.BorderPixel;

            X11.XSetWindowAttributes* attributesPtr = &attributes;

            IntPtr rawWindowHandle = X11.CreateWindow(display.RawHandle, rootWindow, x, y, width, height, 0, visualInfo->Depth, (int)X11.CreateWindowArgs.InputOutput, visualInfo->Visual, (IntPtr)windowValueMask, (IntPtr)attributesPtr);

            if (rawWindowHandle == IntPtr.Zero)
            {
                throw new ApplicationException("Cannot create X window!");
            }

            return new NativeHandle(rawWindowHandle);
        }

        public static EGLWindow CreateEGLWindow(NativeHandle display, FramebufferFormat format, int x, int y, int width, int height)
        {
            NativeHandle eglDisplayHandle = new NativeHandle(EGL.EGL.GetDisplay(display.RawHandle));

            if (!EGL.EGL.Initialize(eglDisplayHandle.RawHandle, out _, out _))
            {
                throw new ApplicationException("CreateEGLWindow() failed. Cannot initialize EGL!");
            }

            if (!EGL.EGL.BindAPI(EGL.EGL.ApiType.OPENGL_API))
            {
                EGL.EGL.Terminate(eglDisplayHandle.RawHandle);

                throw new ApplicationException("CreateEGLWindow() failed. Cannot bind EGL!");
            }

            IntPtr config = EGLHelper.SelectConfig(eglDisplayHandle.RawHandle, format);

            if (config == IntPtr.Zero)
            {
                throw new ApplicationException("CreateEGLWindow() failed. EGL configuration couldn't be selected!");
            }

            unsafe
            {
                X11.XVisualInfo* visualInfo;

                bool success = EGL.EGL.GetConfigAttrib(
                    eglDisplayHandle.RawHandle,
                    config,
                    (int)EGL.EGL.ConfigAttribute.NATIVE_VISUAL_ID,
                    out int visualId
                );

                Debug.Assert(success);

                X11.XVisualInfo template = new X11.XVisualInfo
                {
                    VisualId = (ulong)visualId
                };

                visualInfo = X11.GetVisualInfo(
                    display.RawHandle,
                    1,
                    &template,
                    out _
                );

                if (visualInfo == null)
                {
                    throw new ApplicationException("CreateEGLWindow() failed. X visual info not found!");
                }

                NativeHandle windowHandle = CreateWindow(display, visualInfo, x, y, width, height);

                IntPtr surfaceHandle = EGL.EGL.CreateWindowSurface(eglDisplayHandle.RawHandle, config, windowHandle.RawHandle, [
                    (int)EGL.EGL.ConfigAttribute.GL_COLORSPACE,
                    (int)EGL.EGL.ConfigAttribute.GL_COLORSPACE_LINEAR,
                    (int)EGL.EGL.ConfigAttribute.NONE
                ]);

                if (surfaceHandle == IntPtr.Zero)
                {
                    throw new ApplicationException("CreateEGLWindow() failed. Cannot create EGL window surface!");
                }

                return new EGLWindow(eglDisplayHandle, new SimpleX11Window(display, windowHandle), new NativeHandle(surfaceHandle), new NativeHandle(config));
            }
        }

        public static GLXWindow CreateGLXWindow(NativeHandle display, FramebufferFormat format, int x, int y, int width, int height)
        {
            IntPtr fbConfig = GLXHelper.SelectFBConfig(display.RawHandle, format);

            unsafe
            {
                X11.XVisualInfo* visualInfo;

                if (fbConfig != IntPtr.Zero)
                {
                    visualInfo = GLX.GLX.GetVisualFromFBConfig(display.RawHandle, fbConfig);
                }
                else
                {
                    // TODO: support old visual selection
                    throw new NotImplementedException();
                }

                if (visualInfo == null)
                {
                    throw new NotImplementedException();
                }

                NativeHandle windowHandle = CreateWindow(display, visualInfo, x, y, width, height);

                return new GLXWindow(display, windowHandle);
            }
        }
    }
}