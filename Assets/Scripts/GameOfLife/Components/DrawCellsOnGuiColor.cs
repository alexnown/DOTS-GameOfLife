using Unity.Entities;
using UnityEngine;

namespace GameOfLife
{
    [GenerateAuthoringComponent]
    public struct DrawCellsOnGuiColor : IBufferElementData
    {
        public Color32 Color;
    }
}
