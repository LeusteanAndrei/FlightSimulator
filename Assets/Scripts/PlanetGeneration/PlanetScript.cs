using UnityEngine;

public class PlanetScript : MonoBehaviour
{

    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;
    [Range(2, 256)]
    public int resolution = 10;
    public enum FaceRenderMask { All, Top, Bottom, Left, Right, Front, Back };
    public FaceRenderMask faceRenderMask;
    public ShapeSettings shapeSettings;
    public ColourSettings colourSettings;
    public Material defaultMaterial;
    [HideInInspector]
    public bool shapeSettingsFoldout;
    [HideInInspector]
    public bool colourSettingsFoldout;
    ShapeGenerator shapeGenerator = new ShapeGenerator();
    ColourGenerator colourGenerator = new ColourGenerator();
    public bool autoUpdate = true;

    private bool IsPrefabAsset()
    {
#if UNITY_EDITOR
        return UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject);
#else
        return false;
#endif
    }

    private void OnValidate()
    {
        if (!Application.isPlaying && IsPrefabAsset())
        {
            return;
        }

        //Init();
        //GenerateMesh();
        if (shapeSettings == null || colourSettings == null || defaultMaterial == null)
            return;

        GeneratePlanet();
    }
    void Init()
    {
        if (!Application.isPlaying && IsPrefabAsset())
        {
            return;
        }

        //shapeGenerator = new ShapeGenerator(shapeSettings);
        //colourGenerator = new ColourGenerator(colourSettings);
        shapeGenerator.UpdateSettings(shapeSettings);
        colourGenerator.UpdateSettings(colourSettings);
        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
        }
        
        terrainFaces = new TerrainFace[6];
        Vector3[] directions = {Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back};
        for(int i = 0; i < 6; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.SetParent(transform, false);
                meshObj.transform.localPosition = Vector3.zero;
                meshObj.transform.localRotation = Quaternion.identity;
                meshObj.transform.localScale = Vector3.one;

                meshObj.AddComponent<MeshRenderer>();
                    //.sharedMaterial = defaultMaterial;
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();

                MeshCollider meshCollider = meshObj.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = meshFilters[i].sharedMesh;
                meshCollider.convex = true;
            }

            Mesh runtimeFaceMesh = new Mesh();
            runtimeFaceMesh.name = $"PlanetFace_{i}";
            meshFilters[i].sharedMesh = runtimeFaceMesh;

            MeshCollider existingCollider = meshFilters[i].GetComponent<MeshCollider>();
            if (existingCollider == null)
            {
                existingCollider = meshFilters[i].gameObject.AddComponent<MeshCollider>();
            }

            Transform faceTransform = meshFilters[i].transform;
            faceTransform.SetParent(transform, false);
            faceTransform.localPosition = Vector3.zero;
            faceTransform.localRotation = Quaternion.identity;
            faceTransform.localScale = Vector3.one;

            existingCollider.convex = true;
            existingCollider.sharedMesh = runtimeFaceMesh;

            meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = colourSettings.planetMaterial;
            terrainFaces[i]=new TerrainFace(shapeGenerator, runtimeFaceMesh, resolution, directions[i]);
            bool renderFace = faceRenderMask == FaceRenderMask.All || (int)faceRenderMask - 1 == i;
            meshFilters[i].gameObject.SetActive(renderFace);
        }


    }
    void GenerateColours()
    {
        //foreach (MeshFilter m in meshFilters)
        //{
        //    //m.GetComponent<MeshRenderer>().sharedMaterial.color = colourSettings.planetColour;
        //    m.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_BaseColor", colourSettings.planetColour);
        //}
        colourGenerator.UpdateColours();
        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].UpdateUVs(colourGenerator);
            }
        }
    }
    public void GeneratePlanet()
    {
        if (!Application.isPlaying && IsPrefabAsset())
        {
            return;
        }

        Init();
        GenerateMesh();
        GenerateColours();
    }
    void GenerateMesh()
    {
        //foreach (TerrainFace face in terrainFaces)
        //{
        //    face.ConstructMesh();
        //}
        for(int i = 0; i < 6; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].ConstructMesh();

                MeshCollider meshCollider = meshFilters[i].GetComponent<MeshCollider>();
                if (meshCollider != null)
                {
                    // Reassign to force Unity to rebuild collider from the latest mesh vertices.
                    meshCollider.convex = true;
                    meshCollider.sharedMesh = null;
                    meshCollider.sharedMesh = meshFilters[i].sharedMesh;
                }
            }
        }

        colourGenerator.UpdateElevation(shapeGenerator.elevationMinMax);
    }
    public void OnColourSettingsUpdated()
    {
        if (!Application.isPlaying && IsPrefabAsset())
        {
            return;
        }

        if (autoUpdate)
        {
        Init();
        GenerateColours();
        }

    }
    public void OnShapeSettingsUpdated()
    {
        if (!Application.isPlaying && IsPrefabAsset())
        {
            return;
        }

        if (autoUpdate)
        {
        Init();
        GenerateMesh();
        }

    }

}
