using System.Runtime.InteropServices;

namespace RenderWrapper;

public static class Mem {
    /// <summary>
    /// Allocates a block of memory with a given type.
    /// </summary>
    /// <typeparam name="T">The type to allocate</typeparam>
    /// <param name="count">The number of instances to allocate</param>
    /// <returns>The allocated pointer</returns>
    public static unsafe T *Alloc<T>(int count = 1) where T : unmanaged
        => (T *)Marshal.AllocHGlobal(sizeof(T) * count);

    /// <summary>
    /// Frees a block of memory with a given type.
    /// </summary>
    /// <typeparam name="T">The type to free</typeparam>
    /// <param name="ptr">The pointer to free</param>
    public static unsafe void Free<T>(T *ptr) where T : unmanaged
        => Marshal.FreeHGlobal((nint)ptr);
}
