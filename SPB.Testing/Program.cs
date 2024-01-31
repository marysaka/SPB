using OpenTK.Graphics.OpenGL;
using SPB.Graphics.OpenGL;
using SPB.Compat;
using SPB.Graphics;
using SPB.Windowing;
using SPB.Platform;
using System;
using System.Threading;

namespace SPB
{
    class Program
    {
        static void Main(string[] args)
        {
            SwappableNativeWindowBase window = PlatformHelper.CreateOpenGLWindow(FramebufferFormat.Default, 0, 0, 250, 250);

            window.Show();

            OpenGLContextBase context = PlatformHelper.CreateOpenGLContext(FramebufferFormat.Default, 3, 3, OpenGLContextFlags.Compat);

            context.Initialize(window);
            context.MakeCurrent(window);

            GL.LoadBindings(new OpenToolkitBindingsContext(context));

            GL.ClearColor(1.0f, 0.5f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            window.SwapBuffers();

            Console.WriteLine($"OpenGL vendor: {GL.GetString(StringName.Vendor)}");
            Console.WriteLine($"OpenGL version: {GL.GetString(StringName.Version)}");
            Console.WriteLine($"OpenGL renderer: {GL.GetString(StringName.Renderer)}");
            Console.WriteLine($"OpenGL context profile mask: {GL.GetInteger((GetPName)All.ContextProfileMask)}");
            Console.WriteLine($"OpenGL context flags: {GL.GetInteger((GetPName)All.ContextFlags)}");
            Console.WriteLine($"Window swap interval: {window.SwapInterval}");

            Thread.Sleep(2000);

            window.Dispose();

            Console.WriteLine($"Dispose OpenGL context!");
            context.Dispose();

            string vendor2 = GL.GetString(StringName.Vendor);
            Console.WriteLine($"OpenGL vendor: {vendor2}");

            window.Dispose();
        }
    }
}

