using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace alexnown.EcsLife
{
    [DisableAutoCreation] [UpdateAfter(typeof(UpdateCellWorldsSystem))] 
    public class DrawCellsTextureSystem : ComponentSystem
    {
        public Texture2D GeneratedTexture { get; private set; }
        public int SelectedWorldIndex = 0;

        private int _textureWidth;
        private int _textureHeight;
        private NativeArray<byte> _colors;
        private ComponentGroup _worlds;

        protected override void OnCreateManager()
        {
            _worlds = GetComponentGroup(ComponentType.ReadOnly<CellsWorld>());
        }

        protected override void OnDestroyManager()
        {
            base.OnDestroyManager();
            if(_colors.IsCreated) _colors.Dispose();
        }

        public void InitializeTexture(int width, int height)
        {
            if (_colors.IsCreated) _colors.Dispose();
            _textureWidth = width;
            _textureHeight = height;
            GeneratedTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
            _colors = new NativeArray<byte>(3 * width * height, Allocator.Persistent);
        }

        protected override void OnStopRunning()
        {
            if (_colors.IsCreated) _colors.Dispose();
        }

        protected override void OnUpdate()
        {
            if(_worlds.CalculateLength() == 0) return;
            var worldData = _worlds.GetSharedComponentDataArray<CellsWorld>()[0];
            var drawSystem = worldData.World.GetOrCreateManager<UpdateTextureColorsJobSystem>();
            drawSystem.Width = worldData.Width;
            drawSystem.TextureColors = _colors;
            drawSystem.Enabled = true;
            drawSystem.Update();
            drawSystem.Enabled = false;
            GeneratedTexture.LoadRawTextureData(_colors);
            GeneratedTexture.Apply();
        }


    }
}
