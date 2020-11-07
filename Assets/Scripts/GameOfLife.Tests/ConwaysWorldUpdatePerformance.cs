using NUnit.Framework;
using Unity.PerformanceTesting;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using static GameOfLife.ConwaysWorldUtils;

namespace GameOfLife
{
    public class ConwaysWorldUpdatePerformance
    {
        [TestCase(4800, 360, 512)]
        [Performance]
        public void UpdateCellsStateInAreas_ManualShift(int width, int height, int barchCount)
        {
            NativeArray<int> cellsArray = default;
            Measure.Method(() =>
            {
                var job = new UpdateAreaCells_ManualShiftJob
                {
                    CellStates = cellsArray
                }.Schedule(cellsArray.Length, barchCount);
                job.Complete();
            })
                .SetUp(() =>
                {
                    cellsArray = new NativeArray<int>(width * height, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                })
                .CleanUp(() =>
                {
                    cellsArray.Dispose();
                })
                .Run();
        }

        [TestCase(4800, 360, 512)]
        [Performance]
        public void UpdateCellsStateInAreas_ShiftByDivision(int width, int height, int batchCount)
        {
            NativeArray<int> cellsArray = default;
            Measure.Method(() =>
            {
                var job = new UpdateAreaCells_ShiftByDivisionJob
                {
                    CellStates = cellsArray
                }.Schedule(cellsArray.Length, batchCount);
                job.Complete();
            })
                .SetUp(() =>
                {
                    cellsArray = new NativeArray<int>(width * height, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                })
                .CleanUp(() =>
                {
                    cellsArray.Dispose();
                })
                .Run();
        }

        [TestCase(4800, 360, 512)]
        [Performance]
        public void UpdateCellsStateInAreas_ShiftByMath(int width, int height, int batchCount)
        {
            NativeArray<int> cellsArray = default;
            Measure.Method(() =>
            {
                var job = new UpdateAreaCells_ShiftByMathJob
                {
                    CellStates = cellsArray
                }.Schedule(cellsArray.Length, batchCount);
                job.Complete();
            })
                .SetUp(() =>
                {
                    cellsArray = new NativeArray<int>(width * height, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                })
                .CleanUp(() =>
                {
                    cellsArray.Dispose();
                })
                .Run();
        }

        [TestCase(4800, 360, 512)]
        [Performance]
        public void UpdateCellsStateInAreas_WithoutShiftToFirstBit(int width, int height, int batchCount)
        {
            NativeArray<int> cellsArray = default;
            Measure.Method(() =>
            {
                var job = new UpdateAreaCells_WithoutShiftToFirstBitJob
                {
                    CellStates = cellsArray
                }.Schedule(cellsArray.Length, batchCount);
                job.Complete();
            })
                .SetUp(() =>
                {
                    cellsArray = new NativeArray<int>(width * height, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                })
                .CleanUp(() =>
                {
                    cellsArray.Dispose();
                })
                .Run();
        }

        [TestCase(4800, 360, 512)]
        [Performance]
        public void UpdateCellsStateInAreas_ShiftNeighborsCount(int width, int height, int batchCount)
        {
            NativeArray<int> cellsArray = default;
            Measure.Method(() =>
            {
                var job = new UpdateAreaCells_ShiftNeighborsCountJob
                {
                    CellStates = cellsArray
                }.Schedule(cellsArray.Length, batchCount);
                job.Complete();
            })
                .SetUp(() =>
                {
                    cellsArray = new NativeArray<int>(width * height, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                })
                .CleanUp(() =>
                {
                    cellsArray.Dispose();
                })
                .Run();
        }

        [TestCase(4800, 360, 512)]
        [Performance]
        public void UpdateConwaysNoLoopedWorld(int width, int height, int batchCount)
        {
            NativeArray<int> cellsArray = default;
            Measure.Method(() =>
            {
                var job = new UpdateAreaCells_ShiftNeighborsCountJob
                {
                    CellStates = cellsArray
                }.Schedule(cellsArray.Length, batchCount);
                var statesInt4 = cellsArray.Reinterpret<int4>(UnsafeUtility.SizeOf<int>());
                job = new SetHorizontalSidesInAreasJob
                {
                    Width = width / 4,
                    CellStates = statesInt4
                }.Schedule(height, 64, job);
                job = new SetVerticalSidesInAreasJob
                {
                    Width = width / 4,
                    CellStates = statesInt4
                }.Schedule(width / 4, 64, job);
                job.Complete();
            })
                .SetUp(() =>
                {
                    cellsArray = new NativeArray<int>(width * height, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                })
                .CleanUp(() =>
                {
                    cellsArray.Dispose();
                })
                .Run();
        }
    }
}