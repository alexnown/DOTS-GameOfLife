using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace GameOfLife
{
    public static class ConwaysWorldUtils
    {
        public static readonly int4x3 CellNeighborsMask = new int4x3(
            new int4(0b111000101000111 << 15, 0b111000101000111 << 14, 0b111000101000111 << 13, 0b111000101000111 << 12),
           new int4(0b111000101000111 << 9, 0b111000101000111 << 8, 0b111000101000111 << 7, 0b111000101000111 << 6),
            new int4(0b111000101000111 << 3, 0b111000101000111 << 2, 0b111000101000111 << 1, 0b111000101000111));
        public static readonly int4x3 CellBitMask = new int4x3(
            new int4(1 << 22, 1 << 21, 1 << 20, 1 << 19),
            new int4(1 << 16, 1 << 15, 1 << 14, 1 << 13),
            new int4(1 << 10, 1 << 9, 1 << 8, 1 << 7));
        public static readonly int4x3 CellShifts = new int4x3(new int4(22, 21, 20, 19), new int4(16, 15, 14, 13), new int4(10, 9, 8, 7));

        #region int4 shifts
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 leftShift(int4 source, int4 shifts)
            => new int4(source.x << shifts.x, source.y << shifts.y, source.z << shifts.z, source.w << shifts.w);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4 rightShift(int4 source, int4 shifts)
            => new int4(source.x >> shifts.x, source.y >> shifts.y, source.z >> shifts.z, source.w >> shifts.w);
        #endregion

        #region int4x3 math
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int csum(int4x3 source) => math.csum(source.c0 + source.c1 + source.c2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4x3 countbits(int4x3 source)
            => new int4x3(math.countbits(source.c0), math.countbits(source.c1), math.countbits(source.c2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4x3 leftShift(int4x3 source, int4x3 shifts)
        {
            return new int4x3(leftShift(source.c0, shifts.c0), leftShift(source.c1, shifts.c1), leftShift(source.c2, shifts.c2));
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int4x3 rightShift(int4x3 source, int4x3 shifts)
        {
            return new int4x3(rightShift(source.c0, shifts.c0), rightShift(source.c1, shifts.c1), rightShift(source.c2, shifts.c2));
        }
        #endregion

        #region Update area jobs
        [BurstCompile]
        public struct SetHorizontalSidesInAreasJob : IJobParallelFor
        {
            private static readonly int ClearMask = 0b111111011110011110011110111111;
            private static readonly int LeftSideMask = 0b10000010000010000000000;
            private static readonly int RightSideMask = 0b10000010000010000000;

            public int Width;
            [NativeDisableParallelForRestriction]
            public NativeArray<int4> CellStates;
            public void Execute(int y)
            {
                var index = (Width * y);
                int endIndex = index + Width - 1;
                var nextArea = CellStates[index];
                var nextLeftBits = (nextArea & LeftSideMask) >> 4;
                var nextRightBits = (nextArea & RightSideMask) << 4;
                var prevRightBits = 0;
                while (index < endIndex)
                {
                    var resultArea = nextArea & ClearMask;
                    var leftBits = nextLeftBits;
                    var rightBits = nextRightBits;
                    nextArea = CellStates[index + 1];
                    nextLeftBits = (nextArea & LeftSideMask) >> 4;
                    nextRightBits = (nextArea & RightSideMask) << 4;
                    resultArea += new int4(prevRightBits, rightBits.x, rightBits.y, rightBits.z);
                    resultArea += new int4(leftBits.y, leftBits.z, leftBits.w, nextLeftBits.x);
                    CellStates[index] = resultArea;
                    prevRightBits = rightBits.w;
                    index++;
                }
                nextArea &= ClearMask;
                nextArea += new int4(prevRightBits, nextRightBits.x, nextRightBits.y, nextRightBits.z);
                nextArea += new int4(nextLeftBits.y, nextLeftBits.z, nextLeftBits.w, 0);
                CellStates[endIndex] = nextArea;
            }
        }
        [BurstCompile]
        public struct SetVerticalSidesInAreasJob : IJobParallelFor
        {
            private static readonly int ClearMask = 0b111111111111111111000000;
            private static readonly int TopSideMask = 0b111111000000000000000000;
            private static readonly int BottomSideMask = 0b111111000000;

            public int Width;
            [NativeDisableParallelForRestriction]
            public NativeArray<int4> CellStates;
            public void Execute(int x)
            {
                var index = x;
                int endIndex = CellStates.Length - Width + x;
                var area = CellStates[index];
                int4 bottomBits = default;
                while (index < endIndex)
                {
                    var nextAreaIndex = index + Width;
                    var nextArea = CellStates[nextAreaIndex];
                    var topBits = (nextArea & BottomSideMask) << 18;
                    CellStates[index] = (area & ClearMask) + bottomBits + topBits;
                    bottomBits = (area & TopSideMask) >> 18;
                    index = nextAreaIndex;
                    area = nextArea;
                }
                CellStates[endIndex] = (area & ClearMask) + bottomBits;
            }
        }

        [BurstCompile]
        public struct UpdateAreaCells_WithoutShiftToFirstBitJob : IJobParallelFor
        {
            public NativeArray<int> CellStates;

            public void Execute(int index)
            {
                var state = CellStates[index];
                var state4x3 = new int4x3(state);
                var neighborsBits = ((state4x3 >> 7) & CellBitMask) + ((state4x3 >> 6) & CellBitMask) + ((state4x3 >> 5) & CellBitMask)
                     + ((state4x3 >> 1) & CellBitMask) + ((state4x3 << 1) & CellBitMask)
                     + ((state4x3 << 5) & CellBitMask) + ((state4x3 << 6) & CellBitMask) + ((state4x3 << 7) & CellBitMask);
                //Check if cell has [3 alive neighbors] or [2 live neighbors + current cell state = isAlive], first bit will be 1
                var conwaysLifeRule = (neighborsBits >> 1) & (neighborsBits | state);
                //set cell state to die if neighbors count more 3
                conwaysLifeRule &= ~((neighborsBits >> 2) | (neighborsBits >> 3));
                CellStates[index] = csum(conwaysLifeRule & CellBitMask);
            }
        }

        [BurstCompile]
        public struct UpdateAreaCells_ManualShiftJob : IJobParallelFor
        {
            public NativeArray<int> CellStates;

            public void Execute(int index)
            {
                var state = CellStates[index];
                var neighborsBits = countbits(state & CellNeighborsMask);
                var cellStates = new int4x3(
                    new int4(state >> 22, state >> 21, state >> 20, state >> 19),
                    new int4(state >> 16, state >> 15, state >> 14, state >> 13),
                    new int4(state >> 10, state >> 9, state >> 8, state >> 7));
                //Check if cell has [3 alive neighbors] or [2 live neighbors + current cell state = isAlive], first bit will be 1
                var conwaysLifeRule = (neighborsBits >> 1) & (neighborsBits | cellStates);
                //set cell state to die if neighbors count more 3
                conwaysLifeRule &= ~((neighborsBits >> 2) | (neighborsBits >> 3));
                //clear all bits except first
                conwaysLifeRule &= 1;
                conwaysLifeRule.c0.x <<= 22;
                conwaysLifeRule.c0.y <<= 21;
                conwaysLifeRule.c0.z <<= 20;
                conwaysLifeRule.c0.w <<= 19;
                conwaysLifeRule.c1.x <<= 16;
                conwaysLifeRule.c1.y <<= 15;
                conwaysLifeRule.c1.z <<= 14;
                conwaysLifeRule.c1.w <<= 13;
                conwaysLifeRule.c2.x <<= 10;
                conwaysLifeRule.c2.y <<= 9;
                conwaysLifeRule.c2.z <<= 8;
                conwaysLifeRule.c2.w <<= 7;
                CellStates[index] = csum(conwaysLifeRule);
            }
        }

        [BurstCompile]
        public struct UpdateAreaCells_ShiftByDivisionJob : IJobParallelFor
        {
            public NativeArray<int> CellStates;

            public void Execute(int index)
            {
                var state = CellStates[index];
                var neighborsBits = countbits(state & CellNeighborsMask);
                var cellStates = state / CellBitMask;
                //Check if cell has [3 alive neighbors] or [2 live neighbors + current cell state = isAlive], first bit will be 1
                var conwaysLifeRule = (neighborsBits >> 1) & (neighborsBits | cellStates);
                //set cell state to die if neighbors count more 3
                conwaysLifeRule &= ~((neighborsBits >> 2) | (neighborsBits >> 3));
                CellStates[index] = csum((conwaysLifeRule & 1) * CellBitMask);
            }
        }

        [BurstCompile]
        public struct UpdateAreaCells_ShiftByMathJob : IJobParallelFor
        {

            public NativeArray<int> CellStates;

            public void Execute(int index)
            {
                var state = CellStates[index];
                var neighborsBits = countbits(state & CellNeighborsMask);
                var cellStates = rightShift(state, CellShifts);
                //Check if cell has [3 alive neighbors] or [2 live neighbors + current cell state = isAlive], first bit will be 1
                var conwaysLifeRule = (neighborsBits >> 1) & (neighborsBits | cellStates);
                //set cell state to die if neighbors count more 3
                conwaysLifeRule &= ~((neighborsBits >> 2) | (neighborsBits >> 3));
                CellStates[index] = csum(leftShift(conwaysLifeRule & 1, CellShifts));
            }
        }

        [BurstCompile]
        public struct UpdateAreaCells_ShiftNeighborsCountJob : IJobParallelFor
        {

            public NativeArray<int> CellStates;

            public void Execute(int index)
            {
                var state = CellStates[index];
                var neighborsBits = leftShift(countbits(state & CellNeighborsMask), CellShifts);
                //Check if cell has [3 alive neighbors] or [2 live neighbors + current cell state = isAlive], first bit will be 1
                var conwaysLifeRule = (neighborsBits >> 1) & (neighborsBits | state);
                //set cell state to die if neighbors count more 3
                conwaysLifeRule &= ~((neighborsBits >> 2) | (neighborsBits >> 3));
                CellStates[index] = csum(conwaysLifeRule & CellBitMask);
            }
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AreaState(int4x3 cellStates) => csum(leftShift(cellStates, CellShifts));

    }
}
