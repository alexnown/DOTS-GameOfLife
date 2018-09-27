using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace alexnown.EcsLife
{
    [DisableAutoCreation]
    public class DrawCellsTextureSystem : ComponentSystem
    {
        public Texture2D GeneratedTexture { get; private set; }
        public int SelectedWorldIndex = 0;

        private int _textureWidth;
        private int _textureHeight;
        private NativeArray<byte> _colors;
        private ComponentGroup _worlds;
        private bool _needCreateDrawRequest = true;

        protected override void OnCreateManager()
        {
            _worlds = GetComponentGroup(ComponentType.Create<CellsWorld>());
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
            GeneratedTexture.LoadRawTextureData(_colors);
            GeneratedTexture.Apply();
            if (_needCreateDrawRequest)
            {
                var worlds = _worlds.GetSharedComponentDataArray<CellsWorld>();
                var targetWorld = worlds[SelectedWorldIndex];
                var em = targetWorld.World.GetOrCreateManager<EntityManager>();
                var request = new DrawStateRequest { Width = _textureWidth, Height = _textureHeight, Colors = _colors };
                em.AddSharedComponentData(em.CreateEntity(), request);
                _needCreateDrawRequest = false;
            }
            
            //todo: put to cell world entity-request with colors array for drawing 
            /*
            if(Bootstrap.CellsWorld==null) return;
            var generateTextureSystem = Bootstrap.CellsWorld.GetExistingManager<UpdateTextureColorsJobSystem>();
            if (generateTextureSystem == null) return;

            bool waitWhilePrepareTexture = !generateTextureSystem.TexturePrepared;
            if(waitWhilePrepareTexture) return;
            
            generateTextureSystem.FillTargetArray(_colors, _textureWidth, _textureHeight); */
        }


    }
}
