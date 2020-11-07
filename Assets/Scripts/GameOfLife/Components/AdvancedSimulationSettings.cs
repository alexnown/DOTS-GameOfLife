using Unity.Entities;

namespace GameOfLife
{
    [GenerateAuthoringComponent]
    public struct AdvancedSimulationSettings : IComponentData
    {
        public int MaxCyclesPerFrame;
        public int LimitationMs;
    }
}
