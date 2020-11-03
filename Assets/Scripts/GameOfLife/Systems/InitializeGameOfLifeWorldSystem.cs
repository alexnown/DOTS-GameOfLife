using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace alexnown.GameOfLife
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class InitializeGameOfLifeWorldSystem : SystemBase
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
                .ForEach((Entity e, in InitializeGameOfLifeWorld init) =>
            {
                var size = init.Size;
                if (init.SizeDependsScreenResolution) size = (int2)(screenSize * init.ScreenResolutionMultiplier);
                int elements = size.x * size.y;
                if (elements <= 0) throw new System.ArgumentException($"Cells count={elements}, must be greater than 0.");
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
