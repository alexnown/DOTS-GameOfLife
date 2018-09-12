using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace alexnown.EcsLife
{
    public class ApplySprayPointsToCells : JobComponentSystem
    {
        public int Width;
        public int Height;
        [Inject]
        private EndCellsUpdatesBarrier _barrier;

        #region Job
        struct ProcessSprayPoints : IJobProcessComponentDataWithEntity<SprayComponent, Position2D>
        {
            [ReadOnly]
            public int Frame;
            [ReadOnly]
            public int Width;
            [ReadOnly]
            public int Height;
            [ReadOnly]
            public NativeArray<Entity> CellEntities;
            public EntityCommandBuffer.Concurrent CommandBuffer;

            public void Execute(Entity entity, int index, [ReadOnly]ref SprayComponent spray, [ReadOnly]ref Position2D pos)
            {
                CommandBuffer.DestroyEntity(entity);

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
                        var cellEntity = CellEntities[cellIndex];
                        CommandBuffer.SetComponent(cellEntity, new CellState { State = 1 });
                        CommandBuffer.SetComponent(cellEntity, spray.Style);
                    }
                }
            }
        }
        #endregion

        private ComponentGroup _cellsDb;
        protected override void OnCreateManager(int capacity)
        {
            _cellsDb = GetComponentGroup(ComponentType.Create<CellsDb>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (_cellsDb.CalculateLength() == 0) return inputDeps;
            var cellsDb = _cellsDb.GetSharedComponentDataArray<CellsDb>()[0];
            return new ProcessSprayPoints
            {
                Frame = Time.frameCount,
                Width = Width,
                Height = Height,
                CellEntities = cellsDb.Cells,
                CommandBuffer = _barrier.CreateCommandBuffer()
            }.Schedule(this, inputDeps);
        }
    }
}
