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
            inputWorld.GetOrCreateManager<EntityManager>();
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
            CellsWorld.GetOrCreateManager<UpdateCellsLifeRulesSystem>();
            CellsWorld.GetOrCreateManager<DisposeCellsArrayOnDestroyWorld>();
            var em = CellsWorld.GetOrCreateManager<EntityManager>();
            CellsWorld.GetOrCreateManager<UpdateTextureColorsJobSystem>();
            var applySprays = CellsWorld.GetOrCreateManager<ApplySprayPointsToCells>();
            applySprays.Width = Width;
            applySprays.Height = Height;
            CellsWorld.GetOrCreateManager<EndCellsUpdatesBarrier>();

            var cells = new NativeArray<Entity>(Width * Height, Allocator.Persistent);
            var cellArchetype = em.CreateArchetype(ComponentType.Create<CellState>(), ComponentType.Create<CellStyle>(),
                ComponentType.Create<Position2D>());

            em.CreateEntity(cellArchetype, cells);
            for (int i = 0; i < cells.Length; i++)
            {
                var x = i % Width;
                var y = i / Width;
                em.SetComponentData(cells[i], new Position2D { X = x, Y = y });
            }

            var cellsDb = em.CreateEntity(ComponentType.Create<CellsDb>());
            em.SetSharedComponentData(cellsDb, new CellsDb { Width = Width, Height = Height, Cells = cells });
        }

    }
}
