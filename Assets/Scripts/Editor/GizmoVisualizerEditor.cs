using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GizmoVisualizer))]
public class GizmoVisualizerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GizmoVisualizer script = (GizmoVisualizer)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Recompute simulation"))
        {
            script.RecomputeSimulation();
        }
    }
}