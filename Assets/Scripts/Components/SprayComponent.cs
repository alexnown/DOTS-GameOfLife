using Unity.Entities;

namespace alexnown.GameOfLife
{
    public struct SprayComponent : IComponentData
    {
        public uint Seed;
        public int Radius;
        public float Intensity;
    }
}
