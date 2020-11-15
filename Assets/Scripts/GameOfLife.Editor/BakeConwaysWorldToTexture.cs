using System;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using static GameOfLife.ConwaysWorldUtils;

namespace GameOfLife
{
    public class BakeConwaysWorldToTexture : EditorWindow
    {
        private const string PathKey = "BakeTexturePath";
        [MenuItem("Window/GameOfLife/BakeToTexture")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(BakeConwaysWorldToTexture), false, "BakeTexture");
        }

        private string _gollyString;
        private string _path;
        private int2 _worldSize = new int2(1920, 1080);
        private Vector2 ScrollPos;

        void OnGUI()
        {
            _worldSize.x = EditorGUILayout.IntField("WorldWidth", _worldSize.x);
            _worldSize.y = EditorGUILayout.IntField("WorldHeight", _worldSize.y);
            GUILayout.Label("Golly string:", EditorStyles.boldLabel);

            ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos, GUILayout.Height(300));
            _gollyString = EditorGUILayout.TextArea(_gollyString, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
            GUILayout.Space(30);
            EditorGUILayout.BeginHorizontal();
            bool pathCorrect = !string.IsNullOrEmpty(_path) && Directory.Exists(_path);
            if (!pathCorrect) _path = GetDefaultSavingPath();
            EditorGUILayout.LabelField($"Saving directory: {_path}");
            if (GUILayout.Button("Select"))
            {
                _path = EditorUtility.OpenFolderPanel("Select folder:", _path, "");
                PlayerPrefs.SetString(PathKey, _path);
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            if (GUILayout.Button("Generate", GUILayout.Width(100)))
            {
                var sizeInDemandedAreas = (int2)math.ceil(_worldSize / new float2(16, 3));
                _worldSize = sizeInDemandedAreas * new int2(16, 3);
                var texture = GenerateTextureByGollyString(_worldSize, _gollyString);
                byte[] _bytes = texture.EncodeToPNG();
                var fullPath = $"{_path}/Conways_{DateTime.Now.ToString("HH_mm_ss")}.png";
                System.IO.File.WriteAllBytes(fullPath, _bytes);
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                var shortPath = fullPath.Substring(fullPath.IndexOf("Assets/"));
                TextureImporter importer = AssetImporter.GetAtPath(shortPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Default;
                    importer.isReadable = true;
                    importer.mipmapEnabled = false;
                    var settings = importer.GetDefaultPlatformTextureSettings();
                    settings.format = TextureImporterFormat.RGBA32;
                    settings.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SetPlatformTextureSettings(settings);
                    AssetDatabase.ImportAsset(shortPath, ImportAssetOptions.ForceUpdate);
                }
            }
        }

        private Texture2D GenerateTextureByGollyString(int2 worldSize, string golly)
        {
            var index = golly.IndexOf(',');
            var gollyWidth = golly.Substring(4, index - 4);
            index += 6;
            var gollyHeight = golly.Substring(index, golly.IndexOf(',', index) - index);
            var gollySize = new int2(int.Parse(gollyWidth), int.Parse(gollyHeight));
            if (gollySize.x > worldSize.x || gollySize.y > worldSize.y)
                throw new ArgumentException($"World size={worldSize} can't be smaller than golly size={gollySize}");
            index = golly.IndexOf("S23", index) + 3;
            var texture = new Texture2D(worldSize.x / 4, worldSize.y / 3, TextureFormat.RGBA32, false);
            var rawData = texture.GetRawTextureData<int4>();
            for (int i = 0; i < rawData.Length; i++) rawData[i] = 0;
            int2 cellPos = (worldSize - gollySize) / 2;
            cellPos.y = worldSize.y - cellPos.y;
            int startPoxX = cellPos.x;
            int prevInt = 0;
            while (ParseGollyString(texture, golly, index, startPoxX, ref cellPos, ref prevInt)) { index++; }
            new SetHorizontalSidesInAreasJob
            {
                Width = texture.width / 4,
                CellStates = rawData
            }.Run(texture.height);
            new SetVerticalSidesInAreasJob
            {
                Width = texture.width / 4,
                CellStates = rawData
            }.Run(texture.width / 4);
            texture.Apply();
            return texture;
        }

        private bool ParseGollyString(Texture2D texture, string golly, int stringIndex, int worldStartX, ref int2 cellPos, ref int prevInt)
        {
            var nextChar = golly[stringIndex];
            if (char.IsDigit(nextChar))
            {
                prevInt = 10 * prevInt + nextChar - 48;
                return true;
            }
            else if (nextChar == 'o')
            {
                var cellsCount = math.max(1, prevInt);
                var areas = texture.GetRawTextureData<int>();
                while (cellsCount > 0)
                {
                    cellsCount--;
                    var areaPos = cellPos / new int2(4, 3);
                    var areaIndex = areaPos.y * texture.width + areaPos.x;
                    var bitMask = ConwaysWorldUtils.CellBitMask[2 - (cellPos.y % 3)][cellPos.x % 4];
                    areas[areaIndex] |= bitMask;
                    cellPos.x++;
                }
            }
            else if (nextChar == 'b')
            {
                cellPos.x += math.max(1, prevInt);
            }
            else if (nextChar == '$')
            {
                cellPos = new int2(worldStartX, cellPos.y - math.max(1, prevInt));
            }
            else if (nextChar == 10 || nextChar == 13) return true;
            else if (nextChar == '!') return false;
            else throw new NotImplementedException($"Unknown char={nextChar}({(int)nextChar}) index={stringIndex}");
            prevInt = 0;
            return true;
        }

        private string GetDefaultSavingPath()
        {
            var path = PlayerPrefs.GetString(PathKey);
            bool pathCorrect = !string.IsNullOrEmpty(path) && Directory.Exists(path);
            if (!pathCorrect) path = Application.dataPath;
            return path;
        }
    }
}
