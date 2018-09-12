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
        private int _fps=0;

        private void Update()
        {
            _deltaTime += Time.deltaTime;
            _updatesCount++;
            if (_deltaTime > UpdateFpsInterval)
            {
                _fps = (int) (_updatesCount/_deltaTime);
                _deltaTime = 0;
                _updatesCount = 0;
            }
        }

        public void OnGUI()
        {
            if(Event.current.type != EventType.Repaint) return;
            var texture = RecieveTexture();
            if (texture != null)
            {
                GL.PushMatrix();
                GL.LoadPixelMatrix(0, Screen.width, Screen.height, 0);
                Graphics.DrawTexture(new Rect(0, 0, texture.width, texture.height), texture);
                GL.PopMatrix();
            }

            GUI.Label(new Rect(
           Screen.width * 0.05f,
           Screen.height * 0.01f,
           Screen.width * 0.17f,
           Screen.height * 0.05f), $"{_fps} fps");
        }
    }
}
