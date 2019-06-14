using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
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
                if (CellsReference.Value.ArrayIndex == 0)
                    Process(ref CellsReference.Value.Array0, index);
                else Process(ref CellsReference.Value.Array1, index);
            }

            private void Process(ref BlobArray<byte> cellsArray, int index)
            {
                byte cellState = cellsArray[index];
                if (cellState == 0)
                {
                    TargetTextureArray[3 * index] = 0;
                    TargetTextureArray[3 * index + 1] = 0;
                    TargetTextureArray[3 * index + 2] = 0;
                }
                else
                {
                    var colorData = Colors[cellState - 1];
                    TargetTextureArray[3 * index] = colorData.R;
                    TargetTextureArray[3 * index + 1] = colorData.G;
                    TargetTextureArray[3 * index + 2] = colorData.B;
                }
            }
        }

        private EntityQuery _cellWorld;
        public Texture2D CreatedTexture { get; private set; }
        private NativeArray<byte> _textureArray;
        private readonly Stopwatch _timer = new Stopwatch();

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
                    _timer.Start();
                    int elements = size.Width * size.Height;
                    var listForDrawing = PrepareTextureListForDrawing(size.Width, size.Height);
                    var job = new UpdateTexture
                    {
                        TargetTextureArray = listForDrawing,
                        CellsReference = cells.Value,
                        Colors = colors
                    }.Schedule(elements, 1024);
                    job.Complete();
                    CreatedTexture.Apply(false);
                    _timer.Stop();
                    SimulationStatistics.UpdateTextureCounts++;
                    SimulationStatistics.UpdateTextureTotalTicks += _timer.ElapsedTicks;
                    _timer.Reset();
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
