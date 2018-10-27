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
}
