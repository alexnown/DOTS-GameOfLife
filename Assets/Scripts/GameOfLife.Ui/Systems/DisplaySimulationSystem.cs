using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace GameOfLife
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class DisplaySimulationSystem : SystemBase
    {
        private readonly int MainTextureId = Shader.PropertyToID("_MainTex");
        private readonly int WidthId = Shader.PropertyToID("_Width");
        private readonly int HeightId = Shader.PropertyToID("_Height");
        protected override void OnUpdate()
        {
            Entities.ForEach((RawImage img, ref DisplaySimulation simulation) =>
            {
                if (simulation.Target == Entity.Null)
                {
                    Texture2D gofTexture = default;
                    Entity targetSimulation = Entity.Null;
                    Entities.ForEach((Entity e, GameOfLifeTexture texture) =>
                    {
                        gofTexture = texture.Value;
                        targetSimulation = e;
                    }).WithoutBurst().Run();
                    if (targetSimulation == Entity.Null) return;
                    simulation.Target = targetSimulation;
                    var mat = img.material;
                    img.texture = gofTexture;
                    mat.SetTexture(MainTextureId, gofTexture);
                    mat.SetFloat(WidthId, gofTexture.width);
                    mat.SetFloat(HeightId, gofTexture.height);
                }
                else (img.texture as Texture2D).Apply();
            }).WithoutBurst().Run();
        }
    }
}
