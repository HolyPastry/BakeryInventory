using System;
using UnityEditor;
using UnityEngine;

namespace Bakery
{
    [System.Serializable]
    public class Wrapper<T>
    {
        public T[] values;
    }
    [CustomEditor(typeof(GridInfo))]
    public class GridInfoEditor : Editor
    {
        private SerializedProperty _serialGrid;
        private SerializedProperty _serialSize;

        void OnEnable()
        {
            _serialGrid = serializedObject.FindProperty("Coordinates");
            _serialSize = serializedObject.FindProperty("MaxSize");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            GridInfo script = (GridInfo)target;

            GUILayout.Label(script.name);

            GUILayout.Space(10);
            EditorGUILayout.PropertyField(_serialSize, new GUIContent("Grid Size"));
            GUILayout.Space(10);
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
}