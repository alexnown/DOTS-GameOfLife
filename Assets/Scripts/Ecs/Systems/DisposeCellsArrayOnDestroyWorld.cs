using System.Collections.Generic;
using Unity.Entities;

namespace alexnown.EcsLife
{
    [DisableAutoCreation] [UpdateAfter(typeof(EndFrameBarrier))]
    public class DisposeCellsArrayOnDestroyWorld : JobComponentSystem
    {
        protected override void OnDestroyManager()
        {
            List<CellsDb> cellsDbList = new List<CellsDb>();
            EntityManager.GetAllUniqueSharedComponentData(cellsDbList);
            for (int i = 0; i < cellsDbList.Count; i++)
            {
                if (cellsDbList[i].CellsState0.IsCreated)
                {
                    cellsDbList[i].CellsState0.Dispose();
                }
                if (cellsDbList[i].CellsState1.IsCreated)
                {
                    cellsDbList[i].CellsState1.Dispose();
                }
            }
            base.OnDestroyManager();
        }
    }
}
