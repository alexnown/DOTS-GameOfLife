﻿using Unity.Entities;

namespace alexnown.EcsLife
{
    public struct CellState : IComponentData
    {
        public byte State;
    }

    public struct Position2D : IComponentData
    {
        public int X;
        public int Y;
    }
}
