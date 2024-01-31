using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using SPB.Graphics;
using SPB.Graphics.OpenGL;

using static SPB.Platform.X11.X11;

namespace SPB.Platform.GLX
{
    [SupportedOSPlatform("linux")]
    public sealed class GLXHelper
    {
        private static bool _isInit = false;

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate IntPtr glxCreateContextAttribsARBDelegate(IntPtr display, IntPtr fbConfigs, IntPtr shareContext, bool direct, int[] attributes);

        private static glxCreateContextAttribsARBDelegate CreateContextAttribsArb;

        private static void EnsureInit()
        {
            if (!_isInit)
            {
                CreateContextAttribsArb = Marshal.GetDelegateForFunctionPointer<glxCreateContextAttribsARBDelegate>(GLX.ARB.GetProcAddress("glXCreateContextAttribsARB"));

                _isInit = true;
            }
        }

        public static IntPtr CreateContextAttribs(IntPtr display, IntPtr fbConfigs, IntPtr shareContext, bool direct, int[] attributes)
        {
            EnsureInit();

            return CreateContextAttribsArb(display, fbConfigs, shareContext, direct, attributes);
        }

        public static List<int> FramebufferFormatToVisualAttribute(FramebufferFormat format)
        {
            List<int> result = new List<int>();

            if (format.Color.BitsPerPixel > 0)
            {
                result.Add((int)GLX.Attribute.RENDER_TYPE);
                result.Add((int)GLX.RenderTypeMask.RGBA_BIT);

                result.Add((int)GLX.Attribute.RED_SIZE);
                result.Add(format.Color.Red);

                result.Add((int)GLX.Attribute.GREEN_SIZE);
                result.Add(format.Color.Green);

                result.Add((int)GLX.Attribute.BLUE_SIZE);
                result.Add(format.Color.Blue);

                result.Add((int)GLX.Attribute.ALPHA_SIZE);
                result.Add(format.Color.Alpha);
            }

            if (format.DepthBits > 0)
            {
                result.Add((int)GLX.Attribute.DEPTH_SIZE);
                result.Add(format.DepthBits);
            }


            if (format.Buffers > 1)
            {
                result.Add((int)GLX.Attribute.DOUBLEBUFFER);
                result.Add(1);
            }

            if (format.StencilBits > 0)
            {
                result.Add((int)GLX.Attribute.STENCIL_SIZE);
                result.Add(format.StencilBits);
            }

            if (format.Accumulator.BitsPerPixel > 0)
            {
                result.Add((int)GLX.Attribute.ACCUM_ALPHA_SIZE);
                result.Add(format.Accumulator.Alpha);

                result.Add((int)GLX.Attribute.ACCUM_BLUE_SIZE);
                result.Add(format.Accumulator.Blue);

                result.Add((int)GLX.Attribute.ACCUM_GREEN_SIZE);
                result.Add(format.Accumulator.Green);

                result.Add((int)GLX.Attribute.ACCUM_RED_SIZE);
                result.Add(format.Accumulator.Red);
            }

            if (format.Samples > 0)
            {
                result.Add((int)GLX.Attribute.SAMPLE_BUFFERS);
                result.Add(1);

                result.Add((int)GLX.Attribute.SAMPLES);
                result.Add((int)format.Samples);
            }

            if (format.Stereo)
            {
                result.Add((int)GLX.Attribute.STEREO);
                result.Add(format.Stereo ? 1 : 0);
            }

            // NOTE: Format is key: value, nothing in the spec specify if the end marker follow or not this format.
            // BODY: As such, we add an extra 0 just to be sure we don't break anything.
            result.Add(0);
            result.Add(0);

            return result;
        }

        public static IntPtr SelectFBConfig(IntPtr display, FramebufferFormat format)
        {
            List<int> visualAttribute = FramebufferFormatToVisualAttribute(format);

            IntPtr result = IntPtr.Zero;

            // TODO: make screen configurable?
            int screen = DefaultScreenLocked(display);
            unsafe
            {
                int fbCount;

                IntPtr* fbConfigs = GLX.ChooseFBConfig(display, screen, visualAttribute.ToArray(), out fbCount);

                if (fbCount > 0 && fbConfigs != null)
                {
                    result = *fbConfigs;

                    Free((IntPtr)fbConfigs);
                }
            }

            return result;
        }

        public static List<int> GetContextCreationARBAttribute(OpenGLContextBase context)
        {
            List<int> result = new List<int>();

            result.Add((int)GLX.ARB.CreateContext.MAJOR_VERSION);
            result.Add(context.Major);

            result.Add((int)GLX.ARB.CreateContext.MINOR_VERSION);
            result.Add(context.Minor);

            if (context.Flags != 0)
            {
                if (context.Flags.HasFlag(OpenGLContextFlags.Debug))
                {
                    result.Add((int)GLX.ARB.CreateContext.FLAGS);
                    result.Add((int)GLX.ARB.ContextFlags.DEBUG_BIT);
                }


                result.Add((int)GLX.ARB.CreateContext.PROFILE_MASK);

                if (context.Flags.HasFlag(OpenGLContextFlags.Compat))
                {
                    result.Add((int)GLX.ARB.ContextProfileFlags.COMPATIBILITY_PROFILE);
                }
                else
                {
                    result.Add((int)GLX.ARB.ContextProfileFlags.CORE_PROFILE);
                }
            }

            // NOTE: Format is key: value, nothing in the spec specify if the end marker follow or not this format.
            // BODY: As such, we add an extra 0 just to be sure we don't break anything.
            result.Add(0);
            result.Add(0);

            return result;
        }
    }
}