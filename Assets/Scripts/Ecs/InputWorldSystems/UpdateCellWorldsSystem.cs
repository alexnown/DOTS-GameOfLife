using System;
using Unity.Entities;
using UnityEngine;

namespace alexnown.EcsLife
{
    public class UpdateCellWorldsSystem : ComponentSystem
    {
        public static long TotalUpdates;
        public static float TotalTime;
        public static float UpdatesSpeed;


        public float MaxTimeLimitSec = 0.1f;
        public int MaxUpdatesForFrame = 1000;
        public int LastUpdatesCount { get; private set; }

        private ComponentGroup _worlds;
        private float UpdatesTime;
        private int UpdatesCount;
        protected override void OnCreateManager()
        {
            _worlds = GetComponentGroup(ComponentType.Create<CellsWorld>());
        }

        protected override void OnUpdate()
        {
            UpdateSpeedStatistic();
            LastUpdatesCount = 0;

            var worlds = _worlds.GetSharedComponentDataArray<CellsWorld>();
            var startTime = DateTime.Now;
            while (true)
            {
                for (int i = 0; i < worlds.Length; i++)
                {
                    worlds[i].World.GetExistingManager<UpdateCellsLifeRulesSystem>().Update();
                    worlds[i].World.GetExistingManager<ApplySprayPointsToCells>().Update();
                    worlds[i].World.GetExistingManager<ApplyFutureStatesSystem>().Update();
                }
                LastUpdatesCount++;
                if (LastUpdatesCount >= MaxUpdatesForFrame || DateTime.Now.Subtract(startTime).TotalSeconds > MaxTimeLimitSec) break;
            }

            Debug.Log($"Cell worlds updated {LastUpdatesCount} times.");
        }

        private void UpdateSpeedStatistic()
        {
            TotalUpdates += LastUpdatesCount;
            TotalTime += Time.deltaTime;
            UpdatesCount += LastUpdatesCount;
            UpdatesTime += Time.deltaTime;
            if (UpdatesTime >= 1)
            {
                UpdatesSpeed = UpdatesCount / UpdatesTime;
                UpdatesCount = 0;
                UpdatesTime = 0;
            }
        }
    }
}
