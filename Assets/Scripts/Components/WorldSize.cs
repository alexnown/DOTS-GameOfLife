using Unity.Entities;

namespace alexnown.GameOfLife
{
    public struct WorldSize : IComponentData
    {
        public int Width;
        public int Height;
    }
}
