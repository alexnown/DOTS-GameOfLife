using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace alexnown.GameOfLife
{
    [UpdateInGroup(typeof(WorldSimulationSystemGroup))]
    public class SteppersSimulationSystem : ComponentSystem
    {
        [BurstCompile]
        struct UpdateCells : IJobParallelFor
        {
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
                } else Process(ref CellsData.Value.Array0, ref CellsData.Value.Array1, index);
            }

            private void Process(ref BlobArray<byte> cellsArray, ref BlobArray<byte> nextFrameCells, int index)
            {
                byte cellState = cellsArray[index];
                if (cellState == 1)
                {
                    nextFrameCells[index] = 2;
                    return;
                }
                int posX = index % Width;
                int posY = index / Width;

                var neighbors = Neighbors.Calculate(posX, posY, Width, Length);
                int old = 0;
                int young = 0;
                IncreaseCounters(cellsArray[neighbors.LeftUp], ref old, ref young);
                IncreaseCounters(cellsArray[neighbors.Up], ref old, ref young);
                IncreaseCounters(cellsArray[neighbors.RightUp], ref old, ref young);
                IncreaseCounters(cellsArray[neighbors.Left], ref old, ref young);
                IncreaseCounters(cellsArray[neighbors.Right], ref old, ref young);
                IncreaseCounters(cellsArray[neighbors.LeftDown], ref old, ref young);
                IncreaseCounters(cellsArray[neighbors.Down], ref old, ref young);
                IncreaseCounters(cellsArray[neighbors.RightDown], ref old, ref young);
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
                nextFrameCells[index] = state;
            }

            private void IncreaseCounters(byte value, ref int old, ref int young)
            {
                if (value == 1) young++;
                else if (value == 2) old++;
            }
        }

        private EntityQuery _cellWorlds;
        private readonly Stopwatch _timer = new Stopwatch();

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
                _timer.Start();
                cellsData.Value.Value.ArrayIndex = (byte)((cellsData.Value.Value.ArrayIndex + 1) % 2);
                int length = cellsData.Value.Value.Array0.Length;
                var job = new UpdateCells
                {
                    Width = size.Width,
                    Length = length,
                    CellsData = cellsData.Value
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
