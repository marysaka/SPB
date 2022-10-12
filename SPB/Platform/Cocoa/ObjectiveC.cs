using System;
using System.Runtime.InteropServices;

namespace SPB.Platform.Cocoa;

/// <summary>
/// This class provides basic interaction with ObjectiveC runtime.
/// See: https://developer.apple.com/documentation/objectivec/objective-c_runtime
/// </summary>
internal class ObjectiveC
{
    // In NET6+ this should be easier with updated interop services
    private const string LibraryName = "libobjc.dylib";

    [DllImport(LibraryName, CharSet = CharSet.Ansi)]
    private static extern IntPtr objc_getClass(string className);

    [DllImport(LibraryName, CharSet = CharSet.Ansi)]
    private static extern IntPtr sel_getUid(string selector);

    [DllImport(LibraryName, CharSet = CharSet.Ansi)]
    private static extern string object_getClass(IntPtr obj);

    [DllImport(LibraryName)]
    private static extern IntPtr objc_msgSend(IntPtr self, IntPtr selector);

    [DllImport(LibraryName, EntryPoint = "objc_msgSend")]
    private static extern bool bool_msgSend_IntPtr(IntPtr self, IntPtr selector, IntPtr arg);

    [DllImport(LibraryName, EntryPoint = "objc_msgSend")]
    private static extern void void_msgSend_IntPtr(IntPtr self, IntPtr selector, IntPtr arg);

    [DllImport(LibraryName, EntryPoint = "objc_msgSend")]
    private static extern void void_msgSend_void(IntPtr self, IntPtr selector);

    [DllImport(LibraryName, EntryPoint = "objc_msgSend")]
    private static extern void void_msgSend_bool(IntPtr self, IntPtr selector, bool arg);

    /// <summary>
    /// This method checks if a given object is of expected class and throws
    /// ArgumentException if it is not.
    /// </summary>
    /// <param name="obj">Object to check class</param>
    /// <param name="className">Class name, e.g. 'NSWindow'</param>
    /// <exception cref="ArgumentException">Is thrown if obj is not an instance of class className</exception>
    internal static void EnsureIsKindOfClass(IntPtr obj, string className)
    {
        if (IsKindOfClass(obj, className))
        {
            return;
        }

        var objClassName = GetObjectClassName(obj);
        throw new ArgumentException(
            $"Object is of type '{objClassName}' instead of expected '{className}'"
        );
    }

    /// <summary>
    /// This method checks if a given object is of expected class specified by its string name.
    /// </summary>
    /// <param name="obj">Object to check class</param>
    /// <param name="className">Class name, e.g. 'NSWindow'</param>
    /// <returns>true if object type matches class, false otherwise</returns>
    internal static bool IsKindOfClass(IntPtr obj, string className)
    {
        if (obj == IntPtr.Zero)
        {
            return false;
        }

        var klass = GetClass(className, false);

        if (klass == IntPtr.Zero)
        {
            return false;
        }

        var selector = GetSelector("isKindOfClass:");
        return bool_msgSend_IntPtr(obj, selector, klass);
    }

    /// <summary>
    /// This method returns class name for an object.
    /// </summary>
    /// <param name="obj">Instance to get class name for</param>
    /// <returns>Name of class for a given object instance</returns>
    private static string GetObjectClassName(IntPtr obj)
    {
        return object_getClass(obj);
    }

    /// <summary>
    /// Returns type for class specified by its name.
    /// </summary>
    /// <param name="className">Class name, e.g. 'NSWindow'</param>
    /// <param name="throwOnUnknownName">Indicates need to raise exception if class name is not known</param>
    /// <returns>Class type as IntPtr reference</returns>
    /// <exception cref="ArgumentException">This exception is raised if class name is not known</exception>
    internal static IntPtr GetClass(string className, bool throwOnUnknownName = true)
    {
        var result = objc_getClass(className);

        if (throwOnUnknownName && result == IntPtr.Zero)
        {
            throw new ArgumentException($"No such class '{className}'");
        }

        return result;
    }

    /// <summary>
    /// This method returns selector by its name.
    /// </summary>
    /// <param name="selectorName">Name of selector, e.g. 'layer:'</param>
    /// <returns>Selector as IntPtr reference</returns>
    /// <exception cref="ArgumentException">This exception is raised if selector name is not known</exception>
    private static IntPtr GetSelector(string selectorName)
    {
        var selector = sel_getUid(selectorName);

        if (selector != IntPtr.Zero)
        {
            return selector;
        }

        throw new ArgumentException(
            $"Can not get selector '{selectorName}'"
        );
    }

    /// <summary>
    /// This method sends message that object responds with IntPtr.
    /// </summary>
    /// <param name="obj">Object to send message to</param>
    /// <param name="selectorName">Name of selector for the message</param>
    /// <returns>IntPtr value as responded by object</returns>
    internal static IntPtr GetIntPtrValue(IntPtr obj, string selectorName)
    {
        var selector = GetSelector(selectorName);
        return objc_msgSend(obj, selector);
    }

    /// <summary>
    /// This method sends message to set object property to IntPtr value.
    /// </summary>
    /// <param name="obj">Object to send message to</param>
    /// <param name="selectorName">Name of selector for the message</param>
    /// <param name="value">Value to set</param>
    internal static void SetIntPtrValue(IntPtr obj, string selectorName, IntPtr value)
    {
        var selector = GetSelector(selectorName);
        void_msgSend_IntPtr(obj, selector, value);
    }

    /// <summary>
    /// This method sends message to set object property to boolean value.
    /// </summary>
    /// <param name="obj">Object to send message to</param>
    /// <param name="selectorName">Name of selector for the message</param>
    /// <param name="value">Value to set</param>
    internal static void SetBoolValue(IntPtr obj, string selectorName, bool value)
    {
        var selector = GetSelector(selectorName);
        void_msgSend_bool(obj, selector, value);
    }

    /// <summary>
    /// This method sends message to invoke object method without any arguments
    /// or return value.
    /// </summary>
    /// <param name="obj">Object to send message to</param>
    /// <param name="selectorName">Name of selector for the message</param>
    internal static void InvokeVoid(IntPtr obj, string selectorName)
    {
        var selector = GetSelector(selectorName);
        void_msgSend_void(obj, selector);
    }
}
