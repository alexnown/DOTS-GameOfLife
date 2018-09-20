using Unity.Entities;
using UnityEngine;

namespace alexnown.EcsLife
{
    public struct SprayComponent : IComponentData
    {
        public int Radius;
        public float Intensity;
    }
}
