using System;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

namespace SPB.Platform.Metal
{
    [SupportedOSPlatform("macos")]
    internal sealed class MacOS
    {
        public struct Selector
        {
            public readonly IntPtr NativePtr;

            public unsafe Selector(string value)
            {
                int size = System.Text.Encoding.UTF8.GetMaxByteCount(value.Length);
                byte* data = stackalloc byte[size];

                fixed (char* pValue = value)
                {
                    System.Text.Encoding.UTF8.GetBytes(pValue, value.Length, data, size);
                }

                NativePtr = sel_registerName(data);
            }

            public static implicit operator Selector(string value) => new Selector(value);
        }

        private const string ObjectiveCRuntimeLibrary = "/usr/lib/libobjc.A.dylib";

        [DllImport(ObjectiveCRuntimeLibrary)]
        public static extern void objc_msgSend(IntPtr receiver, Selector selector, byte value);

        [DllImport(ObjectiveCRuntimeLibrary)]
        private static unsafe extern IntPtr sel_registerName(byte* data);
    }
}