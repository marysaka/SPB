using SPB.Platform.Exceptions;
using SPB.Platform.Win32;
using SPB.Platform.X11;
using SPB.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SPB.Graphics.Vulkan
{
    public static class VulkanHelper
    {
        private static bool _isInit = false;

        private static IntPtr _vulkanHandle;

        private const string VulkanLibraryNameWindows = "vulkan-1.dll";
        private const string VulkanLibraryNameLinux = "libvulkan.so.1";
        private const string VulkanLibraryNameMacOS = "libvulkan.dylib";

        private static string[] _extensions;

        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)]
        private delegate IntPtr vkGetInstanceProcAddrDelegate(IntPtr instance, string name);

        private static vkGetInstanceProcAddrDelegate _vkGetInstanceProcAddr;

        private unsafe struct VkExtensionProperty
        {
            public fixed byte ExtensionName[0x100];
            public uint SpecVersion;
        }

        private const uint VK_STRUCTURE_TYPE_XLIB_SURFACE_CREATE_INFO_KHR = 1000004000;
        private const uint VK_STRUCTURE_TYPE_XCB_SURFACE_CREATE_INFO_KHR = 1000005000;
        private const uint VK_STRUCTURE_TYPE_WIN32_SURFACE_CREATE_INFO_KHR = 1000009000;

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

        private unsafe delegate int vkEnumerateInstanceExtensionPropertiesDelegate(string layerName, out uint layerCount, VkExtensionProperty* properties);
        private unsafe delegate int vkCreateWin32SurfaceKHRDelegate(IntPtr instance, ref VkWin32SurfaceCreateInfoKHR createInfo, IntPtr allocator, out IntPtr surface);
        private unsafe delegate int vkCreateXlibSurfaceKHRDelegate(IntPtr instance, ref VkXlibSurfaceCreateInfoKHR createInfo, IntPtr allocator, out IntPtr surface);
        private unsafe delegate int vkCreateXcbSurfaceKHRDelegate(IntPtr instance, ref VkXcbSurfaceCreateInfoKHR createInfo, IntPtr allocator, out IntPtr surface);

        private static vkEnumerateInstanceExtensionPropertiesDelegate _vkEnumerateInstanceExtensionProperties;

        private static string GetLibraryName()
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

        private static bool TryLoadLibrary(out IntPtr vulkanHandle)
        {
            string libraryName = GetLibraryName();

            if (libraryName == null)
            {
                vulkanHandle = IntPtr.Zero;

                return false;
            }

            return NativeLibrary.TryLoad(libraryName, out vulkanHandle);
        }

        private static bool IsExtensionPresent(string extensionName)
        {
            return _extensions.Contains(extensionName);
        }

        private static unsafe string GetStringFromUtf8Byte(byte *start)
        {
            byte* end = start;
            while (*end != 0) end++;

            return Encoding.UTF8.GetString(start, (int)(end - start));
        }

        private static void EnsureInit()
        {
            if (!_isInit)
            {
                if (!TryLoadLibrary(out _vulkanHandle) || !NativeLibrary.TryGetExport(_vulkanHandle, "vkGetInstanceProcAddr", out IntPtr vkGetInstanceProcAddrPtr))
                {
                    throw new NotSupportedException("Unsupported platform for Vulkan!");
                }


                _vkGetInstanceProcAddr = Marshal.GetDelegateForFunctionPointer<vkGetInstanceProcAddrDelegate>(vkGetInstanceProcAddrPtr);
                _vkEnumerateInstanceExtensionProperties = Marshal.GetDelegateForFunctionPointer<vkEnumerateInstanceExtensionPropertiesDelegate>(_vkGetInstanceProcAddr(IntPtr.Zero, "vkEnumerateInstanceExtensionProperties"));

                unsafe
                {
                    int res = _vkEnumerateInstanceExtensionProperties(null, out uint layerCount, null);

                    if (res != 0)
                    {
                        throw new PlatformException($"vkEnumerateInstanceExtensionProperties failed: {res}");
                    }

                    VkExtensionProperty[] extensions = new VkExtensionProperty[layerCount];

                    fixed (VkExtensionProperty *extensionsPtr = extensions)
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
                    if (!IsExtensionPresent("VK_KHR_xcb_surface"))
                    {
                        if (!IsExtensionPresent("VK_KHR_xlib_surface"))
                        {
                            isVulkanUsable = false;
                        }
                    }
                }
                else if (OperatingSystem.IsMacOS())
                {
                    isVulkanUsable &= IsExtensionPresent("VK_MVK_macos_surface");
                }

                if (!isVulkanUsable)
                {
                    throw new NotSupportedException("No supported Vulkan surface found!");
                }

                _isInit = true;
            }
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
                if (IsExtensionPresent("VK_KHR_xcb_surface"))
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
                extensions.Add("VK_MVK_macos_surface");
            }

            return extensions.ToArray();
        }

        public static IntPtr CreateWindowSurface(IntPtr vulkanInstance, NativeWindowBase window)
        {
            EnsureInit();

            if (OperatingSystem.IsWindows() && IsExtensionPresent("VK_KHR_win32_surface"))
            {
                vkCreateWin32SurfaceKHRDelegate vkCreateWin32SurfaceKHR = Marshal.GetDelegateForFunctionPointer<vkCreateWin32SurfaceKHRDelegate>(_vkGetInstanceProcAddr(vulkanInstance, "vkCreateWin32SurfaceKHR"));

                VkWin32SurfaceCreateInfoKHR creationInfo = new VkWin32SurfaceCreateInfoKHR
                {
                    StructType = VK_STRUCTURE_TYPE_WIN32_SURFACE_CREATE_INFO_KHR,
                    Next = IntPtr.Zero,
                    Flags = 0,
// Broken warning here there is no platform issues here...
#pragma warning disable CA1416
                    HInstance = Win32.GetWindowLong(window.WindowHandle.RawHandle, Win32.GetWindowLongIndex.GWL_HINSTANCE),
#pragma warning restore CA1416
                    Hwnd = window.WindowHandle.RawHandle
                };

                int res = vkCreateWin32SurfaceKHR(vulkanInstance, ref creationInfo, IntPtr.Zero, out IntPtr surface);

                if (res != 0)
                {
                    throw new PlatformException($"vkCreateWin32SurfaceKHR failed: {res}");
                }

                return surface;
            }

            if (OperatingSystem.IsLinux())
            {
                if (IsExtensionPresent("VK_KHR_xcb_surface"))
                {
                    vkCreateXcbSurfaceKHRDelegate vkCreateXcbSurfaceKHR = Marshal.GetDelegateForFunctionPointer<vkCreateXcbSurfaceKHRDelegate>(_vkGetInstanceProcAddr(vulkanInstance, "vkCreateXcbSurfaceKHR"));

                    VkXcbSurfaceCreateInfoKHR creationInfo = new VkXcbSurfaceCreateInfoKHR
                    {
                        StructType = VK_STRUCTURE_TYPE_XCB_SURFACE_CREATE_INFO_KHR,
                        Next = IntPtr.Zero,
                        Flags = 0,
                        Connection = X11.GetXCBConnection(window.DisplayHandle.RawHandle),
                        Window = window.WindowHandle.RawHandle
                    };

                    int res = vkCreateXcbSurfaceKHR(vulkanInstance, ref creationInfo, IntPtr.Zero, out IntPtr surface);

                    if (res != 0)
                    {
                        throw new PlatformException($"vkCreateXcbSurfaceKHR failed: {res}");
                    }

                    return surface;
                }
                else
                {
                    vkCreateXlibSurfaceKHRDelegate vkCreateXlibSurfaceKHR = Marshal.GetDelegateForFunctionPointer<vkCreateXlibSurfaceKHRDelegate>(_vkGetInstanceProcAddr(vulkanInstance, "vkCreateXlibSurfaceKHR"));

                    VkXlibSurfaceCreateInfoKHR creationInfo = new VkXlibSurfaceCreateInfoKHR
                    {
                        StructType = VK_STRUCTURE_TYPE_XLIB_SURFACE_CREATE_INFO_KHR,
                        Next = IntPtr.Zero,
                        Flags = 0,
                        Display = window.DisplayHandle.RawHandle,
                        Window = window.WindowHandle.RawHandle
                    };

                    int res = vkCreateXlibSurfaceKHR(vulkanInstance, ref creationInfo, IntPtr.Zero, out IntPtr surface);

                    if (res != 0)
                    {
                        throw new PlatformException($"vkCreateXlibSurfaceKHR failed: {res}");
                    }

                    return surface;
                }
            }

            throw new NotImplementedException();
        }
    }
}
