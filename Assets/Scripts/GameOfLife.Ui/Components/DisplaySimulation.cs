using Unity.Entities;

namespace GameOfLife
{
    [GenerateAuthoringComponent]
    public struct DisplaySimulation : IComponentData
    {
        public Entity Target;
    }
}
