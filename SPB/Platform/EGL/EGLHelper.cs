using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using SPB.Graphics;
using SPB.Graphics.OpenGL;

namespace SPB.Platform.EGL
{
    [SupportedOSPlatform("linux")]
    public sealed class EGLHelper
    {
        public static IntPtr _eglDisplay;
        private static bool init;

        public IntPtr eglDisplay {
            get {
                return _eglDisplay;
            }
        }

        public EGLHelper(IntPtr display) {
            if (!init) {
                unsafe {
                    _eglDisplay = EGL.GetPlatformDisplay((int)EGL.Attribute.PLATFORM_X11_KHR, display, (IntPtr *)IntPtr.Zero.ToPointer());
                }
                EGL.Initialize(eglDisplay, IntPtr.Zero, IntPtr.Zero);
                init = true;
            }
        }

        public static void BindApi() {
            EGL.BindApi((int) EGL.Attribute.OPENGL_API);
        }

        public static List<int> FramebufferFormatToVisualAttribute(FramebufferFormat format)
        {
            List<int> result = new List<int>();

            result.Add((int)EGL.Attribute.COLOR_BUFFER_TYPE);
            result.Add((int)EGL.Attribute.RGB_BUFFER);

            result.Add((int)EGL.Attribute.SURFACE_TYPE);
            result.Add((int)EGL.Attribute.WINDOW_BIT);

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
                result.Add((int)EGL.Attribute.SAMPLES);
                result.Add((int)format.Samples);
            }

            result.Add((int)EGL.Attribute.NONE);

            return result;
        }

        public unsafe IntPtr eglWindowSurface(IntPtr nativeWindowHandle, IntPtr fbConfig) {
            return EGL.CreateWindowSurface(eglDisplay, fbConfig, nativeWindowHandle, (IntPtr *)IntPtr.Zero.ToPointer());
        }

        public IntPtr SelectFBConfig(FramebufferFormat format)
        {
            List<int> visualAttribute = FramebufferFormatToVisualAttribute(format);

            IntPtr result = IntPtr.Zero;

            unsafe
            {
                int configCnt = 0;
                int[] attribs = visualAttribute.ToArray();
               
                fixed (int* attr = &attribs[0]) {
                    uint res = EGL.ChooseConfig(eglDisplay, attr, (IntPtr *)IntPtr.Zero.ToPointer(), 0, &configCnt);

                    if (configCnt == 0 || res == 0)
                    {
                        return IntPtr.Zero;
                    }

                    fixed (IntPtr* fbConfig = new IntPtr[configCnt]) {
                        EGL.ChooseConfig(eglDisplay, attr, fbConfig, configCnt, &configCnt);
                        result = fbConfig[0];
                    }
                }
            }

            return result;
        }

        public List<int> GetContextCreationARBAttribute(OpenGLContextBase context)
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
                    result.Add((int)EGL.ARB.ContextFlags.DEBUG);
                    result.Add(1);
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
