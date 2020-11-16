using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace GameOfLife
{
    public class SteppersSimulationSystem : SystemBase
    {
        struct Neighbors
        {
            public int LeftUp;
            public int Up;
            public int RightUp;
            public int Right;
            public int Left;
            public int LeftDown;
            public int Down;
            public int RightDown;

            public static Neighbors Calculate(int posX, int posY, int width, int length)
            {
                int arrayIndex = posY * width + posX;
                int indexTop = arrayIndex + width;
                if (indexTop >= length) indexTop -= length;
                int indexDown = arrayIndex - width;
                if (indexDown < 0) indexDown += length;
                int leftOffsetX = posX == 0 ? (width - 1) : -1;
                int rightOffsetX = posX == width - 1 ? (1 - width) : 1;

                return new Neighbors
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
            }

            public override string ToString()
            {
                return $"[{LeftUp} {Up} {RightUp} , {Left} x {Right}, {LeftDown} {Down} {RightDown}]";
            }
        }
        [BurstCompile]
        struct ShiftCellStatesJob : IJobParallelFor
        {
            public NativeArray<int4> CellStates;
            public void Execute(int index)
            {
                var state = CellStates[index];
                state = (state >> 4) & 0b11000000110000001100000011;
                CellStates[index] = state;
            }
        }

        [BurstCompile]
        struct UpdateCells : IJobParallelFor
        {
            public int Width;
            public int Length;
            [NativeDisableParallelForRestriction]
            public NativeArray<byte> CellStates;

            public void Execute(int index)
            {
                byte cellState = CellStates[index];
                if (cellState == 1)
                {
                    CellStates[index] = (byte)(cellState + (2 << 4));
                    return;
                }
                int posX = index % Width;
                int posY = index / Width;

                var neighbors = Neighbors.Calculate(posX, posY, Width, Length);
                var neighbors1 = new int4(
                    CellStates[neighbors.LeftUp],
                    CellStates[neighbors.Up],
                    CellStates[neighbors.RightUp],
                    CellStates[neighbors.Left]);
                var neighbors2 = new int4(
                    CellStates[neighbors.Right],
                    CellStates[neighbors.LeftDown],
                    CellStates[neighbors.Down],
                    CellStates[neighbors.RightDown]);
                int old = math.csum(((neighbors1 >> 1) & 1) + ((neighbors2 >> 1) & 1));
                int young = math.csum((neighbors1 & 1) + (neighbors2 & 1));
                int totalSum = old + young;
                //bool isBorn = (~cellState & (old + (young & 1))) == 3; 
                if (cellState == 0 && totalSum == 3 && old > 1)
                {
                    CellStates[index] = (byte)(cellState + (1 << 4));
                }
                else if (cellState == 2 && totalSum > 1 && totalSum < 4 && young < 2)
                {
                    CellStates[index] = (byte)(cellState + (2 << 4));
                }
            }
        }

        private readonly Stopwatch _timer = new Stopwatch();

        protected override void OnUpdate()
        {
            Dependency.Complete();
            Entities.WithAll<IsSteppersSimulation>()
                .ForEach((Entity e, GameOfLifeTexture texture) =>
                {
                    var cells = texture.Value.GetRawTextureData<byte>();
                    int cycles = 0;
                    long totalTicks = 0;
                    var settings = HasComponent<AdvancedSimulationSettings>(e)
                    ? GetComponent<AdvancedSimulationSettings>(e)
                    : new AdvancedSimulationSettings { MaxCyclesPerFrame = 1 };
                    while (cycles < settings.MaxCyclesPerFrame)
                    {
                        cycles++;
                        _timer.Start();
                        var job = new UpdateCells
                        {
                            CellStates = cells,
                            Width = texture.Value.width,
                            Length = cells.Length
                        }.Schedule(cells.Length, 512, Dependency);
                        var statesInt4 = cells.Reinterpret<int4>(UnsafeUtility.SizeOf<byte>());
                        Dependency = new ShiftCellStatesJob { CellStates = statesInt4 }.Schedule(statesInt4.Length, 64, job);
                        Dependency.Complete();
                        totalTicks += _timer.ElapsedTicks;
                        _timer.Reset();
                        if (settings.LimitationMs > 0 && totalTicks > settings.LimitationMs * 10000)
                            break;
                    }
                    if (HasComponent<SimulationStatistic>(e))
                    {
                        var stats = GetComponent<SimulationStatistic>(e);
                        stats.Age += cycles;
                        stats.SimulationTimeMs += totalTicks / 10000f;
                        SetComponent(e, stats);
                    }
                }).WithoutBurst().Run();
        }
    }
}
