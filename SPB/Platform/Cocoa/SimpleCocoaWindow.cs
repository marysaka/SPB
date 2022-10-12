using System;
using System.Runtime.Versioning;
using SPB.Platform.Exceptions;
using SPB.Windowing;

namespace SPB.Platform.Cocoa;

/// <summary>
/// Provides implementation for bindings to NSWindow class on macOS.
/// </summary>
[SupportedOSPlatform("macos")]
public class SimpleCocoaWindow : NativeWindowBase
{
    // NSWindow pointer wrapper.
    public override NativeHandle WindowHandle { get; }

    // CAMetalLayer pointer wrapper.
    public NativeHandle MetalLayerHandle { get; private set; }

    public override NativeHandle DisplayHandle => throw new NotImplementedException();

    /// <summary>
    /// Constructs instance of this class.
    ///
    /// If NSWindow pointer is given its top-level content view is used to setup
    /// Metal layer.
    /// If NSView pointer is given, window it belongs is extracted from NSView.window
    /// property, Metal layer is set up for the given NSView object.
    /// </summary>
    /// <param name="handle">Native pointer</param>
    /// <param name="initMetalLayer">Flag indicating need to acquire Metal layer</param>
    public SimpleCocoaWindow(NativeHandle handle, bool initMetalLayer = true)
    {
        WindowHandle = GetNsWindow(handle);

        if (initMetalLayer)
        {
            var view = GetNsView(handle);
            InitMetalLayer(view.RawHandle);
        }
    }

    /// <summary>
    /// This method returns content view for a given object.
    /// If object is of NSWindow type then its top level content view is returned.
    /// If object is of NSView type then it is returned as is.
    /// </summary>
    /// <param name="handle">UI object pointer to retrieve view from</param>
    /// <returns>NSView pointer wrapped into NativeHandle</returns>
    private NativeHandle GetNsView(NativeHandle handle)
    {
        if (ObjectiveC.IsKindOfClass(handle.RawHandle, "NSView"))
        {
            return handle;
        }

        var contentView = ObjectiveC.GetIntPtrValue(WindowHandle.RawHandle, "contentView");

        ObjectiveC.EnsureIsKindOfClass(contentView, "NSView");
        return new NativeHandle(contentView);
    }

    /// <summary>
    /// This method returns window for a given object.
    /// If object is of NSWindow type then it is returned as is.
    /// If object is of NSView type then its view window object or null pointer,
    /// if view is not installed into window.
    /// </summary>
    /// <param name="handle">UI object pointer to retrieve window from</param>
    /// <returns>NSWindow pointer wrapped into NativeHandle</returns>
    private NativeHandle GetNsWindow(NativeHandle handle)
    {
        if (ObjectiveC.IsKindOfClass(handle.RawHandle, "NSView"))
        {
            IntPtr window = ObjectiveC.GetIntPtrValue(handle.RawHandle, "window");
            return new NativeHandle(window);
        }

        ObjectiveC.EnsureIsKindOfClass(handle.RawHandle, "NSWindow");
        return handle;
    }

    /// <summary>
    /// This method initialises Metal layer of view object if needed.
    /// </summary>
    /// <param name="contentView">NSView object to initialise Metal layer</param>
    private void InitMetalLayer(IntPtr contentView)
    {
        ObjectiveC.EnsureIsKindOfClass(contentView, "NSView");

        // 1. Get view layer.
        var layer = ObjectiveC.GetIntPtrValue(contentView, "layer");

        // 2. If it is CAMetalLayer, we have what we need.
        if (ObjectiveC.IsKindOfClass(contentView, "CAMetalLayer"))
        {
            MetalLayerHandle = new NativeHandle(layer);
            return;
        }

        // 3. Make it CAMetalLayer.
        // https://developer.apple.com/documentation/quartzcore/cametallayer?language=objc
        Console.Out.WriteLine("Replacing original NSView layer with CAMetalLayer");

        var layerClass = ObjectiveC.GetClass("CAMetalLayer");
        var metalLayer = ObjectiveC.GetIntPtrValue(layerClass, "layer");
        MetalLayerHandle = new NativeHandle(metalLayer);

        ObjectiveC.SetBoolValue(contentView, "wantsLayer", true);
        ObjectiveC.SetIntPtrValue(contentView, "setLayer:", metalLayer);
    }

    protected override void Dispose(bool disposing)
    {
        // Do nothing as no actual resource is owned.
    }

    public override void Show()
    {
        if (WindowHandle.RawHandle == IntPtr.Zero)
        {
            throw new PlatformException("Attempted to show not initialised window");
        }

        // [self makeKeyAndOrderFront:self];
        ObjectiveC.SetIntPtrValue(
            WindowHandle.RawHandle,
            "makeKeyAndOrderFront:",
            WindowHandle.RawHandle
        );
    }

    public override void Hide()
    {
        if (WindowHandle.RawHandle == IntPtr.Zero)
        {
            throw new PlatformException("Attempted to hide not initialised window");
        }

        // [self orderOut:self];
        ObjectiveC.SetIntPtrValue(
            WindowHandle.RawHandle,
            "orderOut:",
            WindowHandle.RawHandle
        );
    }
}
