using System.Linq;
using alexnown.EcsLife.Systems;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace alexnown.EcsLife
{
    public static class Bootstrap
    {


        public static BootstrapSettings Settings { get; private set; }
        public static int Width;
        public static int Height;
        public static int TotalCells;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Initialize()
        {
            Settings = Resources.Load<BootstrapSettings>("BootstrapSettings");
            Width = (int)(Screen.width * Settings.ResolutionMultiplier);
            Height = (int)(Screen.height * Settings.ResolutionMultiplier);
            TotalCells = Width * Height;

            World.DisposeAllWorlds();
            InitializeInputWorld();
            var cellsWorld = InitializeCellsWorld();
            var em = World.Active.GetOrCreateManager<EntityManager>();
            em.AddSharedComponentData(em.CreateEntity(), cellsWorld);

            if (Settings.InitializeManualUpdate)
            {
                var autoUpdate = World.Active.GetOrCreateManager<UpdateCellWorldsSystem>();
                autoUpdate.MaxTimeLimitSec = 1f / Settings.PreferedFps;
                autoUpdate.MaxUpdatesForFrame = Settings.MaxWorldsUpdatesLimit;
                ScriptBehaviourUpdateOrder.UpdatePlayerLoop(World.Active);
            }
            else ScriptBehaviourUpdateOrder.UpdatePlayerLoop(World.AllWorlds.ToArray());
        }

        private static void InitializeInputWorld()
        {
            var inputWorld = new World("Input");
            World.Active = inputWorld;
            inputWorld.CreateManager<CreateSpraySourceFromInput>();
            var drawSystem = inputWorld.GetOrCreateManager<DrawCellsTextureSystem>();
            drawSystem.InitializeTexture(Width, Height);

            var drawer = new GameObject("DrawerOnGUI").AddComponent<DrawTextureOnGui>();
            drawer.RecieveTexture = () => drawSystem.GeneratedTexture;
        }

        private static CellsWorld InitializeCellsWorld()
        {
            var world = new World("CellWorld");

            world.CreateManager<ApplyFutureStatesSystem>();
            world.CreateManager<UpdateCellsLifeRulesSystem>();
            world.CreateManager<DisposeCellsArrayOnDestroyWorld>();
            var paintTexture = world.CreateManager<UpdateTextureColorsJobSystem>();
            world.CreateManager<ApplySprayPointsToCells>();
            var em = world.GetOrCreateManager<EntityManager>();

            var colorsArray = Settings.CellColors;
            var colors = new NativeArray<Color32>(colorsArray.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < colors.Length; i++)
            {
                var col = colorsArray[i];
                colors[i] = new Color32((byte)(col.r * Settings.ColorsIntansity),
                    (byte)(col.g * Settings.ColorsIntansity),
                    (byte)(col.b * Settings.ColorsIntansity), 0);
            }
            paintTexture.CellColorsByState = colors;

            var futureCellsState = new NativeArray<CellState>(TotalCells, Allocator.Persistent);
            var activeCellState = new NativeArray<CellState>(TotalCells, Allocator.Persistent);

            var activeCellsDb = em.CreateEntity(ComponentType.Create<CellsDb>(), ComponentType.Create<CellsDbState>());
            var cellsDb = new CellsDb
            {
                UpdateRules = Settings.UpdateRules,
                Width = Width,
                Height = Height,
                CellsState0 = activeCellState,
                CellsState1 = futureCellsState
            };
            em.SetSharedComponentData(activeCellsDb, cellsDb);

            return new CellsWorld
            {
                Width = Width,
                Height = Height,
                World = world
            };
        }

    }
}
