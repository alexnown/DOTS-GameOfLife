using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GameOfLife
{

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class InitializeSteppersWorldSystem : SystemBase
    {
        private EntityQuery _initializeRequests;
        private EntityCommandBufferSystem _barrier;

        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var cb = _barrier.CreateCommandBuffer();
            var screenSize = new float2(Screen.width, Screen.height);
            Entities.WithStoreEntityQueryInField(ref _initializeRequests)
                .WithAll<IsSteppersSimulationTag>()
                .ForEach((Entity e, in InitializeGameOfLifeWorld init) =>
            {
                var size = init.Size;
                if (size.x <= 0) size.x = Screen.width;
                if (size.y <= 0) size.y = Screen.height;
                int elements = size.x * size.y;
                cb.AddComponent(e, new WorldCellsComponent
                {
                    Size = size,
                    Value = ConstructBlob(elements)
                });
            }).WithoutBurst().Run();
            cb.RemoveComponent(_initializeRequests, ComponentType.ReadOnly<InitializeGameOfLifeWorld>());
            _barrier.AddJobHandleForProducer(Dependency);
        }
        private static BlobAssetReference<WorldCellsData> ConstructBlob(int size)
        {
            using (var builder = new BlobBuilder(Allocator.Temp))
            {
                ref var root = ref builder.ConstructRoot<WorldCellsData>();
                ref NativeArray<byte> array0 = ref builder.Allocate(ref root.Array0);
                array0 = new NativeArray<byte>(size, Allocator.Persistent);
                ref NativeArray<byte> array1 = ref builder.Allocate(ref root.Array1);
                array1 = new NativeArray<byte>(size, Allocator.Persistent);
                return builder.CreateBlobAssetReference<WorldCellsData>(Allocator.Persistent);
            }
        }
    }
}
