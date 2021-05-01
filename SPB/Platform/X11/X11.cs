using System;
using System.Runtime.InteropServices;

using Display = System.IntPtr;
using Window = System.IntPtr;

namespace SPB.Platform.X11
{
    public sealed class X11
    {
        private const string LibraryName = "libX11";

        public static object Lock = new object();

        public static int DefaultScreen { get; private set; }
        public static IntPtr DefaultDisplay { get; private set; }

        private static XErrorHandler _errorHandlerDelegate = ErrorHandler;

        [DllImport(LibraryName, EntryPoint = "XInitThreads")]
        private extern static int InitThreads();


        [DllImport(LibraryName, EntryPoint = "XOpenDisplay")]
        private extern static IntPtr OpenDisplayLocked(Display display);

        public static IntPtr OpenDisplay(Display display)
        {
            lock (Lock)
            {
                return OpenDisplayLocked(display);
            }
        }

        [DllImport(LibraryName, EntryPoint = "XCloseDisplay")]
        private extern static int CloseDisplayLocked(Display display);

        public static int CloseDisplay(Display display)
        {
            lock (Lock)
            {
                return CloseDisplayLocked(display);
            }
        }

        [DllImport(LibraryName, EntryPoint = "XDefaultScreen")]
        public extern static int DefaultScreenLocked(Display display);

        [DllImport(LibraryName, EntryPoint = "XRootWindow")]
        public extern static Window RootWindow(Display display, int screenNumber);

        [DllImport(LibraryName, EntryPoint = "XFree")]
        public extern static int Free(IntPtr data);

        [DllImport(LibraryName, EntryPoint = "XCreateWindow")]
        public extern static IntPtr CreateWindow(Display display, Window parent, int x, int y, int width, int height, int borderWidth, int depth, int xclass, IntPtr visual, IntPtr valueMask, IntPtr attributes);

        [DllImport(LibraryName, EntryPoint = "XDestroyWindow")]
        public extern static void DestroyWindow(Display display, Window window);

        [DllImport(LibraryName, EntryPoint = "XMapWindow")]
        public extern static void MapWindow(Display display, Window window);

        [DllImport(LibraryName, EntryPoint = "XUnmapWindow")]
        public extern static int UnmapWindow(Display display, Window window);

        [DllImport(LibraryName, EntryPoint = "XCreateColormap")]
        public static extern IntPtr CreateColormap(Display display, Window window, IntPtr visual, int alloc);

        [DllImport(LibraryName, EntryPoint = "XGetXCBConnection")]
        public extern static IntPtr GetXCBConnection(Display display);

        public enum XEventName
        {
            KeyPress = 2,
            KeyRelease = 3,
            ButtonPress = 4,
            ButtonRelease = 5,
            MotionNotify = 6,
            EnterNotify = 7,
            LeaveNotify = 8,
            FocusIn = 9,
            FocusOut = 10,
            KeymapNotify = 11,
            Expose = 12,
            GraphicsExpose = 13,
            NoExpose = 14,
            VisibilityNotify = 15,
            CreateNotify = 16,
            DestroyNotify = 17,
            UnmapNotify = 18,
            MapNotify = 19,
            MapRequest = 20,
            ReparentNotify = 21,
            ConfigureNotify = 22,
            ConfigureRequest = 23,
            GravityNotify = 24,
            ResizeRequest = 25,
            CirculateNotify = 26,
            CirculateRequest = 27,
            PropertyNotify = 28,
            SelectionClear = 29,
            SelectionRequest = 30,
            SelectionNotify = 31,
            ColormapNotify = 32,
            ClientMessage = 33,
            MappingNotify = 34,
            GenericEvent = 35,

            LASTEvent
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XErrorEvent
        {
            public XEventName type;
            public Display display;
            public IntPtr resourceid;
            public IntPtr serial;
            public byte error_code;
            public XRequest request_code;
            public byte minor_code;
        }

        public delegate int XErrorHandler(Display displayHandle, ref XErrorEvent errorEvent);

