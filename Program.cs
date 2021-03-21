using SPB.Graphics.OpenGL;
using SPB.Platform.GLX;
using SPB.Compat;
using System;
using SPB.Platform.X11;
using SPB.Graphics;
using SPB.Windowing;
using System.Threading;
using OpenTK.Graphics.OpenGL;

namespace SPB
{
    class Program
    {
        static void Main(string[] args)
        {
            GLXOpenGLContext context = new GLXOpenGLContext(FramebufferFormat.Default, 3, 3);

            context.Initialize();

            NativeWindowBase window = X11Helper.CreateGLXWindow(new NativeHandle(X11.DefaultDisplay),
                                                             FramebufferFormat.Default,
                                                             0, 0, 100, 100);

            context.MakeCurrent(window);

            GL.LoadBindings(new OpenToolkitBindingsContext(context));


            GL.ClearColor(1.0f, 0.5f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            window.SwapBuffers();
            window.Dispose();

            Thread.Sleep(10000);

            string vendor = GL.GetString(StringName.Vendor);
            Console.WriteLine($"OpenGL vendor: {vendor}");

            Console.WriteLine($"Dispose OpenGL context!");
            context.Dispose();

            string vendor2 = GL.GetString(StringName.Vendor);
            Console.WriteLine($"OpenGL vendor: {vendor2}");

            window.Dispose();
        }
    }
}

