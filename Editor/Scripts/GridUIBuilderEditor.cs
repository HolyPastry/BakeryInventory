using UnityEditor;
using UnityEngine;

namespace Bakery
{
    [CustomEditor(typeof(GridUIBuilder))]
    public class GridUIBuilderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GridUIBuilder script = (GridUIBuilder)target;

            if (GUILayout.Button("Reset Grid"))
            {
                script.UpdateGrid();
            }
        }
    }
}