        public enum XRequest : byte
        {
            X_CreateWindow = 1,
            X_ChangeWindowAttributes = 2,
            X_GetWindowAttributes = 3,
            X_DestroyWindow = 4,
            X_DestroySubwindows = 5,
            X_ChangeSaveSet = 6,
            X_ReparentWindow = 7,
            X_MapWindow = 8,
            X_MapSubwindows = 9,
            X_UnmapWindow = 10,
            X_UnmapSubwindows = 11,
            X_ConfigureWindow = 12,
            X_CirculateWindow = 13,
            X_GetGeometry = 14,
            X_QueryTree = 15,
            X_InternAtom = 16,
            X_GetAtomName = 17,
            X_ChangeProperty = 18,
            X_DeleteProperty = 19,
            X_GetProperty = 20,
            X_ListProperties = 21,
            X_SetSelectionOwner = 22,
            X_GetSelectionOwner = 23,
            X_ConvertSelection = 24,
            X_SendEvent = 25,
            X_GrabPointer = 26,
            X_UngrabPointer = 27,
            X_GrabButton = 28,
            X_UngrabButton = 29,
            X_ChangeActivePointerGrab = 30,
            X_GrabKeyboard = 31,
            X_UngrabKeyboard = 32,
            X_GrabKey = 33,
            X_UngrabKey = 34,
            X_AllowEvents = 35,
            X_GrabServer = 36,
            X_UngrabServer = 37,
            X_QueryPointer = 38,
            X_GetMotionEvents = 39,
            X_TranslateCoords = 40,
            X_WarpPointer = 41,
            X_SetInputFocus = 42,
            X_GetInputFocus = 43,
            X_QueryKeymap = 44,
            X_OpenFont = 45,
            X_CloseFont = 46,
            X_QueryFont = 47,
            X_QueryTextExtents = 48,
            X_ListFonts = 49,
            X_ListFontsWithInfo = 50,
            X_SetFontPath = 51,
            X_GetFontPath = 52,
            X_CreatePixmap = 53,
            X_FreePixmap = 54,
            X_CreateGC = 55,
            X_ChangeGC = 56,
            X_CopyGC = 57,
            X_SetDashes = 58,
            X_SetClipRectangles = 59,
            X_FreeGC = 60,
            X_ClearArea = 61,
            X_CopyArea = 62,
            X_CopyPlane = 63,
            X_PolyPoint = 64,
            X_PolyLine = 65,
            X_PolySegment = 66,
            X_PolyRectangle = 67,
            X_PolyArc = 68,
            X_FillPoly = 69,
            X_PolyFillRectangle = 70,
            X_PolyFillArc = 71,
            X_PutImage = 72,
            X_GetImage = 73,
            X_PolyText8 = 74,
            X_PolyText16 = 75,
            X_ImageText8 = 76,
            X_ImageText16 = 77,
            X_CreateColormap = 78,
            X_FreeColormap = 79,
            X_CopyColormapAndFree = 80,
            X_InstallColormap = 81,
            X_UninstallColormap = 82,
            X_ListInstalledColormaps = 83,
            X_AllocColor = 84,
            X_AllocNamedColor = 85,
            X_AllocColorCells = 86,
            X_AllocColorPlanes = 87,
            X_FreeColors = 88,
            X_StoreColors = 89,
            X_StoreNamedColor = 90,
            X_QueryColors = 91,
            X_LookupColor = 92,
            X_CreateCursor = 93,
            X_CreateGlyphCursor = 94,
            X_FreeCursor = 95,
            X_RecolorCursor = 96,
            X_QueryBestSize = 97,
            X_QueryExtension = 98,
            X_ListExtensions = 99,
            X_ChangeKeyboardMapping = 100,
            X_GetKeyboardMapping = 101,
            X_ChangeKeyboardControl = 102,
            X_GetKeyboardControl = 103,
            X_Bell = 104,
            X_ChangePointerControl = 105,
            X_GetPointerControl = 106,
            X_SetScreenSaver = 107,
            X_GetScreenSaver = 108,
            X_ChangeHosts = 109,
            X_ListHosts = 110,
            X_SetAccessControl = 111,
            X_SetCloseDownMode = 112,
            X_KillClient = 113,
            X_RotateProperties = 114,
            X_ForceScreenSaver = 115,
            X_SetPointerMapping = 116,
            X_GetPointerMapping = 117,
            X_SetModifierMapping = 118,
            X_GetModifierMapping = 119,
            X_NoOperation = 127
        }

        [DllImport(LibraryName, EntryPoint = "XSetErrorHandler")]
        public extern static IntPtr SetErrorHandler(XErrorHandler error_handler);

        public enum Gravity
        {
            ForgetGravity = 0,
            NorthWestGravity = 1,
            NorthGravity = 2,
            NorthEastGravity = 3,
            WestGravity = 4,
            CenterGravity = 5,
            EastGravity = 6,
            SouthWestGravity = 7,
            SouthGravity = 8,
            SouthEastGravity = 9,
            StaticGravity = 10
        }

        [Flags]
        public enum SetWindowValueMask
        {
            Nothing = 0,
            BackPixmap = 1,
            BackPixel = 2,
            BorderPixmap = 4,
            BorderPixel = 8,
            BitGravity = 16,
            WinGravity = 32,
            BackingStore = 64,
            BackingPlanes = 128,
            BackingPixel = 256,
            OverrideRedirect = 512,
            SaveUnder = 1024,
            EventMask = 2048,
            DontPropagate = 4096,
            ColorMap = 8192,
            Cursor = 16384
        }

        public enum CreateWindowArgs
        {
            CopyFromParent = 0,
            ParentRelative = 1,
            InputOutput = 1,
            InputOnly = 2
        }

        public struct XVisualInfo
        {
            public IntPtr Visual;
            public ulong VisualId;
            public int Screen;
            public int Depth;
            public int Class;
            public ulong RedMask;
            public ulong GreenMask;
            public ulong BlueMask;
            public int ColorMapSize;
            public int BitsPerRgb;
        }

        public struct XSetWindowAttributes
        {
            public IntPtr BackgroundPixmap;
            public IntPtr BackgroundPixel;
            public IntPtr BorderPixmap;
            public IntPtr BorderPixel;
            public Gravity BitGravity;
            public Gravity WinGravity;
            public int BackingStore;
            public IntPtr BackingPlanes;
            public IntPtr BackingPixel;
            public bool SaveUnder;
            public IntPtr EventMask;
            public IntPtr DoNotPropagateMask;
            public bool OverrideRediection;
            public IntPtr ColorMap;
            public IntPtr Cursor;
        }

        static int ErrorHandler(Display displayHandle, ref XErrorEvent errorEvent)
        {
            Console.WriteLine($"XError: {errorEvent.type} result is {errorEvent.error_code}");
            Console.Out.Flush();
            return 0;
        }

        static X11()
        {
            InitThreads();

            SetErrorHandler(_errorHandlerDelegate);

            DefaultDisplay = OpenDisplay(IntPtr.Zero);
            DefaultScreen = 0;

            if (DefaultDisplay == IntPtr.Zero)
            {
                throw new Exception("Cannot connect to X server!");
            }
        }
    }
}