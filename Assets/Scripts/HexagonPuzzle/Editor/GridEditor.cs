#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

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
            {
                grid.GenerateGrid();
                //Scene is automaticly saved because sometimes editor doesn't realize scene is modified, in which case you cannot manually save it.
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }

            if (!Application.isPlaying && GUILayout.Button("Remove Grid"))
            {
                grid.RemoveGrid();
                //Scene is automaticly saved because sometimes editor doesn't realize scene is modified, in which case you cannot manually save it.
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }
        }

        private void OnEnable()
        {
            grid = (Grid)target;
        }
    }
}
#endif