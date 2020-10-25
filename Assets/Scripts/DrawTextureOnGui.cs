using System;
using alexnown.GameOfLife;
using Unity.Entities;
using UnityEngine;

namespace alexnown.EcsLife
{
    public class DrawTextureOnGui : MonoBehaviour
    {
        public float UpdateFpsInterval = 0.5f;


        private float _deltaTime = 0.0f;
        private int _updatesCount;
        private int _fps = 0;
        private int _lastUpdatedFrame;
        private GUIStyle statsStyle;
        private UpdateRendererTextureSystem _rendererSystem;

        private void Update()
        {
            if (statsStyle == null) statsStyle = new GUIStyle { fontSize = 40, normal = new GUIStyleState { textColor = Color.white } };
            _deltaTime += Time.deltaTime;
            _updatesCount++;
            if (_deltaTime > UpdateFpsInterval)
            {
                _fps = (int)(_updatesCount / _deltaTime);
                _deltaTime = 0;
                _updatesCount = 0;
            }
        }

        private void Start()
        {
            //Application.targetFrameRate = 60;
            _rendererSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<UpdateRendererTextureSystem>();
        }


        public void OnGUI()
        {
            if (_lastUpdatedFrame == Time.frameCount) return;
            if (Event.current.type != EventType.Repaint) return;
            _lastUpdatedFrame = Time.frameCount;
            var texture = _rendererSystem.CreatedTexture;
            int minSide = Math.Min(Screen.width, Screen.height);
            statsStyle.fontSize = minSide / 50;
            int totalCells = -1;
            if (texture != null)
            {
                totalCells = texture.width * texture.height;
                int minTextureSide = Math.Min(texture.width, texture.height);
                float screenMultiplier = (float)minTextureSide / minSide;
                int texturePosX = 0;
                int texturePosY = 0;
                if (Screen.width > Screen.height)
                {
                    texturePosX = (int)((Screen.width * screenMultiplier - texture.width) / 2);
                }
                else texturePosY = (int)((Screen.height * screenMultiplier - texture.height) / 2);
                GL.PushMatrix();
                GL.LoadPixelMatrix(0, Screen.width * screenMultiplier, Screen.height * screenMultiplier, 0);
                Graphics.DrawTexture(new Rect(texturePosX, texturePosY, texture.width, texture.height), texture);
                GL.PopMatrix();
            }
            //var stats = $"Fps: {_fps}\nCells:{LongToString(Bootstrap.TotalCells)}\nAge: {LongToString(UpdateCellWorldsSystem.TotalUpdates)}\n" +
            //            $"Time : {(int)UpdateCellWorldsSystem.TotalTime}\n" +
            //            $"Speed: {(int)UpdateCellWorldsSystem.UpdatesSpeed}/s";
            float speed = -1;
            long simulationTicks = SimulationStatistics.SimulationTotalTicks / SimulationStatistics.SimulationsCount;
            long updateTextureTicks = SimulationStatistics.UpdateTextureTotalTicks / SimulationStatistics.UpdateTextureCounts;
            if (Time.time > 0) speed = Time.frameCount / Time.time;
            var stats = $"Cells: {LongToString(totalCells)}  Cores: {SystemInfo.processorCount}\nFps: {_fps}    Age: {Time.frameCount}\nCells update : {LongToString(simulationTicks)} ticks\n" +
                       $"Texture update : {LongToString(updateTextureTicks)} ticks\nSimulation speed: {speed.ToString("F1")}/s";
            var rect = new Rect(
                Screen.width*0.05f,
                Screen.height*0.02f,
                Screen.width*0.2f,
                Screen.height*0.1f);
            GUI.Label(rect, stats, statsStyle);
        }

        public string LongToString(long l)
        {
            if (l > 1000000)
            {
                return $"{(l / 1000000f):F1}M";
            }
            else if (l > 10000)
            {
                return $"{l / 1000}k";
            }
            else if (l > 1000)
            {
                return $"{(l / 1000f):F1}k";
            }
            else return l.ToString();
        }
    }
}
