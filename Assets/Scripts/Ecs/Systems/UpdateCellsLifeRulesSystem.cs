using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace alexnown.EcsLife
{
    [DisableAutoCreation]
    [UpdateBefore(typeof(EndCellsUpdatesBarrier))]
    public class UpdateCellsLifeRulesSystem : JobComponentSystem
    {
        private ComponentGroup _cellsDbGroup;
        [Inject]
        private ComponentDataFromEntity<CellState> _cellStates;

        protected override void OnCreateManager(int capacity)
        {
            _cellsDbGroup = GetComponentGroup(ComponentType.Create<CellsDb>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (_cellsDbGroup.CalculateLength() == 0) return inputDeps;
            var cellsDb = _cellsDbGroup.GetSharedComponentDataArray<CellsDb>()[0];
            var cellEntities = cellsDb.Cells;
            NativeArray<CellState> cellStates = new NativeArray<CellState>(cellEntities.Length, Allocator.TempJob);
            for (int i = 0; i < cellEntities.Length; i++)
            {
                cellStates[i] = _cellStates[cellEntities[i]];
            }
            var job = new UpdateCellState
            {
                Width = cellsDb.Width,
                Height = cellsDb.Height,
                CellStates = cellStates,
                AliceCell = new CellStyle { G = Bootstrap.Settings.GreenColor }
            }.Schedule(this, inputDeps);
            job.Complete();
            cellStates.Dispose();
            return job;
        }

        [BurstCompile]
        struct UpdateCellState : IJobProcessComponentData<CellState, Position2D, CellStyle>
        {
            public int Width;
            public int Height;
            [ReadOnly]
            public NativeArray<CellState> CellStates;

            public CellStyle AliceCell;
            public CellStyle EmptyCell;

            public void Execute(ref CellState state, [ReadOnly]ref Position2D position, ref CellStyle style)
            {
                //int aliveNeighborsCount = 0;
                //int cellsArrayPos = position.Y * Width + position.X;
                //var leftNeighborIndex = CalcArrayIndex(position.X, position.Y, cellsArrayPos, -1, 0);
                //var rightNeighborIndex = CalcArrayIndex(position.X, position.Y, cellsArrayPos, 1, 0);
                //var upNeighborIndex = CalcArrayIndex(position.X, position.Y, cellsArrayPos, 0, 1);
                //var downNeighborIndex = CalcArrayIndex(position.X, position.Y, cellsArrayPos, 0, -1);

                //aliveNeighborsCount += CellStates[leftNeighborIndex].State;
                //aliveNeighborsCount += CellStates[rightNeighborIndex].State;
                //aliveNeighborsCount += CellStates[upNeighborIndex].State;
                //aliveNeighborsCount += CellStates[downNeighborIndex].State;

                //bool isAlive = aliveNeighborsCount == 3 || (aliveNeighborsCount == 2 && state.State == 1);
                //if (isAlive)
                //{
                //    state.State = 1;
                //    style = AliceCell;
                //}
                //else
                //{
                //    state.State = 0;
                //    style = EmptyCell;
                //}
            }

            public int CalcArrayIndex(int posX, int posY, int cellsArrayPos, int offsetX, int offsetY)
            {
                int index = cellsArrayPos;
                if (offsetX == -1) return posX > 0 ? (cellsArrayPos - 1) : (cellsArrayPos + Width - 1);
                else if (offsetX == 1) return posX < Width - 1 ? (cellsArrayPos + 1) : (cellsArrayPos - Width + 1);
                else if (offsetY == 1) return posY < Height - 1 ? (cellsArrayPos + Width) : posX;
                else if (offsetY == -1) return posY > 0 ? (cellsArrayPos - Width) : (CellStates.Length - Width + posX);
                //todo: do calc
                return index;
            }
        }

    }
}
