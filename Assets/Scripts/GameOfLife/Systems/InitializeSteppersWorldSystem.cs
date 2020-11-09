using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GameOfLife
{

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class InitializeSteppersWorldSystem : SystemBase
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
                .WithAll<IsSteppersSimulation>()
                .ForEach((Entity e, ref WorldSize world) =>
                {
                    if (world.Size.x <= 0) world.Size.x = Screen.width;
                    if (world.Size.y <= 0) world.Size.y = Screen.height;
                    var texture = new Texture2D(world.Size.x, world.Size.y, TextureFormat.R8, false);
                    texture.wrapMode = TextureWrapMode.Clamp;
                    var array = texture.GetRawTextureData<int4>();
                    for (int i = 0; i < array.Length; i++) array[i] = 0;
                    texture.Apply();
                    cb.AddComponent(e, new GameOfLifeTexture { Value = texture });
                }).WithoutBurst().Run();
        }
    }
}
