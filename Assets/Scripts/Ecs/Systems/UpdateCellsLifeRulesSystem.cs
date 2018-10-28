using System;
using alexnown.Ecs.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace alexnown.EcsLife
{
    [DisableAutoCreation]
    [UpdateBefore(typeof(EndCellsUpdatesBarrier))]
    public class UpdateCellsLifeRulesSystem : JobComponentSystem
    {
        private ComponentGroup _activeCellsDb;

        protected override void OnCreateManager()
        {
            _activeCellsDb = GetComponentGroup(ComponentType.Create<CellsDbState>(), ComponentType.Create<CellsDb>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (_activeCellsDb.CalculateLength() != 1) throw new InvalidOperationException($"Can't contains {_activeCellsDb.CalculateLength()} active cells db!");
            var cellsDb = _activeCellsDb.GetSharedComponentDataArray<CellsDb>()[0];
            var cellsDbState = _activeCellsDb.GetComponentDataArray<CellsDbState>()[0];
            var activeCells = cellsDb.GetActiveCells(cellsDbState);
            var futureCells = cellsDb.GetFutureCells(cellsDbState);
            int length = activeCells.Length;
            var job = new UpdateCellsState
            {
                Width = cellsDb.Width,
                ActiveCells = activeCells,
                FutureCellsState = futureCells
            }.Schedule(length, 2048, inputDeps);
            job.Complete();
            return job;
        }

        #region Job

        

        [BurstCompile]
        struct UpdateCellsState : IJobParallelFor
        {

            [ReadOnly]
            public int Width;
            [ReadOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<CellState> ActiveCells;
            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<CellState> FutureCellsState;
            
            public void Execute(int index)
            {
                int posX = index % Width;
                int posY = index / Width;
                var neighbors = Neighbors.Calculate(posX, posY, Width, ActiveCells.Length);

                int aliveNeighbors = 0;
                aliveNeighbors += ActiveCells[neighbors.LeftUp].State;
                aliveNeighbors += ActiveCells[neighbors.Up].State;
                aliveNeighbors += ActiveCells[neighbors.RightUp].State;
                aliveNeighbors += ActiveCells[neighbors.Left].State;
                aliveNeighbors += ActiveCells[neighbors.Right].State;
                aliveNeighbors += ActiveCells[neighbors.LeftDown].State;
                aliveNeighbors += ActiveCells[neighbors.Down].State;
                aliveNeighbors += ActiveCells[neighbors.RightDown].State;

                var state = new CellState();
                bool isAlive = aliveNeighbors == 3 || (aliveNeighbors == 2 && ActiveCells[index].State == 1);
                if (isAlive) state.State = 1;
                FutureCellsState[index] = state;
            }
        }

        #endregion

    }
}
