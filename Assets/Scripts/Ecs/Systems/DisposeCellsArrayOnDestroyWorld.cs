using System.Collections.Generic;
using Unity.Entities;

namespace alexnown.EcsLife
{
    [DisableAutoCreation]
    public class DisposeCellsArrayOnDestroyWorld : JobComponentSystem
    {
        [Inject] private EntityManager _entityManager;
        protected override void OnDestroyManager()
        {
            List<CellsDb> cellsDbList = new List<CellsDb>(1);
            _entityManager.GetAllUniqueSharedComponentData(cellsDbList);
            for (int i = 0; i < cellsDbList.Count; i++)
            {
                if (cellsDbList[i].Cells.IsCreated)
                {
                    cellsDbList[i].Cells.Dispose();
                }
            }
            base.OnDestroyManager();
        }
    }
}
