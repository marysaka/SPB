using System;
using System.Runtime.InteropServices;

using Display = System.IntPtr;
using Surface = System.IntPtr;
using Config = System.IntPtr;
using Context = System.IntPtr;

namespace SPB.Platform.EGL
{
    public static class EGL
    {
        private const string LibraryName = "egl";

        public static bool IsPresent => PlatformHelper.IsLibraryAvailable(LibraryName);

        static EGL()
        {
            PlatformHelper.EnsureResolverRegistered();
        }

        [DllImport(LibraryName, EntryPoint = "eglGetDisplay")]
        // NOTE: By spec nativeDisplay is supposed to be an opaque type, however it is always a pointer on all platform except EMS.
        public static unsafe extern Display GetDisplay(IntPtr nativeDisplay);

        [DllImport(LibraryName, EntryPoint = "eglInitialize")]
        public static unsafe extern bool Initialize(Display display, out int major, out int minor);

        [DllImport(LibraryName, EntryPoint = "eglTerminate")]
        public static unsafe extern bool Terminate(Display display);

        [DllImport(LibraryName, EntryPoint = "eglChooseConfig")]
        public static unsafe extern bool ChooseConfig(Display display, int[] attributes, IntPtr configs, int configSize, out int numConfig);


        [DllImport(LibraryName, EntryPoint = "eglGetConfigAttrib")]
        public static unsafe extern bool GetConfigAttrib(Display display, IntPtr config, int attribute, out int value);


        [DllImport(LibraryName, EntryPoint = "eglCreateWindowSurface")]
        public static unsafe extern IntPtr CreateWindowSurface(Display display, IntPtr config, IntPtr nativeWindow, int[] attributes);

        [DllImport(LibraryName, EntryPoint = "eglDestroySurface")]
        public static extern bool DestroySurface(Display display, Surface surface);

        [DllImport(LibraryName, EntryPoint = "eglSwapBuffers")]
        public static extern bool SwapBuffers(Display display, Surface surface);

        public enum ErrorCode : int
        {
            NO_ERROR = 0x3000,
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

        [DllImport(LibraryName, EntryPoint = "eglGetError")]
        public static unsafe extern ErrorCode GetError();

        public enum QueryStringName : int
        {
            VENDOR = 0x3053,
            VERSION = 0x3054,
            EXTENSIONS = 0x3055
        }

        [DllImport(LibraryName, EntryPoint = "eglQueryString")]
        private static unsafe extern IntPtr QueryStringInternal(Display display, QueryStringName name);

        public static string QueryString(Display display, QueryStringName name)
        {
            IntPtr result = QueryStringInternal(display, name);

            if (result == IntPtr.Zero)
            {
                return null;
            }

            return Marshal.PtrToStringAnsi(result);
        }

        public enum ApiType : int
        {
            OPENGL_ES_API = 0x30A0,
            OPENVG_API = 0x30A1,
            OPENGL_API = 0x30A2
        }

        [DllImport(LibraryName, EntryPoint = "eglBindAPI")]
        public static unsafe extern bool BindAPI(ApiType api);

        public enum ConfigAttribute : int
        {
            WINDOW_BIT = 0x4,
            OPENGL_BIT = 0x8,
            GL_COLORSPACE_LINEAR = 0x308A,

            BUFFER_SIZE = 0x3020,
            ALPHA_SIZE = 0x3021,
            BLUE_SIZE = 0x3022,
            GREEN_SIZE = 0x3023,
            RED_SIZE = 0x3024,
            DEPTH_SIZE = 0x3025,
            STENCIL_SIZE = 0x3026,
            CONFIG_CAVEAT = 0x3027,
            NATIVE_VISUAL_ID = 0x302E,
            SAMPLES = 0x3031,
            SAMPLE_BUFFERS  = 0x3032,
            SURFACE_TYPE = 0x3033,
            NONE = 0x3038,
            COLOR_BUFFER_TYPE = 0x303F,
            RENDERABLE_TYPE = 0x3040,
            CONFORMANT = 0x3042,
            RGB_BUFFER = 0x308E,
            SINGLE_BUFFER = 0x3085,
            RENDER_BUFFER = 0x3086,
            GL_COLORSPACE = 0x309D
        }

        public enum CreateContextAttribute : int
        {
            NONE = 0x3038,
            MAJOR_VERSION = 0x3098,
            MINOR_VERSION = 0x30FB,
            FLAGS = 0x30FC,
            PROFILE_MASK = 0x30FD,
        }

        public enum CreateContextFlags : int
        {
            DEBUG_BIT = 0x1,
            FORWARD_COMPATIBLE_BIT = 0x2,
        }

        public enum CreateContextProfileFlags : int
        {
            CORE_PROFILE = 0x1,
            COMPATIBILITY_PROFILE = 0x2,
        }

        [DllImport(LibraryName, EntryPoint = "eglCreateContext")]
        public static extern Context CreateContext(Display display, Config config, Context shareContext, int[] attributes);
        
        [DllImport(LibraryName, EntryPoint = "eglDestroyContext")]
        public static extern bool DestroyContext(Display display, Context context);

        [DllImport(LibraryName, EntryPoint = "eglGetCurrentContext")]
        public static extern Context GetCurrentContext();

        [DllImport(LibraryName, EntryPoint = "eglMakeCurrent")]
        public static extern bool MakeCurrent(Display display, Surface draw, Surface read, Context context);

        [DllImport(LibraryName, EntryPoint = "eglGetProcAddress")]
        public static extern IntPtr GetProcAddress(string procName);

        [DllImport(LibraryName, EntryPoint = "eglSwapInterval")]
        public static extern bool SwapInterval(Display display, int interval);
    }
}
