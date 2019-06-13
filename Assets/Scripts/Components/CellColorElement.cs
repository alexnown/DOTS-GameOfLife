using Unity.Entities;

namespace alexnown.GameOfLife
{
    public struct CellColorElement : IBufferElementData
    {
        public byte R;
        public byte G;
        public byte B;
    }
}
