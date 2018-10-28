using System;
using alexnown.Ecs.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine.AI;

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

            JobHandle jobHandle = default(JobHandle);
            if (cellsDb.UpdateRules == WorldRules.Default)
            {
                jobHandle = new SimpleLifeRules
                {
                    Width = cellsDb.Width,
                    ActiveCells = activeCells,
                    FutureCellsState = futureCells
                }.Schedule(length, 2048, inputDeps);
            }
            else if (cellsDb.UpdateRules == WorldRules.Steppers)
            {
                jobHandle = new SteppersRules
                {
                    Width = cellsDb.Width,
                    ActiveCells = activeCells,
                    FutureCellsState = futureCells
                }.Schedule(length, 2048, inputDeps);
            }

            jobHandle.Complete();
            return jobHandle;
        }

        #region Job



        [BurstCompile]
        struct SimpleLifeRules : IJobParallelFor
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

        [BurstCompile]
        struct SteppersRules : IJobParallelFor
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
                var cellState = ActiveCells[index];
                if (cellState.State == 1)
                {
                    FutureCellsState[index] = new CellState { State = 2 };
                    return;
                }
                int posX = index % Width;
                int posY = index / Width;

                var neighbors = Neighbors.Calculate(posX, posY, Width, ActiveCells.Length);
                int old = 0;
                int young = 0;
                SumNeighbors(neighbors.LeftUp, ref old, ref young);
                SumNeighbors(neighbors.Up, ref old, ref young);
                SumNeighbors(neighbors.RightUp, ref old, ref young);
                SumNeighbors(neighbors.Left, ref old, ref young);
                SumNeighbors(neighbors.Right, ref old, ref young);
                SumNeighbors(neighbors.LeftDown, ref old, ref young);
                SumNeighbors(neighbors.Down, ref old, ref young);
                SumNeighbors(neighbors.RightDown, ref old, ref young);
                int totalSum = old + young;

                var state = new CellState();
                if (cellState.State == 0 && totalSum == 3 && old > 1)
                {
                    state.State = 1;
                }
                else if (cellState.State == 2 && totalSum > 1 && totalSum < 4 && young < 2)
                {
                    state.State = 2;
                }
                FutureCellsState[index] = state;
            }

            private void SumNeighbors(int index, ref int old, ref int young)
            {
                var state = ActiveCells[index].State;
                if (state == 1) young++;
                else if (state == 2) old++;
            }
        }


        #endregion

    }
}
