using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace GameOfLife
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public class ApplySprayPointsToCells : SystemBase
    {
        private EntityQuery _sprayCommands;
        private EntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            RequireForUpdate(_sprayCommands);
        }

        protected override void OnUpdate()
        {
            Entities.WithStoreEntityQueryInField(ref _sprayCommands)
                .ForEach((in SprayComponent sprayComponent) =>
                {
                    var spray = sprayComponent;
                    Entities.WithName("ApplySprays_steppers")
                    .WithAll<IsSteppersSimulation>()
                    .ForEach((GameOfLifeTexture texture, in WorldSize world) =>
                    {
                        var size = world.Size;
                        var array = texture.Value.GetRawTextureData<byte>();
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
                    }).WithoutBurst().Run();
                    Entities.WithName("ApplySprays_conways")
                    .WithAll<IsConwaysSimulation>()
                    .ForEach((GameOfLifeTexture texture, in WorldSize world) =>
                    {
                        var areas = texture.Value.GetRawTextureData<int>();
                        var random = new Random(spray.Seed);
                        int points = (int)(spray.Intensity * math.PI * math.pow(spray.Radius, 2));
                        var centerPos = spray.Position * world.Size;
                        for (int i = 0; i < points; i++)
                        {
                            float theta = random.NextFloat(math.PI * 2);
                            float r = random.NextFloat(spray.Radius);
                            math.sincos(theta, out float sin, out float cos);
                            int2 pos = (int2)(centerPos + r * new float2(sin, cos));
                            bool inBounds = pos.x >= 0 && pos.x < world.Size.x && pos.y >= 0 && pos.y < world.Size.y;
                            if (inBounds)
                            {
                                var areaPos = pos / new int2(4, 3);
                                var areaIndex = areaPos.y * world.Size.x / 4 + areaPos.x;
                                var bitMask = ConwaysWorldUtils.CellBitMask[pos.y % 3][pos.x % 4];
                                areas[areaIndex] |= bitMask;
                            }
                        }
                    }).WithoutBurst().Run();

                }).WithoutBurst().Run();
            var cb = _barrier.CreateCommandBuffer();
            cb.DestroyEntity(_sprayCommands);
            _barrier.AddJobHandleForProducer(Dependency);
        }
    }
}
