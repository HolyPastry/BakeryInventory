using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Bakery
{
    [CreateAssetMenu(fileName = "New Grid Info", menuName = "Bakery/Inventory/Grid Info")]
    public class GridInfo : ScriptableObject
    {
        public Sprite Sprite;
        public List<InventoryFilter> Filters;
        public List<Vector2Int> Coordinates = new();
        public Vector2Int MaxSize;
        public int StackCapacity;
        public bool Lock;

        public Vector2Int Size
        {
            get
            {
                if (Coordinates.Count == 0)
                    return Vector2Int.zero;

                var minX = Coordinates.Min(c => c.x);
                var maxX = Coordinates.Max(c => c.x);
                var minY = Coordinates.Min(c => c.y);
                var maxY = Coordinates.Max(c => c.y);

                return new Vector2Int(maxX - minX + 1, maxY - minY + 1);
            }
        }



        internal bool Compatible(GridInfo gridInfo)
        {
            if (Filters.Count == 0 || gridInfo.Filters.Count == 0)
                return true;
            return Filters.Any(f => gridInfo.Filters.Contains(f));
        }
    }

#if UNITY_EDITOR

    [System.Serializable]
    public class Wrapper<T>
    {
        public T[] values;
    }
    [CustomEditor(typeof(GridInfo))]
    public class GridInfoEditor : Editor
    {
        private SerializedProperty _serialGrid;
        // private SerializedProperty _serialSize;
        // private SerializedProperty _serialStackCapacity;
        // private SerializedProperty _serialFilter;
        // private SerializedProperty _serialSprite;

        void OnEnable()
        {
            _serialGrid = serializedObject.FindProperty("Coordinates");
            // _serialSize = serializedObject.FindProperty("MaxSize");
            // _serialStackCapacity = serializedObject.FindProperty("StackCapacity");
            //_serialFilter = serializedObject.FindProperty("Filter");
            // _serialSprite = serializedObject.FindProperty("Sprite");


        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            GridInfo script = (GridInfo)target;

            // GUILayout.Label(script.name);

            GUILayout.Space(10);
            //EditorGUILayout.PropertyField(_serialSize, new GUIContent("Grid Size"));
            // GUILayout.Space(10);
            // EditorGUILayout.PropertyField(_serialStackCapacity, new GUIContent("Stack Capacity"));
            // GUILayout.Space(10);
            // EditorGUILayout.PropertyField(_serialFilter, new GUIContent("Filter"));
            //GUILayout.Space(10);
            //EditorGUILayout.PropertyField(_serialSprite, new GUIContent("Sprite"));
            // GUILayout.Space(10);
            DrawGrid(script);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGrid(GridInfo script)
        {
            try
            {
                var style = new GUIStyle(GUI.skin.box)
                {
                    stretchWidth = true,
                    alignment = TextAnchor.MiddleCenter
                };
                script.Coordinates.RemoveAll(pos => pos.x >= script.MaxSize.x || pos.y >= script.MaxSize.y);
                GUILayout.BeginVertical(style);
                for (int i = 0; i < script.MaxSize.y; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    for (int j = 0; j < script.MaxSize.x; j++)
                    {
                        bool contains = script.Coordinates.Contains(new Vector2Int(j, i));
                        if (i == 0 && j == 0)
                        {
                            GUI.backgroundColor = Color.yellow;
                            GUILayout.Button("X", GUILayout.MaxWidth(20));
                            if (!contains)
                                AddCoordinates(script, i, j);
                            continue;

                        }
                        GUI.backgroundColor = contains ? Color.green : Color.red;
                        var value = contains ? 1 : 0;
                        if (GUILayout.Button(value.ToString(), GUILayout.MaxWidth(20)))
                        {

                            if (contains)
                            {
                                var index = script.Coordinates.FindIndex(pos => pos.x == j && pos.y == i);
                                if (index >= 0)
                                    _serialGrid.DeleteArrayElementAtIndex(index);
                            }
                            else
                            {
                                AddCoordinates(script, i, j);
                            }
                        }
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();

            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        private void AddCoordinates(GridInfo script, int i, int j)
        {
            var index = script.Coordinates.FindIndex(pos => pos.x == j && pos.y == i);
            if (index < 0)
            {
                if (_serialGrid.arraySize > 0)
                    _serialGrid.InsertArrayElementAtIndex(_serialGrid.arraySize - 1);
                else
                    _serialGrid.InsertArrayElementAtIndex(_serialGrid.arraySize);
                var vector = _serialGrid.GetArrayElementAtIndex(_serialGrid.arraySize - 1);

                vector.vector2IntValue = new Vector2Int(j, i);
            }
        }
    }
#endif
}