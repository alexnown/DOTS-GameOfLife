using Unity.Entities;
using UnityEngine;

namespace alexnown.EcsLife
{
    [AlwaysUpdateSystem]
    [DisableAutoCreation]
    public class CreateSpraySourceFromInput : ComponentSystem
    {
        protected override void OnUpdate()
        {
            if (!Input.GetMouseButton(0)) return;
            float viewPosX = Input.mousePosition.x / Screen.width;
            float viewPosY = Input.mousePosition.y / Screen.height;
            var em = Bootstrap.CellsWorld.GetOrCreateManager<EntityManager>();
            
            var sprayPoint = em.CreateEntity(ComponentType.Create<SprayComponent>(), ComponentType.Create<Position2D>());
            em.SetComponentData(sprayPoint, new Position2D { X = (int)(viewPosX * Bootstrap.Width), Y = (int)(viewPosY * Bootstrap.Height) });
            em.SetComponentData(sprayPoint, new SprayComponent
            {
                Radius = Random.Range(8, 15),
                Intensity = Random.Range(0.2f, 0.6f),
                Style = new CellState { State = 1, G = Bootstrap.Settings.GreenColor }
            });
        }
    }
}
