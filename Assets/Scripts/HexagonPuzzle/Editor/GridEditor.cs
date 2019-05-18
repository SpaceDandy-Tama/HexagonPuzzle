#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HexagonPuzzle.Editors
{
    [CustomEditor(typeof(Grid))]
    public class GridEditor : Editor
    {
        Grid grid;

        public override void OnInspectorGUI()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                base.OnInspectorGUI();

                if (check.changed && false) //Disabled this because it's annoying
                    grid.GenerateGrid();
            }

            if (!Application.isPlaying && GUILayout.Button("Generate Grid"))
                grid.GenerateGrid();

            if (!Application.isPlaying && GUILayout.Button("Remove Grid"))
                grid.RemoveGrid();
        }

        private void OnEnable()
        {
            grid = (Grid)target;
        }
    }
}
#endif