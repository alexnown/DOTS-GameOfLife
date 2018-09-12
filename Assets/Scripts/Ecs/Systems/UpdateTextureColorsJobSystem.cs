using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

namespace alexnown.EcsLife
{
    [DisableAutoCreation] [UpdateAfter(typeof(EndCellsUpdatesBarrier))]
    public class UpdateTextureColorsJobSystem : JobComponentSystem
    {
        public bool TexturePrepared { get; private set; }
        public NativeArray<byte> _textureColorsRGB;
        private int _width;
        private int _height;

        public void FillTargetArray(NativeArray<byte> colors, int width, int height)
        {
            _textureColorsRGB = colors;
            _width = width;
            _height = height;
            Enabled = true;
            TexturePrepared = false;
        }

        protected override void OnCreateManager(int capacity)
        {
            Enabled = false;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            Enabled = false;
            TexturePrepared = true;
            return new UpdateTextureColors
            {
                ColorsArray = _textureColorsRGB,
                Width = _width
            }.Schedule(this, inputDeps);
        }

        [BurstCompile]
        private struct UpdateTextureColors : IJobProcessComponentData<Position2D, CellStyle>
        {
            public int Width;
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<byte> ColorsArray;

            public void Execute([ReadOnly] ref Position2D pos, [ReadOnly] ref CellStyle style)
            {
                int arrayIndex = Width * pos.Y + pos.X;
                ColorsArray[3 * arrayIndex] = style.R;
                ColorsArray[3 * arrayIndex + 1] = style.G;
                ColorsArray[3 * arrayIndex + 2] = style.B;
            }
        }
    }
}
