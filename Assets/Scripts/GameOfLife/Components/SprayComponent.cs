using Unity.Entities;
using Unity.Mathematics;

namespace alexnown.GameOfLife
{
    public struct SprayComponent : IComponentData
    {
        public uint Seed;
        public float2 Position;
        public int Radius;
        public float Intensity;
    }
}
