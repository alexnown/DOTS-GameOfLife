using UnityEngine;

namespace alexnown.EcsLife
{
    [CreateAssetMenu(fileName = nameof(BootstrapSettings))]
    public class BootstrapSettings : ScriptableObject
    {

        public float ResolutionMultiplier => _resolutionMultiplier;
        public WorldRules UpdateRules => _updateRules;

        public Color32[] CellColors => _colorsByState;
        public float ColorsIntansity => _colorsIntensity;

        public bool InitializeManualUpdate => _initializeManualUpdate;
        public int MaxWorldsUpdatesLimit => _maxWorldsUpdatesLimit;
        public int PreferedFps => _preferedFps;

        [SerializeField]
        [Range(0.125f, 4)]
        private float _resolutionMultiplier = 1;
        [Range(10, 255)]
        [SerializeField]
        private int _greenColor = 255;

        [SerializeField]
        private WorldRules _updateRules = WorldRules.Default;

        [Header("ManualWorldsUpdate")]
        [SerializeField]
        private bool _initializeManualUpdate = false;
        [SerializeField]
        private int _maxWorldsUpdatesLimit = 100;
        [SerializeField]
        [Range(1, 60)]
        private int _preferedFps = 20;

        [Header("Customization")]
        [SerializeField]
        private Color32[] _colorsByState;
        [SerializeField]
        [Range(0.1f, 1)]
        private float _colorsIntensity = 1;
    }
}

