using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace alexnown.GameOfLife
{
    public class ConwaysSimulationSystem : ComponentSystem
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
                int posX = index % Width;
                int posY = index / Width;
                var neighbors = Neighbors.Calculate(posX, posY, Width, Length);

                int sum = 0;
                ReadCellState(arrayIndex, neighbors.LeftUp, ref sum);
                ReadCellState(arrayIndex, neighbors.Up, ref sum);
                ReadCellState(arrayIndex, neighbors.RightUp, ref sum);
                ReadCellState(arrayIndex, neighbors.Left, ref sum);
                ReadCellState(arrayIndex, neighbors.Right, ref sum);
                ReadCellState(arrayIndex, neighbors.LeftDown, ref sum);
                ReadCellState(arrayIndex, neighbors.Down, ref sum);
                ReadCellState(arrayIndex, neighbors.RightDown, ref sum);

                int cellState = 0;
                ReadCellState(arrayIndex, index, ref cellState);
                byte state = 0;
                bool isAlive = sum == 3 || (sum == 2 && cellState == 1);
                if (isAlive) state = 1;
                if (arrayIndex == 0) CellsData.Value.Array0[index] = state;
                else CellsData.Value.Array1[index] = state;
            }

            private void ReadCellState(int arrayIndex, int index, ref int sum)
            {
                var cellState = arrayIndex == 0 ?
                    CellsData.Value.Array1[index] :
                    CellsData.Value.Array0[index];
                sum += cellState;
            }
        }

        private EntityQuery _cellWorlds;

        protected override void OnCreate()
        {
            base.OnCreate();
            _cellWorlds = GetEntityQuery(
                ComponentType.ReadOnly<IsConwaysSimulationTag>(),
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
