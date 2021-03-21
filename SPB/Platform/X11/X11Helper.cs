using SPB.Graphics;
using SPB.Platform.GLX;
using SPB.Windowing;
using System;

namespace SPB.Platform.X11
{
    public sealed class X11Helper
    {
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

                // make screen configurable?
                int screen = X11.DefaultScreenLocked(display.RawHandle);

                IntPtr rootWindow = X11.RootWindow(display.RawHandle, screen);

                X11.XSetWindowAttributes attributes = new X11.XSetWindowAttributes();

                attributes.BackgroundPixel = IntPtr.Zero;
                attributes.BorderPixel = IntPtr.Zero;
                attributes.ColorMap = X11.CreateColormap(display.RawHandle, rootWindow, visualInfo->Visual, 0);
                // TODO: events

                X11.SetWindowValueMask windowValueMask = X11.SetWindowValueMask.ColorMap | X11.SetWindowValueMask.EventMask | X11.SetWindowValueMask.BackPixel | X11.SetWindowValueMask.BorderPixel;

                X11.XSetWindowAttributes* attributesPtr = &attributes;

                IntPtr rawWindowHandle = X11.CreateWindow(display.RawHandle, rootWindow, x, y, width, height, 0, visualInfo->Depth, (int)X11.CreateWindowArgs.InputOutput, visualInfo->Visual, (IntPtr)windowValueMask, (IntPtr)attributesPtr);

                if (rawWindowHandle == IntPtr.Zero)
                {
                    throw new ApplicationException("Cannot create X window!");
                }

                return new GLXWindow(display, new NativeHandle(rawWindowHandle));
            }
        }
    }
}