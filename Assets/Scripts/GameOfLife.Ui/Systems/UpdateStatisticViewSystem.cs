using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.UI;

namespace GameOfLife
{

    [UpdateInGroup(typeof(PeriodicalUpdateViewsSystemGroup))]
    public class UpdateStatisticViewSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithAll<SimulationStatisticView>()
                .ForEach((Text textView, ref SimulationStatisticView view) =>
                {
                    SimulationStatistic statistic = default;
                    int2 size = default;
                    Entities.ForEach((in SimulationStatistic simulationStats, in WorldSize world) =>
                    {
                        statistic = simulationStats;
                        size = world.Size;
                    }).Run();
                    if (statistic.Age > 0)
                    {
                        var cellsCount = FormatCellsCount(size.x * size.y);
                        textView.text = $"Cells: {cellsCount}    Cores: {UnityEngine.SystemInfo.processorCount}\nAge: {statistic.Age}\nUpdate: {(statistic.SimulationTimeMs / statistic.Age).ToString("f1")} ms\nSpeed: {Math.Round(statistic.Age / statistic.TotalTime)}/s";
                    }
                    else textView.text = string.Empty;
                }).WithoutBurst().Run();
        }

        private static string FormatCellsCount(int count)
        {
            if (count > 1000000) return $"{(count / 1000000f).ToString("f1")}M";
            else if (count > 1000) return $"{(count / 1000f).ToString("f1")}K";
            else return count.ToString();
        }
    }
}
