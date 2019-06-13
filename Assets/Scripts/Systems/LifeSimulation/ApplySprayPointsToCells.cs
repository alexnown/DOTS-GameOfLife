using alexnown.GameOfLife;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace alexnown.EcsLife
{
    [UpdateBefore(typeof(WorldSimulationSystemGroup))]
    public class ApplySprayPointsToCells : ComponentSystem
    {
        [BurstCompile]
        struct ProcessSprayPoints : IJobForEachWithEntity<SprayComponent, ScreenViewPosition>
        {
            public WorldSize Size;
            public BlobAssetReference<WorldCellsData> CellsReference;
            public void Execute(Entity entity, int index, [ReadOnly]ref SprayComponent spray, [ReadOnly]ref ScreenViewPosition pos)
            {
                var random = new Random(spray.Seed);
                int points = (int)(spray.Intensity * math.PI * math.pow(spray.Radius, 2));
                for (int i = 0; i < points; i++)
                {
                    double theta = random.NextDouble(1) * (math.PI * 2);
                    double r = random.NextDouble(spray.Radius);
                    //Transform the polar coordinate to cartesian (x,y)
                    //and translate the center to the current mouse position
                    int posX = (int)(Size.Width * pos.Value.x);
                    int posY = (int)(Size.Height * pos.Value.y);
                    int x = (int)(posX + math.cos(theta) * r);
                    int y = (int)(posY + math.sin(theta) * r);
                    bool inBounds = x >= 0 && x < Size.Width && y >= 0 && y < Size.Height;
                    if (inBounds)
                    {
                        int cellIndex = y * Size.Width + x;
                        if (CellsReference.Value.ArrayIndex == 0) CellsReference.Value.Array0[cellIndex] = 1;
                        else CellsReference.Value.Array1[cellIndex] = 1;
                    }
                }
            }
        }

        private EntityQuery _sprayCommands;
        private EntityQuery _cellWorlds;

        protected override void OnCreate()
        {
            base.OnCreate();
            _sprayCommands = GetEntityQuery(
                ComponentType.ReadOnly<SprayComponent>(),
                ComponentType.ReadOnly<ScreenViewPosition>());
            RequireForUpdate(_sprayCommands);
            _cellWorlds = GetEntityQuery(
                ComponentType.ReadOnly<WorldSize>(),
                ComponentType.ReadOnly<WorldCellsComponent>());
            RequireForUpdate(_cellWorlds);
        }

        protected override void OnUpdate()
        {
            Entities.With(_cellWorlds).ForEach((ref WorldSize size, ref WorldCellsComponent cellsData) =>
            {
                var job = new ProcessSprayPoints
                {
                    Size = size,
                    CellsReference = cellsData.Value
                }.ScheduleSingle(_sprayCommands);
                job.Complete();
            });
            PostUpdateCommands.DestroyEntity(_sprayCommands);
        }
    }
}
