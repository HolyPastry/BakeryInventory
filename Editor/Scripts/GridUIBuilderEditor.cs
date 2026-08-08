using UnityEditor;
using UnityEngine;

namespace Bakery
{
    [CustomEditor(typeof(GridContainerUI))]
    public class GridUIBuilderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GridContainerUI script = (GridContainerUI)target;

            if (GUILayout.Button("Reset Grid"))
            {
                script.UpdateGrid();
            }
        }
    }
}