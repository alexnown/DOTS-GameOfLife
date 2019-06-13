using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace alexnown.GameOfLife
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class CreateSpraySourceFromInput : ComponentSystem
    {
        private EntityArchetype _sprayArchetype;

        protected override void OnCreate()
        {
            _sprayArchetype = EntityManager.CreateArchetype(
                ComponentType.ReadOnly<SprayComponent>(),
                ComponentType.ReadOnly<ScreenViewPosition>());
        }

        protected override void OnUpdate()
        {
            if (!Input.GetMouseButton(0)) return;

            float viewPosX = Input.mousePosition.x / Screen.width;
            float viewPosY = Input.mousePosition.y / Screen.height;
            var sprayCommand = EntityManager.CreateEntity(_sprayArchetype);
            EntityManager.SetComponentData(sprayCommand, new ScreenViewPosition { Value = new float2(viewPosX, viewPosY) });
            EntityManager.SetComponentData(sprayCommand, new SprayComponent
            {
                Seed = UInt32.MaxValue - (uint)Time.frameCount,
                Radius = Random.Range(8, 15),
                Intensity = Random.Range(0.2f, 0.6f)
            });
        }
    }
}
