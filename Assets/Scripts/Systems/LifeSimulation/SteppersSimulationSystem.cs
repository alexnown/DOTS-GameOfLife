using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace alexnown.GameOfLife
{
    [UpdateInGroup(typeof(WorldSimulationSystemGroup))]
    public class SteppersSimulationSystem : ComponentSystem
    {
        [BurstCompile]
        struct SteppersUpdate : IJobParallelFor
        {
            [ReadOnly]
            public int Width;
            [ReadOnly]
            public int Length;
            public BlobAssetReference<WorldCellsData> CellsData;

            public void Execute(int index)
            {
                byte arrayIndex = CellsData.Value.ArrayIndex;
                CellsData.Value.ArrayIndex = arrayIndex;
                var cellState = arrayIndex == 0 ?
                    CellsData.Value.Array1[index] :
                    CellsData.Value.Array0[index];
                if (cellState == 1)
                {
                    if (arrayIndex == 0) CellsData.Value.Array0[index] = 2;
                    else CellsData.Value.Array1[index] = 2;
                    return;
                }
                int posX = index % Width;
                int posY = index / Width;

                var neighbors = Neighbors.Calculate(posX, posY, Width, Length);
                int old = 0;
                int young = 0;
                ReadCellState(arrayIndex, neighbors.LeftUp, ref old, ref young);
                ReadCellState(arrayIndex, neighbors.Up, ref old, ref young);
                ReadCellState(arrayIndex, neighbors.RightUp, ref old, ref young);
                ReadCellState(arrayIndex, neighbors.Left, ref old, ref young);
                ReadCellState(arrayIndex, neighbors.Right, ref old, ref young);
                ReadCellState(arrayIndex, neighbors.LeftDown, ref old, ref young);
                ReadCellState(arrayIndex, neighbors.Down, ref old, ref young);
                ReadCellState(arrayIndex, neighbors.RightDown, ref old, ref young);
                int totalSum = old + young;
                byte state = 0;
                if (cellState == 0 && totalSum == 3 && old > 1)
                {
                    state = 1;
                }
                else if (cellState == 2 && totalSum > 1 && totalSum < 4 && young < 2)
                {
                    state = 2;
                }
                if (arrayIndex == 0) CellsData.Value.Array0[index] = state;
                else CellsData.Value.Array1[index] = state;
            }

            private void ReadCellState(int arrayIndex, int index, ref int old, ref int young)
            {
                var cellState = arrayIndex == 0 ?
                    CellsData.Value.Array1[index] :
                    CellsData.Value.Array0[index];
                if (cellState == 1) young++;
                else if (cellState == 2) old++;
            }
        }

        private EntityQuery _cellWorlds;

        protected override void OnCreate()
        {
            base.OnCreate();
            _cellWorlds = GetEntityQuery(
                ComponentType.ReadOnly<IsSteppersSimulationTag>(),
                ComponentType.ReadOnly<WorldCellsComponent>(),
                ComponentType.ReadOnly<WorldSize>());
            RequireForUpdate(_cellWorlds);
        }

        protected override void OnUpdate()
        {
            Entities.With(_cellWorlds).ForEach((ref WorldSize size, ref WorldCellsComponent cellsData) =>
            {
                cellsData.Value.Value.ArrayIndex = (byte)((cellsData.Value.Value.ArrayIndex + 1) % 2);
                int length = cellsData.Value.Value.Array0.Length;
                var job = new SteppersUpdate
                {
                    Width = size.Width,
                    Length = length,
                    CellsData = cellsData.Value
                }.Schedule(length, (length / SystemInfo.processorCount) + 1);
                job.Complete();
            });
        }
    }
}
