using Unity.Entities;
using UnityEngine;

namespace GameOfLife
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class UnloadGameOfLifeTextureSystem : SystemBase
    {
        private EntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        }
        protected override void OnUpdate()
        {
            var cb = _barrier.CreateCommandBuffer();
            Entities.WithNone<WorldSize>()
                .ForEach((Entity e, GameOfLifeTexture texture) =>
                {
                    if (!texture.IsCreated) Resources.UnloadAsset(texture.Value);
                    cb.RemoveComponent(e, ComponentType.ReadOnly<GameOfLifeTexture>());
                })
                .WithoutBurst().Run();
        }
    }
}
