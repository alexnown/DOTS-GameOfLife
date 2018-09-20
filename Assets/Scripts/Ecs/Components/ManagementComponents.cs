using Unity.Collections;
using Unity.Entities;

namespace alexnown.EcsLife
{
    public struct CellsDb : ISharedComponentData
    {
        public int Width;
        public int Height;
        public NativeArray<CellState> CellsState0;
        public NativeArray<CellState> CellsState1;

        public NativeArray<CellState> GetActiveCells(CellsDbState state)
        {
            return state.ActiveCellsState == 0 ? CellsState0 : CellsState1;
        }

        public NativeArray<CellState> GetFutureCells(CellsDbState state)
        {
            return state.ActiveCellsState != 0 ? CellsState0 : CellsState1;
        }
    }

    public struct CellsDbState : IComponentData
    {
        public int ActiveCellsState;
    }
    
}
