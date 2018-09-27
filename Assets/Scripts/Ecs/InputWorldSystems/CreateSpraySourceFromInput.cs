using Unity.Entities;
using UnityEngine;

namespace alexnown.EcsLife
{
    [DisableAutoCreation]
    public class CreateSpraySourceFromInput : ComponentSystem
    {
        private ComponentGroup _worlds;
        protected override void OnCreateManager()
        {
            _worlds = GetComponentGroup(ComponentType.Create<CellsWorld>());
        }

        protected override void OnUpdate()
        {
            if (!Input.GetMouseButton(0)) return;
            float viewPosX = Input.mousePosition.x / Screen.width;
            float viewPosY = Input.mousePosition.y / Screen.height;
            var worlds = _worlds.GetSharedComponentDataArray<CellsWorld>();
            for (int i = 0; i < worlds.Length; i++)
            {
                var cellWorld = worlds[i];
                var em = cellWorld.World.GetOrCreateManager<EntityManager>();
                var sprayPoint = em.CreateEntity(ComponentType.Create<SprayComponent>(), ComponentType.Create<Position2D>());
                em.SetComponentData(sprayPoint, new Position2D { X = (int)(viewPosX * cellWorld.Width), Y = (int)(viewPosY * cellWorld.Height) });
                em.SetComponentData(sprayPoint, new SprayComponent
                {
                    Radius = Random.Range(8, 15),
                    Intensity = Random.Range(0.2f, 0.6f)
                });
            }
        }
    }
}
