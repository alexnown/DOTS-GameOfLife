using Unity.Entities;
using UnityEngine;

namespace GameOfLife
{
    [GenerateAuthoringComponent]
    public class GameOfLifeTexture : ISystemStateComponentData
    {
        [HideInInspector]
        public bool IsCreated;
        public Texture2D Value;
    }
}
