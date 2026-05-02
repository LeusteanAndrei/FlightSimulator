using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlanetScript))]
public class PlanetEditor : Editor
{
    PlanetScript planet;
    public override void OnInspectorGUI()
    {
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
            if (check.changed)
            {
                planet.GeneratePlanet();
            }
        }

        if(GUILayout.Button("Generate Planet"))
        {
            planet.GeneratePlanet();
        }
        DrawSettingsEditor(planet.shapeSettings, planet.OnShapeSettingsUpdated, ref planet.shapeSettingsFoldout);
        DrawSettingsEditor(planet.colourSettings, planet.OnColourSettingsUpdated, ref planet.colourSettingsFoldout);
    }
    void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated, ref bool foldout)
    {
        if (settings != null)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
                if (foldout)
                {
                    Editor editor = CreateEditor(settings);
                    editor.OnInspectorGUI();

                    if (check.changed)
                    {
                        if (onSettingsUpdated != null)
                        {
                            onSettingsUpdated();
                        }
                    }

                }

            }
        }
        
    }
    private void OnEnable()
    {
        planet = (PlanetScript)target;
    }

}
