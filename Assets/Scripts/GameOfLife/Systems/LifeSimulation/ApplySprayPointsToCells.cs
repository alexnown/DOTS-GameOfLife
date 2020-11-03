using alexnown.GameOfLife;
using Unity.Entities;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace alexnown.EcsLife
{
    [UpdateBefore(typeof(WorldSimulationSystemGroup))]
    public class ApplySprayPointsToCells : SystemBase
    {
        private EntityQuery _sprayCommands;
        private EntityQuery _cellWorlds;
        private EntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            RequireForUpdate(_sprayCommands);
            _cellWorlds = GetEntityQuery(ComponentType.ReadOnly<WorldCellsComponent>());
            RequireForUpdate(_cellWorlds);
        }

        protected override void OnUpdate()
        {

            Entities.WithStoreEntityQueryInField(ref _sprayCommands)
                .ForEach((in SprayComponent sprayComponent) =>
                {
                    var spray = sprayComponent;
                    Entities.WithName("ApplySprays")
                    .ForEach((ref WorldCellsComponent cells) =>
                    {
                        var size = cells.Size;
                        ref var cellData = ref cells.Value.Value;
                        var array = cells.GetActiveCells();
                        var random = new Random(spray.Seed);
                        int points = (int)(spray.Intensity * math.PI * math.pow(spray.Radius, 2));
                        var centerPos = spray.Position * new float2(size.x, size.y);
                        for (int i = 0; i < points; i++)
                        {
                            float theta = random.NextFloat(math.PI * 2);
                            float r = random.NextFloat(spray.Radius);
                            math.sincos(theta, out float sin, out float cos);
                            int2 pos = (int2)(centerPos + r * new float2(sin, cos));
                            bool inBounds = pos.x >= 0 && pos.x < size.x && pos.y >= 0 && pos.y < size.y;
                            if (inBounds)
                            {
                                int cellIndex = pos.y * size.x + pos.x;
                                array[cellIndex] = 1;
                            }
                        }
                    }).Run();
                }).WithoutBurst().Run();
            var cb = _barrier.CreateCommandBuffer();
            cb.DestroyEntity(_sprayCommands);
            _barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
