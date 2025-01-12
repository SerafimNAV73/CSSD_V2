using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShadowCaster2DTileMap))]
public class ShadowCastersGeneratorEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ShadowCaster2DTileMap generator = (ShadowCaster2DTileMap)target;

        if (GUILayout.Button("Generate"))
        {
            generator.Generate();
        }

        if (GUILayout.Button("Destroy All Children"))
        {
            generator.DestroyAllChildren();
        }
    }

}