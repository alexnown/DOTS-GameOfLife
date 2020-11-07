using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace GameOfLife
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class DrawCellAreasToTexture : SystemBase
    {
        [BurstCompile]
        private struct UpdateTexture : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            public DynamicBuffer<DrawCellsOnGuiColor> Colors;
            [ReadOnly]
            public NativeArray<byte> CellStates;
            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<byte> TargetTextureArray;

            public void Execute(int index)
            {
                byte cellState = CellStates[index];
                var colorData = Colors[cellState].Color;
                TargetTextureArray[3 * index] = colorData.r;
                TargetTextureArray[3 * index + 1] = colorData.g;
                TargetTextureArray[3 * index + 2] = colorData.b;
            }
        }

        private readonly Stopwatch _timer = new Stopwatch();

        protected override void OnUpdate()
        {
            Entities.ForEach((DrawTextureOnGui drawer) =>
            {
                var texture = drawer.Texture;
                Entities.ForEach((ref WorldCellsComponent cells, in DynamicBuffer<DrawCellsOnGuiColor> colors) =>
                {
                    if (texture == null)
                    {
                        texture = new Texture2D(cells.Size.x, cells.Size.y, TextureFormat.RGB24, false);
                        drawer.Texture = texture;
                        drawer.enabled = true;
                    }
                    else if (texture.width != cells.Size.x || texture.height != cells.Size.y)
                    {
                        texture.Resize(cells.Size.x, cells.Size.y);
                    }
                    var job = new UpdateTexture
                    {
                        TargetTextureArray = texture.GetRawTextureData<byte>(),
                        CellStates = cells.GetActiveCells(),
                        Colors = colors
                    }.Schedule(cells.Size.x * cells.Size.y, 1024);
                    job.Complete();
                    texture.Apply(false);
                }).WithoutBurst().Run();
            }).WithoutBurst().Run();
        }
    }
}
