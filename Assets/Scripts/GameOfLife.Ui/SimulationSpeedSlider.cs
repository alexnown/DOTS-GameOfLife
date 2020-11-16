using System.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace GameOfLife
{
    public class SimulationSpeedSlider : MonoBehaviour
    {
        [SerializeField]
        private Slider _slider = null;
        [SerializeField]
        private Text _sliderText = null;
        private EntityQuery _advancesSettings;
        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            _advancesSettings = em.CreateEntityQuery(ComponentType.ReadWrite<AdvancedSimulationSettings>());
            _slider = GetComponent<Slider>();
            _slider.onValueChanged.AddListener((v) =>
            {
                _sliderText.text = $"{(int)v} updates per frame";
                using (var entities = _advancesSettings.ToEntityArray(Unity.Collections.Allocator.TempJob))
                {
                    for (int i = 0; i < entities.Length; i++)
                        em.SetComponentData(entities[i], new AdvancedSimulationSettings { MaxCyclesPerFrame = (int)v });
                }
            });
        }
    }
}
