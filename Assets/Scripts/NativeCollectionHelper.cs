using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public static class NativeCollectionHelper
{

    public unsafe static NativeArray<T> AsNativeArray<T>(void* pointer, int size) where T : struct 
    {
        var nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(pointer, size, Allocator.Invalid);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, AtomicSafetyHandle.Create());
#endif
        return nativeArray;
    }

    public unsafe static NativeArray<byte> AsNativeArray(void* pointer, int size)
    {
        var nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(pointer, size, Allocator.Invalid);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref nativeArray, AtomicSafetyHandle.Create());
#endif
        return nativeArray;
    }
}
