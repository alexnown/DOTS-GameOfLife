using Unity.Entities;
using Unity.Mathematics;

namespace alexnown.GameOfLife
{
    public struct ScreenViewPosition : IComponentData
    {
        public float2 Value;
    }
}
