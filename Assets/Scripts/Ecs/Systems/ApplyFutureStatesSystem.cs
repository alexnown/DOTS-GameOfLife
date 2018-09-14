using System;
using alexnown.EcsLife;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace Assets.Scripts.Ecs.Systems
{
    [UpdateBefore(typeof(ApplyFutureStatesBarrier))]
    public class ApplyFutureStatesSystem : JobComponentSystem
    {
        private ComponentGroup _activeCellsDb;
        private ComponentGroup _futureCellsDb;
        [Inject]
        private ApplyFutureStatesBarrier _barrier;

        protected override void OnCreateManager(int capacity)
        {
            _activeCellsDb = GetComponentGroup(ComponentType.Create<ActiveState>(), ComponentType.Create<CellsDb>());
            _futureCellsDb = GetComponentGroup(ComponentType.Create<FutureState>(), ComponentType.Create<CellsDb>());
        }
        
        struct ApplyJob : IJob
        {
            public EntityCommandBuffer Cb;
            public Entity ActiveDbEntity;
            public Entity FutureDbEntity;

            public void Execute()
            {
                Cb.RemoveComponent<ActiveState>(ActiveDbEntity);
                Cb.AddComponent(ActiveDbEntity, new FutureState());
                Cb.RemoveComponent<FutureState>(FutureDbEntity);
                Cb.AddComponent(FutureDbEntity, new ActiveState());
            }
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (_activeCellsDb.CalculateLength() != 1) throw new InvalidOperationException($"Can't contains {_activeCellsDb.CalculateLength()} active cells db!");
            if (_futureCellsDb.CalculateLength() != 1) throw new InvalidOperationException($"Can't contains {_futureCellsDb.CalculateLength()} future cells db!");
            return new ApplyJob
            {
                Cb = _barrier.CreateCommandBuffer(),
                ActiveDbEntity = _activeCellsDb.GetEntityArray()[0],
                FutureDbEntity = _futureCellsDb.GetEntityArray()[0]
            }.Schedule(inputDeps);
        }
    }
}
