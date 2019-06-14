using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

namespace alexnown.GameOfLife
{
    public class ConwaysSimulationSystem : ComponentSystem
    {
        [BurstCompile]
        struct UpdateCells : IJobParallelFor
        {
            [NativeSetThreadIndex]
            public int ThreadIndex;
            [ReadOnly]
            public int Width;
            [ReadOnly]
            public int Length;
            public BlobAssetReference<WorldCellsData> CellsData;

            public void Execute(int index)
            {
                if (CellsData.Value.ArrayIndex == 0)
                {
                    Process(ref CellsData.Value.Array1, ref CellsData.Value.Array0, index);
                }
                else Process(ref CellsData.Value.Array0, ref CellsData.Value.Array1, index);
            }

            private void Process(ref BlobArray<byte> cellsArray, ref BlobArray<byte> nextFrameCells, int index)
            {
                int posX = index % Width;
                int posY = index / Width;
                var neighbors = Neighbors.Calculate(posX, posY, Width, Length);

                int sum = 0;
                sum += cellsArray[neighbors.LeftUp];
                sum += cellsArray[neighbors.Up];
                sum += cellsArray[neighbors.RightUp];
                sum += cellsArray[neighbors.Left];
                sum += cellsArray[neighbors.Right];
                sum += cellsArray[neighbors.LeftDown];
                sum += cellsArray[neighbors.Down];
                sum += cellsArray[neighbors.RightDown];

                byte state = 0;
                bool isAlive = sum == 3 || (sum == 2 && cellsArray[index] == 1);
                if (isAlive) state = 1;
                nextFrameCells[index] = state;
            }
        }

        private EntityQuery _cellWorlds;
        private readonly Stopwatch _timer = new Stopwatch();

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
                _timer.Start();
                cellsData.Value.Value.ArrayIndex = (byte)((cellsData.Value.Value.ArrayIndex + 1) % 2);
                int length = cellsData.Value.Value.Array0.Length;
                var job = new UpdateCells
                {
                    Width = size.Width,
                    Length = length,
                    CellsData = cellsData.Value,
                }.Schedule(length, 1024);
                job.Complete();
                _timer.Stop();
                SimulationStatistics.SimulationsCount++;
                SimulationStatistics.SimulationTotalTicks += _timer.ElapsedTicks;
                _timer.Reset();
            });
        }
    }
}
