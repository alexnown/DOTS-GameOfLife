using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace alexnown.GameOfLife
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class UpdateRendererTextureSystem : ComponentSystem
    {
        [BurstCompile]
        private struct UpdateTexture : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            public DynamicBuffer<CellColorElement> Colors;
            public BlobAssetReference<WorldCellsData> CellsReference;
            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<byte> TargetTextureArray;
            public void Execute(int index)
            {
                int cellState = CellsReference.Value.ArrayIndex == 0 ? 
                    CellsReference.Value.Array0[index] : 
                    CellsReference.Value.Array1[index];
                cellState = math.clamp(cellState, 0, 2);
                if (cellState == 0)
                {
                    TargetTextureArray[3 * index] = 0;
                    TargetTextureArray[3 * index + 1] = 0;
                    TargetTextureArray[3 * index + 2] = 0;
                }
                else
                {
                    var colorData = Colors[cellState-1];
                    TargetTextureArray[3 * index] = colorData.R;
                    TargetTextureArray[3 * index + 1] = colorData.G;
                    TargetTextureArray[3 * index + 2] = colorData.B;
                }

            }
        }

        private EntityQuery _cellWorld;
        public Texture2D CreatedTexture { get; private set; }
        private NativeArray<byte> _textureArray;
        protected override void OnCreate()
        {
            base.OnCreate();
            _cellWorld = GetEntityQuery(
                ComponentType.ReadOnly<CellColorElement>(),
                ComponentType.ReadOnly<WorldCellsComponent>(),
                ComponentType.ReadOnly<WorldSize>());
            RequireForUpdate(_cellWorld);
            CreatedTexture = new Texture2D(1, 1, TextureFormat.RGB24, false);
        }

        protected override void OnUpdate()
        {
            Entities.With(_cellWorld).ForEach(
                (DynamicBuffer<CellColorElement> colors, ref WorldCellsComponent cells, ref WorldSize size) =>
                {
                    int elements = size.Width * size.Height;
                    var listForDrawing = PrepareTextureListForDrawing(size.Width, size.Height);
                    var job = new UpdateTexture
                    {
                        TargetTextureArray = listForDrawing,
                        CellsReference = cells.Value,
                        Colors = colors
                    }.Schedule(elements, (int)(elements / SystemInfo.processorCount) + 1);
                    job.Complete();
                    CreatedTexture.Apply(false);
                });
        }

        private NativeArray<byte> PrepareTextureListForDrawing(int width, int height)
        {
            if (!_textureArray.IsCreated || _textureArray.Length != 3 * width * height)
            {
                CreatedTexture.Resize(width, height);
                _textureArray = CreatedTexture.GetRawTextureData<byte>();
            }
            return _textureArray;
        }
    }
}
