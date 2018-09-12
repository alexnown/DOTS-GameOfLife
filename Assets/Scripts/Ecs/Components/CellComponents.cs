using Unity.Entities;

namespace alexnown.EcsLife
{
    public struct CellStyle : IComponentData
    {
        public byte R;
        public byte G;
        public byte B;
    }

    public struct CellState : IComponentData
    {
        public int State;
    }

    public struct Position2D : IComponentData
    {
        public int X;
        public int Y;
    }
}
