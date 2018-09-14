using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace alexnown.EcsLife
{
    [AlwaysUpdateSystem] [DisableAutoCreation]
    public class DrawCellsTextureSystem : ComponentSystem
    {
        public Texture2D GeneratedTexture { get; private set; }

        private int _textureWidth;
        private int _textureHeight;
        
        private NativeArray<byte> _colors;

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
            if(Bootstrap.CellsWorld==null) return;
            var generateTextureSystem = Bootstrap.CellsWorld.GetExistingManager<UpdateTextureColorsJobSystem>();
            if (generateTextureSystem == null) return;

            bool waitWhilePrepareTexture = !generateTextureSystem.TexturePrepared;
            if(waitWhilePrepareTexture) return;
            GeneratedTexture.LoadRawTextureData(_colors);
            GeneratedTexture.Apply();
            generateTextureSystem.FillTargetArray(_colors, _textureWidth, _textureHeight);
        }


    }
}
