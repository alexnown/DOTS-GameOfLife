using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace GameOfLife
{
    public struct WorldCellsComponent : IComponentData
    {
        public int2 Size;
        public BlobAssetReference<WorldCellsData> Value;

        public NativeArray<byte> GetActiveCells() => GetCellsArray(Value.Value.ArrayIndex);

        public NativeArray<byte> GetCellsArray(byte arrayIndex)
            => arrayIndex == 0 ? Value.Value.Array0.Value : Value.Value.Array1.Value;
    }

    public struct WorldCellsData
    {
        public byte ArrayIndex;
        public BlobPtr<NativeArray<byte>> Array0;
        public BlobPtr<NativeArray<byte>> Array1;
    }


}
