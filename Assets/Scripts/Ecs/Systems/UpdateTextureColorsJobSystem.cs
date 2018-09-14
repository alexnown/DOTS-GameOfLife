using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

namespace alexnown.EcsLife
{
    [DisableAutoCreation]
    [UpdateAfter(typeof(ApplyFutureStatesSystem))]
    public class UpdateTextureColorsJobSystem : JobComponentSystem
    {
        public bool TexturePrepared { get; private set; } = true;
        public const int BATCHS_COUNT = 64;

        public NativeArray<byte> _textureColorsRGB;

        private ComponentGroup _activeCellsDb;

        public void FillTargetArray(NativeArray<byte> colors, int width, int height)
        {
            _textureColorsRGB = colors;
            TexturePrepared = false;
        }

        protected override void OnCreateManager(int capacity)
        {
            _activeCellsDb = GetComponentGroup(ComponentType.Create<CellsDbState>(), ComponentType.Create<CellsDb>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (!_textureColorsRGB.IsCreated || TexturePrepared) return inputDeps;
            if (_activeCellsDb.CalculateLength() != 1) throw new InvalidOperationException($"Can't contains {_activeCellsDb.CalculateLength()} active cells db!");
            var cellsDb = _activeCellsDb.GetSharedComponentDataArray<CellsDb>()[0];
            var cellsDbState = _activeCellsDb.GetComponentDataArray<CellsDbState>()[0];
            var activeCells = cellsDbState.ActiveCellsState == 0 ? cellsDb.CellsState0 : cellsDb.CellsState1;
            int length = activeCells.Length;

            var job = new UpdateTextureColors
            {
                ColorsArray = _textureColorsRGB,
                CellStates = activeCells
            }.ScheduleBatch(length, (length / BATCHS_COUNT + 1), inputDeps);
            job.Complete();
            TexturePrepared = true;
            return job;
        }

        [BurstCompile]
        private struct UpdateTextureColors : IJobParallelForBatch
        {
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<byte> ColorsArray;

            public NativeArray<CellState> CellStates;

            public void Execute(int startIndex, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    int realIndex = startIndex + i;
                    var cellState = CellStates[realIndex];
                    //ColorsArray[3*realIndex] = 0;//cellState.R;
                    ColorsArray[3 * realIndex + 1] = cellState.G;
                    //ColorsArray[3*realIndex + 2] = 0;//cellState.B;
                }
            }
        }
    }
}
