using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GameOfLife
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class InitializeConwaysWorldSystem : SystemBase
    {
        private EntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var cb = _barrier.CreateCommandBuffer();
            Entities
                .WithNone<GameOfLifeTexture>()
                .WithAll<IsConwaysSimulation>()
                .ForEach((Entity e, ref WorldSize world) =>
                {
                    if (world.Size.x <= 0) world.Size.x = Screen.width;
                    if (world.Size.y <= 0) world.Size.y = Screen.height;
                    var sizeInDemandedAreas = (int2)math.ceil(world.Size / new float2(16, 3));
                    world.Size = sizeInDemandedAreas * new int2(16, 3);
                    var texture = new Texture2D(4 * sizeInDemandedAreas.x, sizeInDemandedAreas.y, TextureFormat.RGBA32, false);
                    texture.wrapMode = TextureWrapMode.Clamp;
                    var array = texture.GetRawTextureData<int4>();
                    for (int i = 0; i < array.Length; i++) array[i] = 0;
                    texture.Apply();
                    cb.AddComponent(e, new GameOfLifeTexture { Value = texture });
                }).WithoutBurst().Run();
        }
    }
}
