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
    [UpdateAfter(typeof(ApplyFutureStatesSystem))]
    public class UpdateTextureColorsJobSystem : JobComponentSystem
    {
        public const int BUTCH_COUNT = 1024;
        public bool TexturePrepared { get; private set; } = true;
        public NativeArray<Color32> CellColorsByState; 

        private NativeArray<byte> _textureColorsRGB;
        private int _width;
        private int _height;

        private ComponentGroup _activeCellsDb;

        public void FillTargetArray(NativeArray<byte> colors, int width, int height)
        {
            _textureColorsRGB = colors;
            _width = width;
            _height = height;
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
            var activeCells = cellsDb.GetActiveCells(cellsDbState);
            int length = _width * _height;

            JobHandle job = inputDeps;
            if (length == activeCells.Length)
            {
                job = new SetColorsNoResolutionMultiplier
                {
                    TextureColors = _textureColorsRGB,
                    CellStates = activeCells,
                    CellColorsByState = CellColorsByState
                }.Schedule(length, BUTCH_COUNT, inputDeps);
            }
            else
            {
                throw new NotSupportedException();
            }

            job.Complete();
            TexturePrepared = true;
            return job;
        }
        
        [BurstCompile]
        private struct SetColorsNoResolutionMultiplier : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<Color32> CellColorsByState;
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<byte> TextureColors;
            public NativeArray<CellState> CellStates;

            public void Execute(int index)
            {
                var cellState = CellStates[index];
                var color = CellColorsByState[cellState.State];
                TextureColors[3 * index] = color.r;
                TextureColors[3 * index + 1] = color.g;
                TextureColors[3 * index + 2] = color.b;
            }
        }
        /*
        private struct SetColorsWithResolutionMultiplier : IJobParallelFor
        {
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<byte> ColorsArray;

            public int ResolutionMultiplier;
            public NativeArray<CellState> CellStates;

            public void Execute(int index)
            {
                var cellState = CellStates[index];
                ColorsArray[3 * index + 1] = cellState.G;
            }
        } */
    }
}
