using System;
using Unity.Entities;
using UnityEngine;

namespace alexnown.EcsLife
{
    public class UpdateCellWorldsSystem : ComponentSystem
    {
        public Func<float> MaxTimeLimitSec = () => 1f;
        public Func<int> MaxUpdatesLimit = () => 1000;
        public int LastUpdatesCount { get; private set; }

        private ComponentGroup _worlds;
        protected override void OnCreateManager()
        {
            _worlds = GetComponentGroup(ComponentType.Create<CellsWorld>());
        }

        protected override void OnUpdate()
        {
            var worlds = _worlds.GetSharedComponentDataArray<CellsWorld>();
            var startTime = DateTime.Now;
            float maxTimeLimit = MaxTimeLimitSec.Invoke();
            int updateCountLimit = MaxUpdatesLimit.Invoke();
            LastUpdatesCount = 0;
            while (true)
            {
                for (int i = 0; i < worlds.Length; i++)
                {
                    foreach (var systems in worlds[i].World.BehaviourManagers)
                    {
                        systems.Update();
                    }
                }
                LastUpdatesCount++;
                if (LastUpdatesCount >= updateCountLimit || DateTime.Now.Subtract(startTime).TotalSeconds > maxTimeLimit) break;
            }
            Debug.Log($"Cell worlds updated {LastUpdatesCount} times.");
        }
        /*
        protected override void OnDestroyManager()
        {
            var worlds = _worlds.GetSharedComponentDataArray<CellsWorld>();
            for (int i = 0; i < worlds.Length; i++)
            {
                if(worlds[i].World.IsCreated) worlds[i].World.Dispose();
            }
        } */
    }
}
