using System;
using UnityEngine;

namespace alexnown.GameOfLife
{
    public class DrawTextureOnGui : MonoBehaviour
    {
        public Texture2D Texture = null;

        private void Awake()
        {
            enabled = false;
        }

        public void OnGUI()
        {
            if (Event.current.type != EventType.Repaint) return;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture, ScaleMode.ScaleToFit);
        }
    }
}
