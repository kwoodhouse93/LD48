using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor
{
    bool autogen = false;

    public override void OnInspectorGUI()
    {
        TerrainGenerator terrainGenerator = (TerrainGenerator)target;

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Generate"))
        {
            terrainGenerator.Generate();
        }

        autogen = EditorGUILayout.ToggleLeft("Auto-generate", autogen);
        if (autogen)
        {
            terrainGenerator.Generate();
        }

        EditorGUILayout.EndHorizontal();

        DrawDefaultInspector();
    }
}
