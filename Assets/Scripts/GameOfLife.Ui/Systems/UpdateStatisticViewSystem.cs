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
            var time = (float)Time.ElapsedTime;

            SimulationStatistic statistic = default;
            int2 size = default;
            Entities.ForEach((in SimulationStatistic simulationStats, in WorldSize world) =>
            {
                statistic = simulationStats;
                size = world.Size;
            }).Run();
            Entities.ForEach((Text textView, ref SimulationStatisticView view) =>
            {
                var ageDiff = statistic.Age - view.Age;
                var timeDiff = time - view.PrevTime;
                view.Age = statistic.Age;
                view.PrevTime = time;
                if (ageDiff > 0 && timeDiff > 0)
                {
                    var cellsCount = FormatCellsCount(size.x * size.y);
                    textView.text = $"Cells: {cellsCount}\nUpdate: {(statistic.SimulationTimeMs / statistic.Age).ToString("f1")} ms\nAge: {statistic.Age}\nSpeed: {Math.Round(ageDiff / timeDiff)}/s";
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
