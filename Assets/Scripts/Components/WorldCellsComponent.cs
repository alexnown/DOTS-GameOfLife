using Unity.Collections;
using Unity.Entities;

namespace alexnown
{
    public struct WorldCellsComponent : IComponentData
    {
        public BlobAssetReference<WorldCellsData> Value;

        public NativeArray<byte> GetActiveCells() => GetCellsArray(Value.Value.ArrayIndex);

        public unsafe NativeArray<byte> GetCellsArray(byte arrayIndex)
        {
            if (arrayIndex == 0)
            {
                return NativeCollectionHelper.AsNativeArray(
                    Value.Value.Array0.GetUnsafePtr(),
                    Value.Value.Array0.Length);
            }
            else
            {
                return NativeCollectionHelper.AsNativeArray(
                    Value.Value.Array1.GetUnsafePtr(),
                    Value.Value.Array1.Length);
            }
        }
    }

    public struct WorldCellsData
    {
        public byte ArrayIndex;
        public BlobArray<byte> Array0;
        public BlobArray<byte> Array1;
    }
}
