using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace alexnown.EcsLife
{
    [DisableAutoCreation]
    public class UpdateTextureColorsJobSystem : JobComponentSystem
    {
        public int Width;
        public NativeArray<Color32> CellColorsByState;
        public NativeArray<byte> TextureColors;
        private ComponentGroup _activeCellsDb;


        protected override void OnCreateManager()
        {
            Enabled = false;
            _activeCellsDb = GetComponentGroup(ComponentType.Create<CellsDbState>(), ComponentType.Create<CellsDb>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (_activeCellsDb.CalculateLength() < 1) throw new InvalidOperationException($"Can't contains {_activeCellsDb.CalculateLength()} active cells db!");
            var cellsDb = _activeCellsDb.GetSharedComponentDataArray<CellsDb>()[0];
            var cellsDbState = _activeCellsDb.GetComponentDataArray<CellsDbState>()[0];
            var activeCells = cellsDb.GetActiveCells(cellsDbState);
            return new SetColorsNoResolutionMultiplier
            {
                CellsState = activeCells,
                CellColorsByState = CellColorsByState,
                TextureColors = TextureColors
            }.Schedule(activeCells.Length, 1024, inputDeps);
        }



        [BurstCompile]
        private struct SetColorsNoResolutionMultiplier : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<Color32> CellColorsByState;
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<byte> TextureColors;
            [ReadOnly]
            public NativeArray<CellState> CellsState;

            public void Execute(int index)
            {
                var state = CellsState[index];
                var color = CellColorsByState[state.State];
                TextureColors[3 * index] = color.r;
                TextureColors[3 * index + 1] = color.g;
                TextureColors[3 * index + 2] = color.b;
            }
        }
    }
}
