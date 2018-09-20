using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace alexnown.EcsLife
{
    public static class Bootstrap
    {
        public static World CellsWorld { get; private set; }
        public static int Width { get; private set; }
        public static int Height { get; private set; }

        public static BootstrapSettings Settings { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Initialize()
        {
            Settings = Resources.Load<BootstrapSettings>("BootstrapSettings");

            World.DisposeAllWorlds();
            InitializeInputWorld();
            InitializeCellsWorld();
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop(World.AllWorlds.ToArray());
        }

        private static void InitializeInputWorld()
        {
            var inputWorld = new World("Input");
            World.Active = inputWorld;
            //inputWorld.GetOrCreateManager<EntityManager>();
            inputWorld.GetOrCreateManager<CreateSpraySourceFromInput>();
            var drawSystem = inputWorld.GetOrCreateManager<DrawCellsTextureSystem>();
            drawSystem.InitializeTexture(Screen.width, Screen.height);

            var drawer = new GameObject("DrawerOnGUI").AddComponent<DrawTextureOnGui>();
            drawer.RecieveTexture = () => drawSystem.GeneratedTexture;
        }

        private static void InitializeCellsWorld()
        {
            Width = Screen.width * Settings.ResolutionMultiplier;
            Height = Screen.height * Settings.ResolutionMultiplier;

            CellsWorld = new World("CellsWorld");
            CellsWorld.CreateManager<EndCellsUpdatesBarrier>();
            CellsWorld.CreateManager<ApplyFutureStatesSystem>();
            CellsWorld.CreateManager<UpdateCellsLifeRulesSystem>();
            CellsWorld.CreateManager<DisposeCellsArrayOnDestroyWorld>();
            var paintTexture = CellsWorld.CreateManager<UpdateTextureColorsJobSystem>();
            CellsWorld.CreateManager<ApplySprayPointsToCells>();
            var em = CellsWorld.GetOrCreateManager<EntityManager>();


            var colors = new NativeArray<Color32>(3, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            colors[0] = new Color32();
            colors[1] = new Color32(0, Settings.GreenColor, 0, 0);
            colors[2] = new Color32(0, (byte) (Settings.GreenColor / 2), 0, 0);
            paintTexture.CellColorsByState = colors;
            /*
            var cells = new NativeArray<Entity>(Width * Height, Allocator.Persistent);
            var cellArchetype = em.CreateArchetype(ComponentType.Create<CellState>(), ComponentType.Create<CellState>(),
                ComponentType.Create<Position2D>());

            em.CreateEntity(cellArchetype, cells);
            for (int i = 0; i < cells.Length; i++)
            {
                var x = i % Width;
                var y = i / Width;
                em.SetComponentData(cells[i], new Position2D { X = x, Y = y });
            } */

            var futureCellsState = new NativeArray<CellState>(Width * Height, Allocator.Persistent);
            var activeCellState = new NativeArray<CellState>(futureCellsState.Length, Allocator.Persistent);

            var activeCellsDb = em.CreateEntity(ComponentType.Create<CellsDb>(), ComponentType.Create<CellsDbState>());
            em.SetSharedComponentData(activeCellsDb, new CellsDb { Width = Width, Height = Height, CellsState0 = activeCellState, CellsState1 = futureCellsState });
        }

    }
}
