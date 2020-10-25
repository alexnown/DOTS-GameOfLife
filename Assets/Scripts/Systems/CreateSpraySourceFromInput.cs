using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace alexnown.GameOfLife
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class CreateSpraySourceFromInput : SystemBase
    {
        protected override void OnUpdate()
        {
            if (!Input.GetMouseButton(0)) return;
            float viewPosX = Input.mousePosition.x / Screen.width;
            float viewPosY = Input.mousePosition.y / Screen.height;

            var sprayCommand = EntityManager.CreateEntity(ComponentType.ReadOnly<SprayComponent>());
            EntityManager.SetComponentData(sprayCommand, new SprayComponent
            {
                Seed = 1 + (uint)UnityEngine.Time.frameCount,
                Position = new float2(viewPosX, viewPosY),
                Radius = Random.Range(8, 15),
                Intensity = Random.Range(0.2f, 0.6f)
            });
        }
    }
}
