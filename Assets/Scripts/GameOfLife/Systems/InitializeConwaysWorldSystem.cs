using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GameOfLife
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class InitializeConwaysWorldSystem : SystemBase
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
                .WithAll<IsConwaysSimulationTag>()
                .ForEach((Entity e, in InitializeGameOfLifeWorld init) =>
                {
                    var size = init.Size;
                    if (size.x <= 0) size.x = Screen.width;
                    if (size.y <= 0) size.y = Screen.height;
                    var sizeInDemandedAreas = (int2)math.ceil(size / new float2(16, 3));
                    int elements = 4 * sizeInDemandedAreas.x * sizeInDemandedAreas.y;
                    size = sizeInDemandedAreas * new int2(16, 3);
                    cb.AddComponent(e, new CellsInAreas
                    {
                        Size = size,
                        Areas = ConstructBlob(elements)
                    });
                }).WithoutBurst().Run();
            cb.RemoveComponent(_initializeRequests, ComponentType.ReadOnly<InitializeGameOfLifeWorld>());
            _barrier.AddJobHandleForProducer(Dependency);
        }
        private static BlobAssetReference<AreasData> ConstructBlob(int size)
        {
            using (var builder = new BlobBuilder(Allocator.Temp))
            {
                ref var root = ref builder.ConstructRoot<AreasData>();
                ref NativeArray<int> array = ref builder.Allocate(ref root.ArrayPtr);
                array = new NativeArray<int>(size, Allocator.Persistent);
                return builder.CreateBlobAssetReference<AreasData>(Allocator.Persistent);
            }
        }
    }
}
