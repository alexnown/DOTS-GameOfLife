using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace alexnown.EcsLife
{
    [UpdateAfter(typeof(EndCellsUpdatesBarrier))]
    [UpdateBefore(typeof(ApplyFutureStatesSystem))]
    public class ApplySprayPointsToCells : JobComponentSystem
    {
        [Inject]
        private EndFrameBarrier _barrier;

        #region Job

        [BurstCompile]
        struct ProcessSprayPoints : IJobProcessComponentDataWithEntity<SprayComponent, Position2D>
        {
            [ReadOnly]
            public int Frame;
            [ReadOnly]
            public int Width;
            [ReadOnly]
            public int Height;
            [WriteOnly]
            [NativeDisableParallelForRestriction]
            public NativeArray<CellState> CellStates;
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity, int index, [ReadOnly]ref SprayComponent spray, [ReadOnly]ref Position2D pos)
            {
                CommandBuffer.DestroyEntity(0, entity);

                var random = new Unity.Mathematics.Random((uint)(UInt32.MaxValue - Frame - index));
                int points = (int)(spray.Intensity * math.PI * math.pow(spray.Radius, 2));
                for (int i = 0; i < points; i++)
                {
                    double theta = random.NextDouble(1) * (math.PI * 2);
                    double r = random.NextDouble(spray.Radius);
                    // Transform the polar coordinate to cartesian (x,y)
                    // and translate the center to the current mouse position
                    int x = (int)(pos.X + Math.Cos(theta) * r);
                    int y = (int)(pos.Y + Math.Sin(theta) * r);
                    bool inBounds = x >= 0 && x < Width && y >= 0 && y < Height;
                    if (inBounds)
                    {
                        int cellIndex = y * Width + x;
                        CellStates[cellIndex] = new CellState { State = 1 };
                    }
                }
            }
        }
        #endregion

        private ComponentGroup _cellsDb;
        protected override void OnCreateManager()
        {
            _cellsDb = GetComponentGroup(ComponentType.Create<CellsDb>(), ComponentType.Create<CellsDbState>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (_cellsDb.CalculateLength() == 0) return inputDeps;
            var cellsDb = _cellsDb.GetSharedComponentDataArray<CellsDb>()[0];
            var cellsDbState = _cellsDb.GetComponentDataArray<CellsDbState>()[0];
            var currCellsState = cellsDbState.ActiveCellsState != 0 ? cellsDb.CellsState0 : cellsDb.CellsState1;
            return new ProcessSprayPoints
            {
                Frame = Time.frameCount,
                Width = cellsDb.Width,
                Height = cellsDb.Height,
                CellStates = currCellsState,
                CommandBuffer = _barrier.CreateCommandBuffer().ToConcurrent()
            }.Schedule(this, inputDeps);
        }
    }
}
