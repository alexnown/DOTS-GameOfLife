using Unity.Entities;
using UnityEngine;

namespace GameOfLife
{
    [RequireComponent(typeof(RectTransform))]
    public class CreateSprayOnDragUpdate : MonoBehaviour
    {
        public int Radius = 10;
        public float Intensity = 0.4f;

        private void Update()
        {
            if (!Input.GetMouseButton(0)) return;
            var rectTransform = transform as RectTransform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out var pos))
            {
                if (rectTransform.rect.Contains(pos))
                {
                    var em = World.DefaultGameObjectInjectionWorld.EntityManager;
                    var sprayEntity = em.CreateEntity(ComponentType.ReadOnly<SprayComponent>());
                    em.SetComponentData(sprayEntity, new SprayComponent
                    {
                        Seed = 1 + (uint)UnityEngine.Time.frameCount,
                        Position = (pos / rectTransform.rect.size) + new Vector2(0.5f, 0.5f),
                        Radius = Radius,
                        Intensity = Intensity
                    });
                }
            }
        }
    }
}
