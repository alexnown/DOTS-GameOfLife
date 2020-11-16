using Unity.Entities;

namespace GameOfLife
{
    [GenerateAuthoringComponent]
    public struct SimulationStatisticView : IComponentData
    {
        public float PrevTime;
        public int Age;
    }
}
