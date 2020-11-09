using Unity.Entities;

namespace GameOfLife
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public class PeriodicalUpdateViewsSystemGroup : ComponentSystemGroup
    {
        public PeriodicalUpdateViewsSystemGroup()
        {
            FixedRateManager = new FixedRateUtils.FixedRateCatchUpManager(0.5f);
        }
    }
}
