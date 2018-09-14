using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;

namespace alexnown.EcsLife
{

    public class ApplyFutureStatesSystem : JobComponentSystem
    {
        [BurstCompile]
        struct ApplyJob : IJobProcessComponentData<CellsDbState>
        {
            public void Execute(ref CellsDbState data)
            {
                data.ActiveCellsState = (data.ActiveCellsState + 1) % 2;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new ApplyJob().Schedule(this, inputDeps);
            job.Complete();
            return job;
        }
    }

    /*
    public class ApplyFutureStatesSystem : JobComponentSystem
    {
        [Inject]
        private ApplyFutureStatesBarrier _barrier;

        private ComponentGroup _group;

        protected override void OnCreateManager(int capacity)
        {
            _group = GetComponentGroup(ComponentType.Create<CellsDbState>());
        }

        //[BurstCompile]
        struct ApplyJob : IJob
        {
            public EntityCommandBuffer Cb;
            public ComponentDataArray<CellsDbState> States;
            public EntityArray Entities;
            
            public void Execute()
            {
                for (int i = 0; i < Entities.Length; i++)
                {
                    int newState = (States[i].ActiveCellsState + 1) % 2;
                    Cb.SetComponent(Entities[i], new CellsDbState { ActiveCellsState = newState });
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new ApplyJob
            {
                Cb = _barrier.CreateCommandBuffer(),
                States = _group.GetComponentDataArray<CellsDbState>(),
                Entities = _group.GetEntityArray()
            }.Schedule(inputDeps);
        }
    }
    */
}
