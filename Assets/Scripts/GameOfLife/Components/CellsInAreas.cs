using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace GameOfLife
{
    public struct CellsInAreas : IComponentData
    {
        public int2 Size;
        public BlobAssetReference<AreasData> Areas;
    }

    public struct AreasData
    {
        public BlobPtr<NativeArray<int>> ArrayPtr;
    }
}
