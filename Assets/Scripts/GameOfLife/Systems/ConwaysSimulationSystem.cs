﻿using Unity.Entities;
using Unity.Jobs;
using static GameOfLife.ConwaysWorldUtils;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using System.Diagnostics;

namespace GameOfLife
{
    public class ConwaysSimulationSystem : SystemBase
    {
        private readonly Stopwatch _timer = new Stopwatch();
        protected override void OnUpdate()
        {
            Dependency.Complete();
            Entities.WithAll<IsConwaysSimulation>()
                .ForEach((Entity e, GameOfLifeTexture texture, in WorldSize world) =>
                {
                    var areas = texture.Value.GetRawTextureData<int>();
                    int cycles = 0;
                    long totalTicks = 0;
                    var settings = HasComponent<AdvancedSimulationSettings>(e)
                    ? GetComponent<AdvancedSimulationSettings>(e)
                    : new AdvancedSimulationSettings { MaxCyclesPerFrame = 1 };
                    while (cycles < settings.MaxCyclesPerFrame)
                    {
                        cycles++;
                        _timer.Start();
                        var job = new UpdateAreaCells_ManualShiftJob
                        {
                            CellStates = areas
                        }.Schedule(areas.Length, 256, Dependency);
                        var statesInt4 = areas.Reinterpret<int4>(UnsafeUtility.SizeOf<int>());
                        var widthInArea4 = world.Size.x / 16;
                        job = new SetHorizontalSidesInAreasJob
                        {
                            Width = widthInArea4,
                            CellStates = statesInt4
                        }.Schedule(world.Size.y / 3, 32, job);
                        Dependency = new SetVerticalSidesInAreasJob
                        {
                            Width = widthInArea4,
                            CellStates = statesInt4
                        }.Schedule(widthInArea4, 32, job);
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
