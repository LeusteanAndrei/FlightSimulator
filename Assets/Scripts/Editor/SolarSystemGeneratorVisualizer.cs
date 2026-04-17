using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SolarSystemGenerator))]
public class SolarSystemGeneratorButton : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SolarSystemGenerator script = (SolarSystemGenerator)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Spawn Planets"))
        {
            script.SpawnPlanets();
            script.ApplyScaling();
        }
    }
}