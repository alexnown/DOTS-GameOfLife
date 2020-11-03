using Unity.Entities;
using Unity.Mathematics;

namespace alexnown.GameOfLife
{
    [GenerateAuthoringComponent]
    public struct InitializeGameOfLifeWorld : IComponentData
    {
        public int2 Size;
        public bool SizeDependsScreenResolution;
        public float ScreenResolutionMultiplier;
    }
}
