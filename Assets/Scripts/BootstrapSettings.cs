using UnityEngine;

namespace alexnown.EcsLife
{
    [CreateAssetMenu(fileName = nameof(BootstrapSettings))]
    public class BootstrapSettings : ScriptableObject
    {
        public int ResolutionMultiplier => _resolutionMultiplier;
        public byte GreenColor => (byte) _greenColor;

        [Range(1, 20)]
        [SerializeField]
        private int _resolutionMultiplier = 1;
        [Range(10, 255)]
        [SerializeField] private int _greenColor = 255;
    }
}

