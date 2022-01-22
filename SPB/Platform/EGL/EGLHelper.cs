using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using SPB.Graphics;
using SPB.Graphics.OpenGL;

using static SPB.Platform.X11.X11;

namespace SPB.Platform.EGL
{
    [SupportedOSPlatform("linux")]
    public sealed class EGLHelper
    {
        public static List<int> FramebufferFormatToVisualAttribute(FramebufferFormat format)
        {
            List<int> result = new List<int>();

            result.Add((int)EGL.Attribute.COLOR_BUFFER_TYPE);
            result.Add((int)EGL.Attribute.RGB_BUFFER);

            result.Add((int)EGL.Attribute.RENDERABLE_TYPE);
            result.Add((int)EGL.Attribute.OPENGL_BIT);
            result.Add((int)EGL.Attribute.CONFORMANT);
            result.Add((int)EGL.Attribute.OPENGL_BIT);

            result.Add((int)EGL.Attribute.CONFIG_CAVEAT);
            result.Add((int)EGL.Attribute.NONE);

            if (format.Color.BitsPerPixel > 0)
            {
                result.Add((int)EGL.Attribute.RED_SIZE);
                result.Add(format.Color.Red);

                result.Add((int)EGL.Attribute.GREEN_SIZE);
                result.Add(format.Color.Green);

                result.Add((int)EGL.Attribute.BLUE_SIZE);
                result.Add(format.Color.Blue);

                result.Add((int)EGL.Attribute.ALPHA_SIZE);
                result.Add(format.Color.Alpha);
            }

            if (format.DepthBits > 0)
            {
                result.Add((int)EGL.Attribute.DEPTH_SIZE);
                result.Add(format.DepthBits);
            }

            if (format.StencilBits > 0)
            {
                result.Add((int)EGL.Attribute.STENCIL_SIZE);
                result.Add(format.StencilBits);
            }

            if (format.Samples > 0)
            {
                result.Add((int)EGL.Attribute.SAMPLE_BUFFERS);
                result.Add(1);

                result.Add((int)EGL.Attribute.SAMPLES);
                result.Add((int)format.Samples);
            }

            result.Add((int)EGL.Attribute.NONE);

            return result;
        }

        public static IntPtr SelectFBConfig(IntPtr display, FramebufferFormat format)
        {
            List<int> visualAttribute = FramebufferFormatToVisualAttribute(format);

            IntPtr result = IntPtr.Zero;

            unsafe
            {
                int configCnt;
                int[] attribs = visualAttribute.ToArray();

                uint res = EGL.ChooseConfig(display, attribs, (IntPtr *)IntPtr.Zero.ToPointer(), 0, out configCnt);

                if (configCnt < 0 || res != 0)
                {
                    return IntPtr.Zero;
                }

                fixed (IntPtr* fbConfig = new IntPtr[configCnt]) {
                    EGL.ChooseConfig(display, attribs, fbConfig, configCnt, out configCnt);
                    result = fbConfig[0];
                }
            }

            return result;
        }

        public static List<int> GetContextCreationARBAttribute(OpenGLContextBase context)
        {
            List<int> result = new List<int>();

            result.Add((int)EGL.ARB.CreateContextAttr.MAJOR_VERSION);
            result.Add(context.Major);

            result.Add((int)EGL.ARB.CreateContextAttr.MINOR_VERSION);
            result.Add(context.Minor);

            if (context.Flags != 0)
            {
                if (context.Flags.HasFlag(OpenGLContextFlags.Debug))
                {
                    result.Add((int)EGL.ARB.CreateContextAttr.FLAGS);
                    result.Add((int)EGL.ARB.ContextFlags.DEBUG);
                }


                result.Add((int)EGL.ARB.CreateContextAttr.PROFILE_MASK);

                if (context.Flags.HasFlag(OpenGLContextFlags.Compat))
                {
                    result.Add((int)EGL.ARB.ContextProfileFlags.COMPATIBILITY_PROFILE);
                }
                else
                {
                    result.Add((int)EGL.ARB.ContextProfileFlags.CORE_PROFILE);
                }
            }

            result.Add((int)EGL.Attribute.NONE);

            return result;
        }
    }
}