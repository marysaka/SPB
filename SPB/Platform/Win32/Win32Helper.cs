using OpenTK.Core.Platform;
using SPB.Platform.WGL;
using SPB.Windowing;
using System;
using System.Runtime.InteropServices;
using static SPB.Platform.Win32.Win32;

namespace SPB.Platform.Win32
{
    public class Win32Helper
    {
        private static bool _isInit = false;

        private static ushort classRegistrationAtom = 0;

        internal const string ClassName = "SPB.Win32";

        internal static void EnsureInit()
        {
            if (!_isInit)
            {
                WNDCLASSEX cl = WNDCLASSEX.Create();

                cl.lpszClassName = ClassName;
                cl.hInstance = GetModuleHandle(null);
                cl.style = ClassStyles.CS_OWNDC;
                cl.lpfnWndProc = WindowProc;

                classRegistrationAtom = RegisterClassEx(ref cl);

                if (classRegistrationAtom == 0)
                {
                    throw new PlatformException($"RegisterClassEx failed: {Marshal.GetLastWin32Error()}");
                }

                _isInit = true;
            }
        }

        private static IntPtr WindowProc(IntPtr hWnd, WindowsMessages msg, IntPtr wParam, IntPtr lParam)
        {
            return DefWindowProc(hWnd, msg, wParam, lParam);
        }

        internal static IntPtr CreateNativeWindow(WindowStylesEx stylesEx, WindowStyles style, string name, int x, int y, int width, int height)
        {
            EnsureInit();

            return CreateWindowEx(stylesEx,
                                  ClassName,
                                  name,
                                  style,
                                  x, y, width, height, IntPtr.Zero, IntPtr.Zero, GetModuleHandle(null), IntPtr.Zero);
        }

        // TODO: support custom display
        public static SimpleWin32Window CreateSimpleWindow(int x, int y, int width, int height)
        {
            EnsureInit();

            IntPtr handle = CreateNativeWindow(WindowStylesEx.WS_EX_APPWINDOW | WindowStylesEx.WS_EX_TOPMOST,
                                           WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_CLIPCHILDREN,
                                           "SPB no name",
                                           x, y, width, height);

            if (handle == IntPtr.Zero)
            {
                throw new PlatformException($"CreateWindowEx failed: {Marshal.GetLastWin32Error()}");
            }

            return new SimpleWin32Window(new NativeHandle(handle));
        }

        // TODO: support custom display
        public static WGLWindow CreateWindowForWGL(int x, int y, int width, int height)
        {
            EnsureInit();

            IntPtr handle = CreateNativeWindow(WindowStylesEx.WS_EX_APPWINDOW | WindowStylesEx.WS_EX_TOPMOST,
                                           WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_CLIPCHILDREN,
                                           "SPB no name",
                                           x, y, width, height);

            if (handle == IntPtr.Zero)
            {
                throw new PlatformException($"CreateWindowEx failed: {Marshal.GetLastWin32Error()}");
            }

            return new WGLWindow(new NativeHandle(handle));
        }
    }
}
