using System;
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

        protected override void OnCreateManager(int capacity)
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
                Height = cellsDb.Height,
                ActiveCells = activeCells,
                FutureCellsState = futureCells
            }.Schedule(length, 2048, inputDeps);
            job.Complete();
            return job;
        }

        #region Job

        struct Neighborns
        {
            public int LeftUp;
            public int Up;
            public int RightUp;
            public int Right;
            public int Left;
            public int LeftDown;
            public int Down;
            public int RightDown;

            public override string ToString()
            {
                return $"[{LeftUp} {Up} {RightUp} , {Left} x {Right}, {LeftDown} {Down} {RightDown}]";
            }
        }

        [BurstCompile]
        struct UpdateCellsState : IJobParallelFor
        {

            [ReadOnly]
            public int Width;
            [ReadOnly]
            public int Height;
            [ReadOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<CellState> ActiveCells;
            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<CellState> FutureCellsState;

            private Neighborns CalculateNeighborsOptimized(int posX, int posY, int width, int height, int length)
            {
                int arrayIndex = posY * width + posX;
                int indexTop = arrayIndex + width;
                if (indexTop >= length) indexTop -= length;
                int indexDown = arrayIndex - width;
                if (indexDown < 0) indexDown += length;
                int leftOffsetX = posX == 0 ? (width - 1) : -1;
                int rightOffsetX = posX == width - 1 ? (1 - width) : 1;

                var neighbors = new Neighborns
                {
                    LeftUp = indexTop + leftOffsetX,
                    Up = indexTop,
                    RightUp = indexTop + rightOffsetX,
                    Left = arrayIndex + leftOffsetX,
                    Right = arrayIndex + rightOffsetX,
                    LeftDown = indexDown + leftOffsetX,
                    Down = indexDown,
                    RightDown = indexDown + rightOffsetX
                };
                return neighbors;
            }

            public void Execute(int index)
            {
                int posX = index % Width;
                int posY = index / Width;
                var neighbors = CalculateNeighborsOptimized(posX, posY, Width, Height, ActiveCells.Length);

                int aliveNeighbors = 0;
                aliveNeighbors += ActiveCells[neighbors.LeftUp].State;
                aliveNeighbors += ActiveCells[neighbors.Up].State;
                aliveNeighbors += ActiveCells[neighbors.RightUp].State;
                aliveNeighbors += ActiveCells[neighbors.Left].State;
                aliveNeighbors += ActiveCells[neighbors.Right].State;
                aliveNeighbors += ActiveCells[neighbors.LeftDown].State;
                aliveNeighbors += ActiveCells[neighbors.Down].State;
                aliveNeighbors += ActiveCells[neighbors.RightDown].State;


                bool isAlive = aliveNeighbors == 3 || aliveNeighbors == 2;
                var state = new CellState { State = isAlive ? (byte)1 : (byte)0 };
                FutureCellsState[index] = state;
            }
        }

        #endregion

    }
}
