using System;
using UnityEngine;

namespace alexnown.EcsLife
{
    public class DrawTextureOnGui : MonoBehaviour
    {
        public Func<Texture2D> RecieveTexture;
        public float UpdateFpsInterval = 0.5f;


        private float _deltaTime = 0.0f;
        private int _updatesCount;
        private int _fps = 0;
        private GUIStyle statsStyle;

        private void Update()
        {
            if (statsStyle == null) statsStyle = new GUIStyle { fontSize = 30, normal = new GUIStyleState { textColor = Color.white } };
            _deltaTime += Time.deltaTime;
            _updatesCount++;
            if (_deltaTime > UpdateFpsInterval)
            {
                _fps = (int)(_updatesCount / _deltaTime);
                _deltaTime = 0;
                _updatesCount = 0;
            }
        }

        public void OnGUI()
        {
            if (Event.current.type != EventType.Repaint) return;

            var texture = RecieveTexture();
            if (texture != null)
            {
                GL.PushMatrix();
                GL.LoadPixelMatrix(0, Screen.width, Screen.height, 0);
                Graphics.DrawTexture(new Rect(0, 0, texture.width, texture.height), texture);
                GL.PopMatrix();
            }
            var stats = $"Fps: {_fps}\nCells:{LongToString(Bootstrap.TotalCells)}\nAge: {LongToString(UpdateCellWorldsSystem.TotalUpdates)}\n" +
                        $"Time : {(int)UpdateCellWorldsSystem.TotalTime}\n" +
                        $"Speed: {(int)UpdateCellWorldsSystem.UpdatesSpeed}/s";
            GUI.Label(new Rect(
           Screen.width * 0.05f,
           Screen.height * 0.02f,
           Screen.width * 0.2f,
           Screen.height * 0.1f), stats, statsStyle);
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
