using SPB.Platform.Exceptions;
using SPB.Platform.Win32;
using SPB.Platform.X11;
using SPB.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using SPB.Platform.Cocoa;

namespace SPB.Graphics.Vulkan
{
    public static class VulkanHelper
    {
        private static bool _isInit = false;

        private static IntPtr _vulkanHandle;

        // Platform specific library hints.
        // For Win32:
        private static readonly string[] VulkanLibraryNameWindows =
        {
            "vulkan-1.dll"
        };

        // For Linux:
        private static readonly string[] VulkanLibraryNameLinux =
        {
            "libvulkan.so.1"
        };

        // For macOS: try MoltenVK first (see: https://github.com/KhronosGroup/MoltenVK),
        // then try libvulkan as a last chance option.
        private static readonly string[] VulkanLibraryNameMacOS =
        {
            "libMoltenVk.dylib",
            "libvulkan.dylib"
        };

        private static string[] _extensions;

        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
        private delegate IntPtr vkGetInstanceProcAddrDelegate(IntPtr instance, string name);

        private static vkGetInstanceProcAddrDelegate _vkGetInstanceProcAddr;

        private unsafe struct VkExtensionProperty
        {
            public fixed byte ExtensionName[0x100];
            public uint SpecVersion;
        }

        // Extensions related structure type IDs.
        // See: https://github.com/KhronosGroup/Vulkan-Headers/blob/main/include/vulkan/vulkan_core.h
        private const uint VK_STRUCTURE_TYPE_XLIB_SURFACE_CREATE_INFO_KHR = 1000004000;
        private const uint VK_STRUCTURE_TYPE_XCB_SURFACE_CREATE_INFO_KHR = 1000005000;
        private const uint VK_STRUCTURE_TYPE_WIN32_SURFACE_CREATE_INFO_KHR = 1000009000;
        private const uint VK_STRUCTURE_TYPE_MACOS_SURFACE_CREATE_INFO_MVK = 1000123000;
        private const uint VK_STRUCTURE_TYPE_METAL_SURFACE_CREATE_INFO_EXT = 1000217000;

        private unsafe struct VkWin32SurfaceCreateInfoKHR
        {
            public uint StructType;
            public IntPtr Next;
            public uint Flags;
            public IntPtr HInstance;
            public IntPtr Hwnd;
        };

        private unsafe struct VkXlibSurfaceCreateInfoKHR
        {
            public uint StructType;
            public IntPtr Next;
            public uint Flags;
            public IntPtr Display;
            public IntPtr Window;
        }

        private unsafe struct VkXcbSurfaceCreateInfoKHR
        {
            public uint StructType;
            public IntPtr Next;
            public uint Flags;
            public IntPtr Connection;
            public IntPtr Window;
        }

        // See: https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkMacOSSurfaceCreateInfoMVK.html
        private unsafe struct VkMacOSSurfaceCreateInfoMVK
        {
            public uint StructType; // type of this structure
            public IntPtr Next; // IntPtr.Zero or pointer to extending structure
            public uint Flags; // reserved
            public IntPtr View; // pointer to CAMetalLayer or NSView
        }

        // See: https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/vkCreateMetalSurfaceEXT.html
        private unsafe struct VkMetalSurfaceCreateInfoEXT
        {
            public uint StructType; // type of this structure
            public IntPtr Next; // IntPtr.Zero or pointer to extending structure
            public uint Flags; // reserved
            public IntPtr View; // pointer to CAMetalLayer
        }

        private unsafe delegate int vkEnumerateInstanceExtensionPropertiesDelegate(string layerName,
            out uint layerCount, VkExtensionProperty* properties);

        private unsafe delegate int vkCreateWin32SurfaceKHRDelegate(IntPtr instance,
            ref VkWin32SurfaceCreateInfoKHR createInfo, IntPtr allocator, out IntPtr surface);

        private unsafe delegate int vkCreateXlibSurfaceKHRDelegate(IntPtr instance,
            ref VkXlibSurfaceCreateInfoKHR createInfo, IntPtr allocator, out IntPtr surface);

