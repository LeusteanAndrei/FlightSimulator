using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OrbitScript))]
public class OrbitInspectorButton : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        OrbitScript script = (OrbitScript)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Recompute orbit"))
        {
            script.Planet().SetupOrbitVelocity();
        }
    }
}