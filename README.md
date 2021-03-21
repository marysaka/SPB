# SPB (Simple Platform Bindings)

This is an API providing simple ways to handle native window and graphic context on various platform.

Graphic context support:
- OpenGL

Platform support:

- X11 via GLX (v1.3 and upper, backward compatibility to do)
- WGL (with WGL_ARB_create_context, WGL_ARB_create_context_profile, WGL_ARB_pixel_format and WGL_EXT_swap_control)

TODO:
- EGL
- Cocoa (& Carbone?)
- Vulkan context
- Metal
- DirectX
