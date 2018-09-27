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
        
        public NativeArray<Color32> CellColorsByState;

        [Inject] private EndFrameBarrier _barrier;

        private ComponentGroup _activeCellsDb;
        private ComponentGroup _drawRequests;
        
        protected override void OnCreateManager()
        {
            _activeCellsDb = GetComponentGroup(ComponentType.Create<CellsDbState>(), ComponentType.Create<CellsDb>());
            _drawRequests = GetComponentGroup(ComponentType.Create<DrawStateRequest>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (_drawRequests.CalculateLength() == 0) return inputDeps;
            if (_activeCellsDb.CalculateLength() != 1) throw new InvalidOperationException($"Can't contains {_activeCellsDb.CalculateLength()} active cells db!");
            var cellsDb = _activeCellsDb.GetSharedComponentDataArray<CellsDb>()[0];
            var cellsDbState = _activeCellsDb.GetComponentDataArray<CellsDbState>()[0];
            var activeCells = cellsDb.GetActiveCells(cellsDbState);

            var requests = _drawRequests.GetSharedComponentDataArray<DrawStateRequest>();
            var requestEntities = _drawRequests.GetEntityArray();
            for (int i = 0; i < requests.Length; i++)
            {
                //_barrier.PostUpdateCommands.DestroyEntity(requestEntities[i]);
                return  new SetColorsNoResolutionMultiplier
                {
                    TextureColors = requests[i].Colors,
                    CellStates = activeCells,
                    CellColorsByState = CellColorsByState
                }.Schedule(activeCells.Length, 1024*8, inputDeps);
            }
            
            return inputDeps;
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
    }
}