        private unsafe delegate int vkCreateXcbSurfaceKHRDelegate(IntPtr instance,
            ref VkXcbSurfaceCreateInfoKHR createInfo, IntPtr allocator, out IntPtr surface);

        private unsafe delegate int vkCreateMacOSSurfaceMVKDelegate(IntPtr instance,
            ref VkMacOSSurfaceCreateInfoMVK createInfo, IntPtr allocator, out IntPtr surface);

        private unsafe delegate int vkCreateMetalSurfaceEXTDelegate(IntPtr instance,
            ref VkMetalSurfaceCreateInfoEXT createInfo, IntPtr allocator, out IntPtr surface);

        private static vkEnumerateInstanceExtensionPropertiesDelegate _vkEnumerateInstanceExtensionProperties;

        private static string[] GetLibraryNameHints()
        {
            if (OperatingSystem.IsWindows())
            {
                return VulkanLibraryNameWindows;
            }
            else if (OperatingSystem.IsLinux())
            {
                return VulkanLibraryNameLinux;
            }
            else if (OperatingSystem.IsMacOS())
            {
                return VulkanLibraryNameMacOS;
            }

            return null;
        }

        /// <summary>
        /// This method attempts to load any suitable Vulkan library.
        /// If multiple options are available,
        /// handle to the first successfully loaded library is returned.
        /// </summary>
        /// <param name="vulkanHandle">Library handle to set if any library is loaded</param>
        /// <returns>true if library was successfully loaded, false otherwise</returns>
        private static bool TryLoadLibrary(out IntPtr vulkanHandle)
        {
            var libraryNameHints = GetLibraryNameHints();
            vulkanHandle = IntPtr.Zero;

            if (libraryNameHints == null)
            {
                return false;
            }

            // Returns on the first successfully loaded library.
            foreach (var libraryName in libraryNameHints)
            {
                if (NativeLibrary.TryLoad(libraryName, out vulkanHandle))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsExtensionPresent(string extensionName)
        {
            return _extensions.Contains(extensionName);
        }

        private static unsafe string GetStringFromUtf8Byte(byte* start)
        {
            byte* end = start;
            while (*end != 0) end++;

            return Encoding.UTF8.GetString(start, (int)(end - start));
        }

        private static void EnsureInit()
        {
            if (_isInit)
            {
                return;
            }

            if (!TryLoadLibrary(out _vulkanHandle) ||
                !NativeLibrary.TryGetExport(
                    _vulkanHandle,
                    "vkGetInstanceProcAddr",
                    out IntPtr vkGetInstanceProcAddrPtr)
                )
            {
                throw new NotSupportedException("Unsupported platform for Vulkan!");
            }


            _vkGetInstanceProcAddr = Marshal.GetDelegateForFunctionPointer<vkGetInstanceProcAddrDelegate>(
                vkGetInstanceProcAddrPtr
            );
            _vkEnumerateInstanceExtensionProperties =
                Marshal.GetDelegateForFunctionPointer<vkEnumerateInstanceExtensionPropertiesDelegate>(
                    _vkGetInstanceProcAddr(IntPtr.Zero, "vkEnumerateInstanceExtensionProperties")
                );

            unsafe
            {
                int res = _vkEnumerateInstanceExtensionProperties(null, out uint layerCount, null);

                if (res != 0)
                {
                    throw new PlatformException($"vkEnumerateInstanceExtensionProperties failed: {res}");
                }

                VkExtensionProperty[] extensions = new VkExtensionProperty[layerCount];

                fixed (VkExtensionProperty* extensionsPtr = extensions)
                {
                    res = _vkEnumerateInstanceExtensionProperties(null, out layerCount, extensionsPtr);
                }

                if (res != 0)
                {
                    throw new PlatformException($"vkEnumerateInstanceExtensionProperties failed: {res}");
                }

                _extensions = new string[extensions.Length];

                for (int i = 0; i < extensions.Length; i++)
                {
                    fixed (byte* extensionNamePtr = extensions[i].ExtensionName)
                    {
                        _extensions[i] = GetStringFromUtf8Byte(extensionNamePtr);
                    }
                }
            }

            // Ensure that all extensions that we are requiring are present.

            bool isVulkanUsable = IsExtensionPresent("VK_KHR_surface");

            if (OperatingSystem.IsWindows())
            {
                isVulkanUsable &= IsExtensionPresent("VK_KHR_win32_surface");
            }
            // FIXME: We assume Linux == X11 for now.
            else if (OperatingSystem.IsLinux())
            {
                if (!IsExtensionPresent("VK_KHR_xcb_surface") || !X11.IsXcbAvailable())
                {
                    if (!IsExtensionPresent("VK_KHR_xlib_surface"))
                    {
                        isVulkanUsable = false;
                    }
                }
            }
            else if (OperatingSystem.IsMacOS())
            {
                // See:
                // https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VK_MVK_macos_surface.html
                // https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VK_EXT_metal_surface.html
                isVulkanUsable &= IsExtensionPresent("VK_MVK_macos_surface") | // deprecated
                                  IsExtensionPresent("VK_EXT_metal_surface"); // newer extension
            }

            if (!isVulkanUsable)
            {
                throw new NotSupportedException("No supported Vulkan surface found!");
            }

            _isInit = true;
        }

        public static IntPtr GetInstanceProcAddr(IntPtr instance, string name)
        {
            EnsureInit();

            return _vkGetInstanceProcAddr(instance, name);
        }

        public static string[] GetRequiredInstanceExtensions()
        {
            EnsureInit();

            List<string> extensions = new() { "VK_KHR_surface" };

            if (OperatingSystem.IsWindows())
            {
                extensions.Add("VK_KHR_win32_surface");
            }

            if (OperatingSystem.IsLinux())
            {
                // Prefer XCB as xlib implementation was known to have ICD issues.
                if (IsExtensionPresent("VK_KHR_xcb_surface") && X11.IsXcbAvailable())
                {
                    extensions.Add("VK_KHR_xcb_surface");
                }
                else
                {
                    extensions.Add("VK_KHR_xlib_surface");
                }
            }

            if (OperatingSystem.IsMacOS())
            {
                // prefer a newer extension over deprecated
                extensions.Add(
                    IsExtensionPresent("VK_EXT_metal_surface")
                        ? "VK_EXT_metal_surface"
                        : "VK_MVK_macos_surface"
                );
            }

            return extensions.ToArray();
        }

        /// <summary>
        /// This method creates Vulkan surface on Win32 platform.
        /// </summary>
        /// <param name="vulkanInstance">Instance of Vulkan library</param>
        /// <param name="window">Window native handle</param>
        /// <returns>Reference to surface</returns>
        /// <exception cref="PlatformException">This exception is raised on surface creation error</exception>
        private static IntPtr CreateWindowSurfaceWin32(
            IntPtr vulkanInstance,
            NativeWindowBase window
        )
        {
            vkCreateWin32SurfaceKHRDelegate vkCreateWin32SurfaceKHR =
                Marshal.GetDelegateForFunctionPointer<vkCreateWin32SurfaceKHRDelegate>(
                    _vkGetInstanceProcAddr(vulkanInstance, "vkCreateWin32SurfaceKHR"
                    )
                );

            VkWin32SurfaceCreateInfoKHR creationInfo = new VkWin32SurfaceCreateInfoKHR
            {
                StructType = VK_STRUCTURE_TYPE_WIN32_SURFACE_CREATE_INFO_KHR,
                Next = IntPtr.Zero,
                Flags = 0,
// Broken warning here there is no platform issues here...
#pragma warning disable CA1416
                HInstance = Win32.GetWindowLong(
                    window.WindowHandle.RawHandle,
                    Win32.GetWindowLongIndex.GWL_HINSTANCE
                ),
#pragma warning restore CA1416
                Hwnd = window.WindowHandle.RawHandle
            };

            int res = vkCreateWin32SurfaceKHR(
                vulkanInstance,
                ref creationInfo,
                IntPtr.Zero,
                out IntPtr surface
            );

            if (res != 0)
            {
                throw new PlatformException($"vkCreateWin32SurfaceKHR failed: {res}");
            }

            return surface;
        }

        /// <summary>
        /// This method creates Vulkan surface on Linux (XCB) platform.
        /// </summary>
        /// <param name="vulkanInstance">Instance of Vulkan library</param>
        /// <param name="window">Window native handle</param>
        /// <returns>Reference to surface</returns>
        /// <exception cref="PlatformException">This exception is raised on surface creation error</exception>
        [SupportedOSPlatform("linux")]
        private static IntPtr CreateWindowSurfaceXCB(IntPtr vulkanInstance, NativeWindowBase window)
        {
            vkCreateXcbSurfaceKHRDelegate vkCreateXcbSurfaceKHR =
                Marshal.GetDelegateForFunctionPointer<vkCreateXcbSurfaceKHRDelegate>(
                    _vkGetInstanceProcAddr(vulkanInstance, "vkCreateXcbSurfaceKHR"
                    )
                );

            VkXcbSurfaceCreateInfoKHR creationInfo = new VkXcbSurfaceCreateInfoKHR
            {
                StructType = VK_STRUCTURE_TYPE_XCB_SURFACE_CREATE_INFO_KHR,
                Next = IntPtr.Zero,
                Flags = 0,
                Connection = X11.GetXCBConnection(window.DisplayHandle.RawHandle),
                Window = window.WindowHandle.RawHandle
            };

            int res = vkCreateXcbSurfaceKHR(
                vulkanInstance,
                ref creationInfo,
                IntPtr.Zero,
                out IntPtr surface
            );

            if (res != 0)
            {
                throw new PlatformException($"vkCreateXcbSurfaceKHR failed: {res}");
            }

            return surface;
        }

        /// <summary>
        /// This method creates Vulkan surface on Linux (Xlib) platform.
        /// </summary>
        /// <param name="vulkanInstance">Instance of Vulkan library</param>
        /// <param name="window">Window native handle</param>
        /// <returns>Reference to surface</returns>
        /// <exception cref="PlatformException">This exception is raised on surface creation error</exception>
        private static IntPtr CreateWindowSurfaceXlib(IntPtr vulkanInstance, NativeWindowBase window)
        {
            vkCreateXlibSurfaceKHRDelegate vkCreateXlibSurfaceKHR =
                Marshal.GetDelegateForFunctionPointer<vkCreateXlibSurfaceKHRDelegate>(
                    _vkGetInstanceProcAddr(vulkanInstance, "vkCreateXlibSurfaceKHR"
                    )
                );

            VkXlibSurfaceCreateInfoKHR creationInfo = new VkXlibSurfaceCreateInfoKHR
            {
                StructType = VK_STRUCTURE_TYPE_XLIB_SURFACE_CREATE_INFO_KHR,
                Next = IntPtr.Zero,
                Flags = 0,
                Display = window.DisplayHandle.RawHandle,
                Window = window.WindowHandle.RawHandle
            };

            int res = vkCreateXlibSurfaceKHR(
                vulkanInstance,
                ref creationInfo,
                IntPtr.Zero,
                out IntPtr surface
            );

            if (res != 0)
            {
                throw new PlatformException($"vkCreateXlibSurfaceKHR failed: {res}");
            }

            return surface;
        }

        /// <summary>
        /// This method creates Vulkan surface on macOS platform using deprecated MoltenVK extension.
        /// </summary>
        /// <param name="vulkanInstance">Instance of Vulkan library</param>
        /// <param name="window">Window native handle</param>
        /// <returns>Reference to surface</returns>
        /// <exception cref="PlatformException">This exception is raised on surface creation error</exception>
        [SupportedOSPlatform("macos")]
        private static IntPtr CreateWindowSurfaceMVK(IntPtr vulkanInstance, SimpleCocoaWindow window)
        {
            vkCreateMacOSSurfaceMVKDelegate vkCreateMacOSSurfaceMVK =
                Marshal.GetDelegateForFunctionPointer<vkCreateMacOSSurfaceMVKDelegate>(
                    _vkGetInstanceProcAddr(vulkanInstance, "vkCreateMacOSSurfaceMVK"
                    )
                );

            VkMacOSSurfaceCreateInfoMVK creationInfo = new VkMacOSSurfaceCreateInfoMVK
            {
                StructType = VK_STRUCTURE_TYPE_MACOS_SURFACE_CREATE_INFO_MVK,
                Next = IntPtr.Zero,
                Flags = 0,
                View = window.MetalLayerHandle.RawHandle
            };

            int res = vkCreateMacOSSurfaceMVK(
                vulkanInstance,
                ref creationInfo,
                IntPtr.Zero,
                out IntPtr surface
            );

            if (res != 0)
            {
                throw new PlatformException($"vkCreateMacOSSurfaceMVK failed: {res}");
            }

            return surface;
        }

        /// <summary>
        /// This method creates Vulkan surface on macOS platform using modern MoltenVK extension.
        /// </summary>
        /// <param name="vulkanInstance">Instance of Vulkan library</param>
        /// <param name="window">Window native handle</param>
        /// <returns>Reference to surface</returns>
        /// <exception cref="PlatformException">This exception is raised on surface creation error</exception>
        [SupportedOSPlatform("macos")]
        private static IntPtr CreateWindowSurfaceMetal(IntPtr vulkanInstance, SimpleCocoaWindow window)
        {
            vkCreateMetalSurfaceEXTDelegate vkCreateMetalSurfaceEXT =
                Marshal.GetDelegateForFunctionPointer<vkCreateMetalSurfaceEXTDelegate>(
                    _vkGetInstanceProcAddr(vulkanInstance, "vkCreateMetalSurfaceEXT"
                    )
                );

            VkMetalSurfaceCreateInfoEXT creationInfo = new VkMetalSurfaceCreateInfoEXT
            {
                StructType = VK_STRUCTURE_TYPE_METAL_SURFACE_CREATE_INFO_EXT,
                Next = IntPtr.Zero,
                Flags = 0,
                View = window.MetalLayerHandle.RawHandle
            };

            int res = vkCreateMetalSurfaceEXT(
                vulkanInstance,
                ref creationInfo,
                IntPtr.Zero,
                out IntPtr surface
            );

            if (res != 0)
            {
                throw new PlatformException($"vkCreateMetalSurfaceEXT failed: {res}");
            }

            return surface;
        }

        /// <summary>
        /// This method creates Vulkan surface using platform dependent API.
        /// </summary>
        /// <param name="vulkanInstance">Instance of Vulkan library</param>
        /// <param name="window">Window native handle</param>
        /// <returns>Reference to surface</returns>
        /// <exception cref="NotImplementedException">This exception is raised if platform is not supported</exception>
        public static IntPtr CreateWindowSurface(IntPtr vulkanInstance, NativeWindowBase window)
        {
            EnsureInit();

            if (OperatingSystem.IsWindows() && IsExtensionPresent("VK_KHR_win32_surface"))
            {
                return CreateWindowSurfaceWin32(vulkanInstance, window);
            }

            if (OperatingSystem.IsLinux())
            {
                if (IsExtensionPresent("VK_KHR_xcb_surface") && X11.IsXcbAvailable())
                {
                    return CreateWindowSurfaceXCB(vulkanInstance, window);
                }

                return CreateWindowSurfaceXlib(vulkanInstance, window);
            }

            if (OperatingSystem.IsMacOS())
            {
                var cocoaWindow = (SimpleCocoaWindow)window;
                return IsExtensionPresent("VK_EXT_metal_surface")
                    ? CreateWindowSurfaceMetal(vulkanInstance, cocoaWindow)
                    : CreateWindowSurfaceMVK(vulkanInstance, cocoaWindow);
            }

            throw new NotImplementedException();
        }
    }
}
