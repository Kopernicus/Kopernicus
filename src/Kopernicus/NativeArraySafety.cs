using System;
using System.Reflection;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Kopernicus;

/// <summary>
/// Wraps existing memory in a <see cref="NativeArray{T}"/> that can also be used in the Unity editor.
/// </summary>
internal static unsafe class NativeArraySafety
{
    public static NativeArray<T> Wrap<T>(void* data, int length, Allocator allocator)
        where T : unmanaged
    {
        var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(
            data,
            length,
            allocator
        );
        AssignHandle(ref array);
        return array;
    }

    [BurstDiscard]
    static void AssignHandle<T>(ref NativeArray<T> array)
        where T : unmanaged
    {
        if (Reflected.SharedHandle is null)
            return;

        object[] args = [array, Reflected.SharedHandle];
        Setter<T>.Method.Invoke(null, args);
        array = (NativeArray<T>)args[0];
    }

    // These live in nested classes so that the static initializers stay out of reach of Burst.
    static class Reflected
    {
        public static readonly object SharedHandle;
        public static readonly MethodInfo SetHandle;

        static Reflected()
        {
            var assembly = typeof(NativeArrayUnsafeUtility).Assembly;
            var handleType = assembly.GetType(
                "Unity.Collections.LowLevel.Unsafe.AtomicSafetyHandle"
            );
            var getHandle = handleType?.GetMethod("GetTempUnsafePtrSliceHandle");
            SetHandle = typeof(NativeArrayUnsafeUtility).GetMethod("SetAtomicSafetyHandle");

            if (getHandle is not null && SetHandle is not null)
                SharedHandle = getHandle.Invoke(null, []);
        }
    }

    static class Setter<T>
    {
        public static readonly MethodInfo Method = Reflected.SetHandle?.MakeGenericMethod(
            typeof(T)
        );
    }
}
