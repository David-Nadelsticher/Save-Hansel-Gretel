using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class TestGeneralEditorTool : EditorWindow
    {
        // Add menu item to open the window
        [MenuItem("BossProjectTools/Test General Editor Tool")]
        public static void ShowWindow()
        {
            //Print a message to the console
            Debug.Log("Opening Test General Editor Tool window...");
            // Create the window
            TestGeneralEditorTool window = GetWindow<TestGeneralEditorTool>("Test General Editor Tool");
        }
        
        // OnGUI method to draw the window
        private void OnGUI()
        {
            CreateTable("Test General Editor Tool", 3, 3);
            EditorGUILayout.LabelField("Welcome to the Test General Editor Tool", EditorStyles.largeLabel);
            
            EditorGUIUtility.labelWidth = 100;
            
            /*GUILayout.Label("Test General Editor Tool", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Click Me!"))
            {
                Debug.Log("Button clicked!");
            }

            GUILayout.Space(10);
            GUILayout.Label("This is a test editor tool window.", EditorStyles.wordWrappedLabel);*/
        }

        private void CreateTable(string testGeneralEditorTool, int row, int col)
        {
            // Create a table with the specified number of rows and columns
            GUILayout.BeginVertical("box");
            for (int i = 0; i < row; i++)
            {
                GUILayout.BeginHorizontal();
                for (int j = 0; j < col; j++)
                {
                    // Create a button in each cell
                    if (GUILayout.Button($"Cell {i + 1}, {j + 1}"))
                    {
                        Debug.Log($"Button clicked in Cell {i + 1}, {j + 1}");
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
    }
}