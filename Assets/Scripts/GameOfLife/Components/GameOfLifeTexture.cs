using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GameOfLife
{
    [GenerateAuthoringComponent]
    public class GameOfLifeTexture : IComponentData
    {
        public Texture2D Value;
    }
}
