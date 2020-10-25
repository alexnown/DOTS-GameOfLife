using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace alexnown.GameOfLife
{
    public enum Simulation
    {
        None,
        Conways,
        Steppers
    }

    [ExecuteInEditMode]
    public class GameOfLifeProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        [Header("Resolution")]
        public bool UseScreenResolution = true;
        [Range(0.125f, 4)]
        public float ScreenResolutionMultiplier = 1;
        public int2 TargetResolution;

        [Header("Simulation")]
        public Simulation Kind = Simulation.Conways;

        [Header("Customization")]
        public Color32[] CellColors = new Color32[1] { new Color32(0, 128, 0, 0) };

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            int2 size = UseScreenResolution ? CalculateTargetResolution() : TargetResolution;
            dstManager.AddComponentData(entity, new WorldSize { Width = size.x, Height = size.y });
            dstManager.AddComponentData(entity, new WorldCellsComponent { Value = ConstructBlob(size.x * size.y) });
            switch (Kind)
            {
                case Simulation.Steppers:
                    dstManager.AddComponentData(entity, default(IsSteppersSimulationTag));
                    break;
                case Simulation.Conways:
                    dstManager.AddComponentData(entity, default(IsConwaysSimulationTag));
                    break;
            }
            var colorsBuffer = dstManager.AddBuffer<CellColorElement>(entity);
            foreach (var color in CellColors)
            {
                colorsBuffer.Add(new CellColorElement { R = color.r, G = color.g, B = color.b });
            }
        }

        private void Update()
        {
            if (Application.isPlaying) return;
            if (UseScreenResolution) TargetResolution = CalculateTargetResolution();
        }

        private int2 CalculateTargetResolution() => new int2(
            (int)(Screen.width * ScreenResolutionMultiplier),
            (int)(Screen.height * ScreenResolutionMultiplier));

        public unsafe BlobAssetReference<WorldCellsData> ConstructBlob(int size)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<WorldCellsData>();
            builder.Allocate(ref root.Array0, size);
            builder.Allocate(ref root.Array1, size);
            var blobAsset = builder.CreateBlobAssetReference<WorldCellsData>(Allocator.Persistent);

            builder.Dispose();

            return blobAsset;
        }
    }
}
