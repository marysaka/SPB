namespace SPB.Windowing
{
    public abstract class SwapableNativeWindowBase : NativeWindowBase
    {
        public abstract uint SwapInterval { get; set; }

        public abstract void SwapBuffers();
    }
}
