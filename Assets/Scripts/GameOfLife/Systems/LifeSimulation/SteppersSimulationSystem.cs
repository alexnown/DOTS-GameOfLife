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
            [ReadOnly]
            public NativeArray<byte> CellStates;
            [WriteOnly]
            public NativeArray<byte> NextFrameCells;

            public void Execute(int index)
            {
                byte cellState = CellStates[index];
                if (cellState == 1)
                {
                    NextFrameCells[index] = 2;
                    return;
                }
                int posX = index % Width;
                int posY = index / Width;

                var neighbors = Neighbors.Calculate(posX, posY, Width, Length);
                int old = 0;
                int young = 0;
                IncreaseCounters(CellStates[neighbors.LeftUp], ref old, ref young);
                IncreaseCounters(CellStates[neighbors.Up], ref old, ref young);
                IncreaseCounters(CellStates[neighbors.RightUp], ref old, ref young);
                IncreaseCounters(CellStates[neighbors.Left], ref old, ref young);
                IncreaseCounters(CellStates[neighbors.Right], ref old, ref young);
                IncreaseCounters(CellStates[neighbors.LeftDown], ref old, ref young);
                IncreaseCounters(CellStates[neighbors.Down], ref old, ref young);
                IncreaseCounters(CellStates[neighbors.RightDown], ref old, ref young);
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
                NextFrameCells[index] = state;
            }


            private static void IncreaseCounters(byte value, ref int old, ref int young)
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
                ComponentType.ReadOnly<WorldCellsComponent>());
            RequireForUpdate(_cellWorlds);
        }

        protected override void OnUpdate()
        {
            Entities.With(_cellWorlds).ForEach((ref WorldCellsComponent cellsData) =>
            {
                _timer.Start();
                byte currIndex = cellsData.Value.Value.ArrayIndex;
                var cellArray = cellsData.GetCellsArray(currIndex);
                currIndex = (byte)((currIndex + 1) % 2);
                cellsData.Value.Value.ArrayIndex = currIndex;
                var nextFrameCells = cellsData.GetCellsArray(currIndex);
                int length = cellArray.Length;
                var job = new UpdateCells
                {
                    Width = cellsData.Size.x,
                    Length = length,
                    CellStates = cellArray,
                    NextFrameCells = nextFrameCells

                }.Schedule(length, 1024);
                job.Complete();
                _timer.Reset();
            });
        }
    }
}
