using System;
using System.Reflection;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Kopernicus;

/// <summary>
/// Wraps existing memory in a <see cref="NativeArray{T}"/> that can also be used in the Unity editor.
/// </summary>
internal static unsafe class NativeArraySafety
{
    // Null in a player. The shared handle is the one Unity gives to memory it doesn't own, and it
    // is never invalidated, so it doesn't need to be released.
    private static readonly object SharedHandle;
    private static readonly MethodInfo SetHandle;

    static NativeArraySafety()
    {
        Type handleType = typeof(NativeArrayUnsafeUtility).Assembly.GetType(
            "Unity.Collections.LowLevel.Unsafe.AtomicSafetyHandle");
        MethodInfo getHandle = handleType?.GetMethod("GetTempUnsafePtrSliceHandle");
        SetHandle = typeof(NativeArrayUnsafeUtility).GetMethod("SetAtomicSafetyHandle");

        if (getHandle != null && SetHandle != null)
        {
            SharedHandle = getHandle.Invoke(null, Array.Empty<object>());
        }
    }

    public static NativeArray<T> Wrap<T>(void* data, int length, Allocator allocator)
        where T : unmanaged
    {
        NativeArray<T> array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(data, length, allocator);
        if (SharedHandle == null)
        {
            return array;
        }

        object[] args = { array, SharedHandle };
        Setter<T>.Method.Invoke(null, args);
        return (NativeArray<T>)args[0];
    }

    private static class Setter<T>
    {
        public static readonly MethodInfo Method = SetHandle?.MakeGenericMethod(typeof(T));
    }
}
