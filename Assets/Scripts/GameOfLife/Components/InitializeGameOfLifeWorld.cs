using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GameOfLife
{
    [GenerateAuthoringComponent]
    public struct InitializeGameOfLifeWorld : IComponentData
    {
        public int2 Size;
    }
}
