using Unity.Entities;
using UnityEngine;

namespace alexnown.GameOfLife
{
    [GenerateAuthoringComponent]
    public struct DrawCellsOnGuiColor : IBufferElementData
    {
        public Color32 Color;
    }
}
