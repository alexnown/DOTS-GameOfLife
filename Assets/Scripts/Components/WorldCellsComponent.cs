using Unity.Entities;

namespace alexnown
{
    public struct WorldCellsComponent : IComponentData
    {
        public BlobAssetReference<WorldCellsData> Value;
    }

    public struct WorldCellsData
    {
        public byte ArrayIndex;
        public BlobArray<byte> Array0;
        public BlobArray<byte> Array1;
    }
}
