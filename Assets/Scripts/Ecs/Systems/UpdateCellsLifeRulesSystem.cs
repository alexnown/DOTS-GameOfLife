using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace alexnown.EcsLife
{
    [DisableAutoCreation]
    [UpdateBefore(typeof(EndCellsUpdatesBarrier))]
    public class UpdateCellsLifeRulesSystem : JobComponentSystem
    {
        public const int BATCHS_COUNT = 64;

        private ComponentGroup _activeCellsDb;
        private ComponentGroup _futureCellsDb;

        protected override void OnCreateManager(int capacity)
        {
            _activeCellsDb = GetComponentGroup(ComponentType.Create<ActiveState>(), ComponentType.Create<CellsDb>());
            _futureCellsDb = GetComponentGroup(ComponentType.Create<FutureState>(), ComponentType.Create<CellsDb>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (_activeCellsDb.CalculateLength() != 1) throw new InvalidOperationException($"Can't contains {_activeCellsDb.CalculateLength()} active cells db!");
            if (_futureCellsDb.CalculateLength() != 1) throw new InvalidOperationException($"Can't contains {_futureCellsDb.CalculateLength()} future cells db!");
            var activeCellsDb = _activeCellsDb.GetSharedComponentDataArray<CellsDb>()[0];
            int length = activeCellsDb.Cells.Length;
            return new UpdateCellsState
            {
                Width = activeCellsDb.Width,
                Height = activeCellsDb.Height,
                ActiveCells = activeCellsDb.Cells,
                FutureCellsState = _futureCellsDb.GetSharedComponentDataArray<CellsDb>()[0].Cells,
                AliveState = new CellState { State = 1, G = Bootstrap.Settings.GreenColor }
            }.ScheduleBatch(length, (length / BATCHS_COUNT + 1), inputDeps);
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
        struct UpdateCellsState : IJobParallelForBatch
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
            [ReadOnly]
            public CellState AliveState;

            public void Execute(int startIndex, int count)
            {
                int length = ActiveCells.Length;
                for (int i = 0; i < count; i++)
                {
                    int index = startIndex + i;
                    int posX = index % Width;
                    int posY = index / Width;
                    var neighbors = CalculateNeighborsOptimized(posX, posY, Width, Height, length);
                    //bool wrongNeighbors = true;
                    //neighbors.LeftUp > length || neighbors.Up > length ||
                    //                      neighbors.RightUp > length || neighbors.Left > length || neighbors.Right > length ||
                    //   neighbors.LeftDown > length || neighbors.Down > length || neighbors.RightDown > length;
                    //if (wrongNeighbors) Debug.Log($"[{posX}, {posY}] neighbors=[{neighbors}]");


                    int aliveNeighbors = 0;
                    aliveNeighbors += ActiveCells[neighbors.LeftUp].State;
                    aliveNeighbors += ActiveCells[neighbors.Up].State;
                    aliveNeighbors += ActiveCells[neighbors.RightUp].State;
                    aliveNeighbors += ActiveCells[neighbors.Left].State;
                    aliveNeighbors += ActiveCells[neighbors.Right].State;
                    aliveNeighbors += ActiveCells[neighbors.LeftDown].State;
                    aliveNeighbors += ActiveCells[neighbors.Down].State;
                    aliveNeighbors += ActiveCells[neighbors.RightDown].State;


                    bool isAlive = aliveNeighbors == 3 || (aliveNeighbors == 2 && ActiveCells[index].State != 1);
                    FutureCellsState[startIndex + i] = isAlive ? AliveState : new CellState();

                    //FutureCellsState[index] = ActiveCells[index];
                }
            }



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
        }

        #endregion

    }
}
