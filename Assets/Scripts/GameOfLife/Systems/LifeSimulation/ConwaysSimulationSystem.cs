using System.Diagnostics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;

namespace alexnown.GameOfLife
{
    [UpdateInGroup(typeof(WorldSimulationSystemGroup))]
    public class ConwaysSimulationSystem : SystemBase
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
                int posX = index % Width;
                int posY = index / Width;
                var neighbors = Neighbors.Calculate(posX, posY, Width, Length);

                int sum = 0;
                sum += CellStates[neighbors.LeftUp];
                sum += CellStates[neighbors.Up];
                sum += CellStates[neighbors.RightUp];
                sum += CellStates[neighbors.Left];
                sum += CellStates[neighbors.Right];
                sum += CellStates[neighbors.LeftDown];
                sum += CellStates[neighbors.Down];
                sum += CellStates[neighbors.RightDown];

                byte state = 0;
                bool isAlive = sum == 3 || (sum == 2 && CellStates[index] == 1);
                if (isAlive) state = 1;
                NextFrameCells[index] = state;
            }
        }

        private EntityQuery _cellWorlds;
        private readonly Stopwatch _timer = new Stopwatch();

        protected override void OnCreate()
        {
            base.OnCreate();
            _cellWorlds = GetEntityQuery(
                ComponentType.ReadOnly<IsConwaysSimulationTag>(),
                ComponentType.ReadOnly<WorldCellsComponent>());
            RequireForUpdate(_cellWorlds);
        }

        protected override void OnUpdate()
        {
            Entities.ForEach((ref WorldCellsComponent cellsData) =>
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
            }).WithoutBurst().Run();
        }

    }
}
