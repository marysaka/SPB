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
    // NSWindow pointer wrapper
    public override NativeHandle WindowHandle { get; }

    // CAMetalLayer pointer wrapper
    public NativeHandle MetalLayerHandle { get; private set; }

    public override NativeHandle DisplayHandle => throw new NotImplementedException();

    /// <summary>
    /// Constructs instance of this class.
    /// </summary>
    /// <param name="windowHandle">NSWindow native pointer</param>
    /// <param name="initMetalLayer">Flag indicating need to acquire Metal layer</param>
    public SimpleCocoaWindow(NativeHandle windowHandle, bool initMetalLayer = true)
    {
        WindowHandle = windowHandle;

        // Sanity check
        ObjectiveC.EnsureIsKindOfClass(windowHandle.RawHandle, "NSWindow");

        if (initMetalLayer)
        {
            InitMetalLayer();
        }
    }

    private void InitMetalLayer()
    {
        // assuming that handles is NSWindow
        // 1. get top-level NSView
        // 2. get its layer
        // 3. if it is CAMetalLayer, we have what we need
        // 4. make it CAMetalLayer

        // 1. get top-level NSView
        var contentView = ObjectiveC.GetIntPtrValue(WindowHandle.RawHandle, "contentView");

        ObjectiveC.EnsureIsKindOfClass(contentView, "NSView");

        // 2. get its layer
        var layer = ObjectiveC.GetIntPtrValue(contentView, "layer");

        // 3. if it is CAMetalLayer, we have what we need
        if (ObjectiveC.IsKindOfClass(contentView, "CAMetalLayer"))
        {
            MetalLayerHandle = new NativeHandle(layer);
            return;
        }

        // 4. make it CAMetalLayer
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
        // Do nothing as no actual resource is owned
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
