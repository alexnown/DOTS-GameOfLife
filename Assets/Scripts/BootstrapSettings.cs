using UnityEngine;

namespace alexnown.EcsLife
{
    [CreateAssetMenu(fileName = nameof(BootstrapSettings))]
    public class BootstrapSettings : ScriptableObject
    {
        public int ResolutionMultiplier => _resolutionMultiplier;
        public byte GreenColor => (byte)_greenColor;

        public bool InitializeManualUpdate => _initializeManualUpdate;
        public int MaxWorldsUpdatesLimit => _maxWorldsUpdatesLimit;
        public float MaxWorldsUpdatesTimeLimitSec => _maxWorldsUpdatesTimeLimitSec;

        [Range(1, 20)]
        [SerializeField]
        private int _resolutionMultiplier = 1;
        [Range(10, 255)]
        [SerializeField]
        private int _greenColor = 255;

        [Header("ManualWorldsUpdate")]
        [SerializeField]
        private bool _initializeManualUpdate;
        [SerializeField]
        private int _maxWorldsUpdatesLimit = 50;
        [SerializeField]
        private float _maxWorldsUpdatesTimeLimitSec = 0.5f;
    }
}

