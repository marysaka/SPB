using System;
using System.Runtime.InteropServices;

using Display = System.IntPtr;
using Config = System.IntPtr;
using Surface = System.IntPtr;
using Context = System.IntPtr;
using System.Runtime.Versioning;

namespace SPB.Platform.EGL
{
    [SupportedOSPlatform("linux")]
    internal sealed class EGL
    {
        private const string LibraryName = "libEGL.dll";

        static EGL()
        {
            NativeLibrary.SetDllImportResolver(typeof(EGL).Assembly, (name, assembly, path) =>
            {
                if (name != LibraryName)
                {
                    return IntPtr.Zero;
                }

                if (!NativeLibrary.TryLoad("libEGL.so.1", assembly, path, out IntPtr result))
                {
                    if (!NativeLibrary.TryLoad("libEGL.so", assembly, path, out result))
                    {
                        return IntPtr.Zero;
                    }
                }

                return result;
            });
        }

        [DllImport(LibraryName, EntryPoint = "eglChooseConfig")]
        public unsafe extern static uint ChooseConfig(Display display, int* attributes, Config* configs, int config_size, int* num_config);

        [DllImport(LibraryName, EntryPoint = "eglGetConfigAttrib")]
        public unsafe extern static uint GetConfigAttrib(Display display, Config config, int attribute, out int value);

        [DllImport(LibraryName, EntryPoint = "eglDestroyContext")]
        public static extern uint DestroyContext(Display display, Context context);

        [DllImport(LibraryName, EntryPoint = "eglGetCurrentContext")]
        public static extern Context GetCurrentContext();

        [DllImport(LibraryName, EntryPoint = "eglSwapBuffers")]
        public static extern uint SwapBuffers(Display display, Surface drawable);

        [DllImport(LibraryName, EntryPoint = "eglMakeCurrent")]
        public static extern uint MakeCurrent(Display display, Surface drawable, Surface readable, Context context);

        [DllImport(LibraryName, EntryPoint = "eglBindAPI")]
        public static extern uint BindApi(int api);

        [DllImport(LibraryName, EntryPoint = "eglInitialize")]
        public static extern uint Initialize(Display display, IntPtr major, IntPtr minor);

        [DllImport(LibraryName, EntryPoint = "eglGetPlatformDisplay")]
        public unsafe static extern Display GetPlatformDisplay(int platform, IntPtr nativeDisplay, IntPtr* attribList);

        [DllImport(LibraryName, EntryPoint = "eglCreateWindowSurface")]
        public unsafe static extern Surface CreateWindowSurface(Display display, Config config, IntPtr nativeWindow, IntPtr* attribList);

        [DllImport(LibraryName, EntryPoint = "eglDestroySurface")]
        public unsafe static extern uint DestroySurface(Display display, Surface surface);

        [DllImport(LibraryName, EntryPoint = "eglGetError")]
        public static extern int GetError();

        internal enum Attribute : int
        {
            OPENGL_API = 0x30A2,
            COLOR_BUFFER_TYPE = 0x303F,
            RGB_BUFFER = 0x308E,
            CONFIG_CAVEAT = 0x3027,
            RENDERABLE_TYPE = 0x3040,
            NATIVE_VISUAL_ID = 0x302E,
            PLATFORM_X11_KHR = 0x31D5,
            OPENGL_BIT = 0x0008,
            CONFORMANT = 0x3042,
            RED_SIZE = 0x3024,
            GREEN_SIZE = 0x3023,
            BLUE_SIZE = 0x3022,
            ALPHA_SIZE = 0x3021,
            DEPTH_SIZE = 0x3025,
            STENCIL_SIZE = 0x3026,
            SAMPLE_BUFFERS = 0x3032,
            SAMPLES = 0x3031,
            SURFACE_TYPE = 0x3033,
            WINDOW_BIT = 0x4,
            PBUFFER_BIT = 0x0001,
            NONE = 0x3038
        }

        internal enum RenderTypeMask : int
        {
            COLOR_INDEX_BIT_SGIX = 0x00000002,
            RGBA_BIT = 0x00000001,
            RGBA_FLOAT_BIT_ARB = 0x00000004,
            RGBA_BIT_SGIX = 0x00000001,
            COLOR_INDEX_BIT = 0x00000002,
        }

        public enum ErrorCode : int
        {
            SUCCESS = 0x3000,
            NOT_INITIALIZED = 0x3001,
            BAD_ACCESS = 0x3002,
            BAD_ALLOC = 0x3003,
            BAD_ATTRIBUTE = 0x3004,
            BAD_CONFIG = 0x3005,
            BAD_CONTEXT = 0x3006,
            BAD_CURRENT_SURFACE = 0x3007,
            BAD_DISPLAY = 0x3008,
            BAD_MATCH = 0x3009,
            BAD_NATIVE_PIXMAP = 0x300A,
            BAD_NATIVE_WINDOW = 0x300B,
            BAD_PARAMETER = 0x300C,
            BAD_SURFACE = 0x300D,
            CONTEXT_LOST = 0x300E
        }

        internal sealed class ARB
        {
            public enum ContextFlags : int
            {
                DEBUG = 0x31B0
            }

            public enum ContextProfileFlags : int
            {
                CORE_PROFILE = 0x1,
                COMPATIBILITY_PROFILE = 0x2,
            }

            public enum CreateContextAttr : int
            {
                MAJOR_VERSION = 0x3098,
                MINOR_VERSION = 0x30FB,
                FLAGS = 0x30FC,
                PROFILE_MASK = 0x30FD,
            }

            [DllImport(LibraryName, EntryPoint = "eglGetProcAddress")]
            public static extern IntPtr GetProcAddress(string procName);


            [DllImport(LibraryName, EntryPoint = "eglCreateContext")]
            public static extern Context CreateContext(Display display, Config config, Context shareContext, int[] attributes);
        }

        internal sealed class Ext
        {
            [DllImport(LibraryName, EntryPoint = "eglSwapInterval")]
            public static extern ErrorCode SwapInterval(Display display, int interval);
        }

    }
}