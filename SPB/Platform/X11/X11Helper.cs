using SPB.Graphics;
using SPB.Platform.GLX;
using SPB.Platform.EGL;
using SPB.Windowing;
using System;
using System.Runtime.Versioning;

namespace SPB.Platform.X11
{
    [SupportedOSPlatform("linux")]
    public sealed class X11Helper
    {
        private static unsafe NativeHandle CreateX11Window(NativeHandle display, X11.XVisualInfo* visualInfo, int x, int y, int width, int height) {
            if (visualInfo == null)
            {
                throw new NotImplementedException();
            }

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
        public static EGLWindow CreateEGLWindow(NativeHandle display, FramebufferFormat format, int x, int y, int width, int height) {
            unsafe {
                int num_visuals = 0;
                X11.XVisualInfo template = new X11.XVisualInfo();
                X11.XVisualInfo* visualInfo = X11.GetVisualInfo(
                    display.RawHandle,
                    1,
                    out template,
                    out num_visuals
                );
                
                if (num_visuals != 1) {
                    throw new NotImplementedException();
                }

                NativeHandle windowHandle = CreateX11Window(display, visualInfo, x, y, width, height);

                return new EGLWindow(display, windowHandle);
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

                NativeHandle windowHandle = CreateX11Window(display, visualInfo, x, y, width, height);

                return new GLXWindow(display, windowHandle);
            }
        }
    }
}