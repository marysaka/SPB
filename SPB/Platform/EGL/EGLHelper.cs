using SPB.Graphics;
using SPB.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace SPB.Platform.EGL
{
    internal class EGLHelper
    {
        public static IntPtr SelectConfig(IntPtr display, FramebufferFormat format)
        {
            List<int> attributes = FramebufferFormatToConfigAttribute(format);

            IntPtr result = IntPtr.Zero;

            unsafe
            {
                int configCount;

                bool success = EGL.ChooseConfig(display, attributes.ToArray(), (IntPtr)(&result), 1, out configCount);

                if (!success || configCount < 1)
                {
                    result = IntPtr.Zero;
                }
            }

            return result;
        }

        private static List<int> FramebufferFormatToConfigAttribute(FramebufferFormat format)
        {
            List<int> result = new List<int>();

            // Do not allow possible software rendering.
            result.Add((int)EGL.ConfigAttribute.CONFIG_CAVEAT);
            result.Add(0);

            // Only allow surface window.
            result.Add((int)EGL.ConfigAttribute.SURFACE_TYPE);
            result.Add((int)EGL.ConfigAttribute.WINDOW_BIT);

            // Enforce OpenGL Core render but do not enforce conformance.
            result.Add((int)EGL.ConfigAttribute.RENDERABLE_TYPE);
            result.Add((int)EGL.ConfigAttribute.OPENGL_BIT);

            if (format.Color.BitsPerPixel > 0)
            {
                result.Add((int)EGL.ConfigAttribute.COLOR_BUFFER_TYPE);
                result.Add((int)EGL.ConfigAttribute.RGB_BUFFER);

                result.Add((int)EGL.ConfigAttribute.RED_SIZE);
                result.Add(format.Color.Red);

                result.Add((int)EGL.ConfigAttribute.GREEN_SIZE);
                result.Add(format.Color.Green);

                result.Add((int)EGL.ConfigAttribute.BLUE_SIZE);
                result.Add(format.Color.Blue);

                result.Add((int)EGL.ConfigAttribute.ALPHA_SIZE);
                result.Add(format.Color.Alpha);
            }

            if (format.DepthBits > 0)
            {
                result.Add((int)EGL.ConfigAttribute.DEPTH_SIZE);
                result.Add(format.DepthBits);
            }


            if (format.Buffers <= 1)
            {
                result.Add((int)EGL.ConfigAttribute.RENDER_BUFFER);
                result.Add((int)EGL.ConfigAttribute.SINGLE_BUFFER);
            }

            if (format.StencilBits > 0)
            {
                result.Add((int)EGL.ConfigAttribute.STENCIL_SIZE);
                result.Add(format.StencilBits);
            }

            if (format.Samples > 0)
            {
                result.Add((int)EGL.ConfigAttribute.SAMPLES);
                result.Add((int)format.Samples);
            }

            result.Add(0);

            return result;
        }

        public static List<int> GetContextCreationAttribute(OpenGLContextBase context)
        {
            List<int> result = new List<int>();

            result.Add((int)EGL.CreateContextAttribute.MAJOR_VERSION);
            result.Add(context.Major);

            result.Add((int)EGL.CreateContextAttribute.MINOR_VERSION);
            result.Add(context.Minor);

            if (context.Flags != 0)
            {
                if (context.Flags.HasFlag(OpenGLContextFlags.Debug))
                {
                    result.Add((int)EGL.CreateContextAttribute.FLAGS);
                    result.Add((int)EGL.CreateContextFlags.DEBUG_BIT);
                }


                result.Add((int)EGL.CreateContextAttribute.PROFILE_MASK);

                if (context.Flags.HasFlag(OpenGLContextFlags.Compat))
                {
                    result.Add((int)EGL.CreateContextProfileFlags.COMPATIBILITY_PROFILE);
                }
                else
                {
                    result.Add((int)EGL.CreateContextProfileFlags.CORE_PROFILE);
                }
            }

            result.Add(0);

            return result;
        }
    }
}
