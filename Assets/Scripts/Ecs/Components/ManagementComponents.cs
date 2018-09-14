using Unity.Collections;
using Unity.Entities;

namespace alexnown.EcsLife
{
    public struct CellsDb : ISharedComponentData
    {
        public int Width;
        public int Height;
        public NativeArray<CellState> Cells;
    }

    public struct ActiveState : IComponentData { }
    public struct FutureState : IComponentData { }
}
