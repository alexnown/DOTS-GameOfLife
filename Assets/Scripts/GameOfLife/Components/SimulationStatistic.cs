using Unity.Entities;

namespace GameOfLife
{
    [GenerateAuthoringComponent]
    public struct SimulationStatistic : IComponentData
    {
        public int Age;
        public float TotalMs;
    }
}
