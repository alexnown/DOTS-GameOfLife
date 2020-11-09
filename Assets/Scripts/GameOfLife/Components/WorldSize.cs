using Unity.Entities;
using Unity.Mathematics;

namespace GameOfLife
{
    [GenerateAuthoringComponent]
    public struct WorldSize : IComponentData
    {
        public int2 Size;
    }
}
