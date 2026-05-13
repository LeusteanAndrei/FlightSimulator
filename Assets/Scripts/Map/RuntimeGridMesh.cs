using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RuntimeGridMesh : MonoBehaviour
{
    public Camera targetCamera;

    [Header("Grid Size & Levels")]
    [Tooltip("The size of the smaller squares. Set this closer to your minimum zoom!")]
    public float minorGridSize = 500f;
    [Tooltip("How many minor squares fit inside a major square.")]
    public int minorLinesPerMajor = 5;

    [Header("Appearance")]
    [Tooltip("Height above 0 to draw the grid. Prevents Z-fighting with the ground.")]
    public float gridAltitude = 5f;
    [Tooltip("Thickness in pixels. This stays constant regardless of zoom.")]
    public float lineThicknessPixels = 2f;
    public Color majorLineColor = new Color(1f, 1f, 1f, 0.4f);
    public Color minorLineColor = new Color(1f, 1f, 1f, 0.15f);

    [Header("Zoom Fade (Minor Grid)")]
    [Tooltip("Zoom level where minor lines start to fade out.")]
    public float fadeStartZoom = 5000f;
    [Tooltip("Zoom level where minor lines disappear completely.")]
    public float fadeEndZoom = 15000f;

    private Mesh mesh;
    private List<Vector3> vertices = new List<Vector3>();
    private List<Color> colors = new List<Color>();
    private List<int> indices = new List<int>();

    private Vector3 lastCamPos;
    private float lastZoom;

    void Start()
    {
        mesh = new Mesh();
        mesh.name = "Dynamic Minimap Grid";

        // CRITICAL FIX: Upgrade mesh to 32-bit so it can handle more than 65,535 vertices when zoomed way out.
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        GetComponent<MeshFilter>().mesh = mesh;

        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        MeshRenderer rend = GetComponent<MeshRenderer>();
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rend.receiveShadows = false;
        rend.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        if (rend.sharedMaterial == null || rend.sharedMaterial.name == "Default-Material")
        {
            rend.material = new Material(Shader.Find("Sprites/Default"));
        }
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        Vector3 camPos = targetCamera.transform.position;
        float zoom = targetCamera.orthographicSize;

        // Rebuild if the camera has moved or zoomed
        if (Vector3.Distance(camPos, lastCamPos) > 10f || Mathf.Abs(zoom - lastZoom) > 10f)
        {
            RebuildGrid(camPos, zoom);
            lastCamPos = camPos;
            lastZoom = zoom;
        }
    }

    void RebuildGrid(Vector3 camPos, float zoom)
    {
        // 1. Clear the mesh and lists
        mesh.Clear();
        vertices.Clear();
        colors.Clear();
        indices.Clear();

        minorGridSize = Mathf.Max(10f, minorGridSize);

        // 2. Thickness and Bounds calculations
        float screenHeightPixels = targetCamera.pixelHeight > 0 ? targetCamera.pixelHeight : 1080f;
        float worldHeight = zoom * 2f;
        float thickness = (lineThicknessPixels / screenHeightPixels) * worldHeight;

        float worldWidth = worldHeight * targetCamera.aspect;

        // We add a 2x buffer so lines don't "pop" in at the very edge of the screen
        float buffer = minorGridSize * 2f;
        float startX = camPos.x - (worldWidth / 2f) - buffer;
        float endX = camPos.x + (worldWidth / 2f) + buffer;
        float startZ = camPos.z - (worldHeight / 2f) - buffer;
        float endZ = camPos.z + (worldHeight / 2f) + buffer;

        float minorAlphaFade = 1f - Mathf.InverseLerp(fadeStartZoom, fadeEndZoom, zoom);
        Color currentMinorColor = minorLineColor;
        currentMinorColor.a *= minorAlphaFade;

        float firstX = Mathf.Floor(startX / minorGridSize) * minorGridSize;
        float firstZ = Mathf.Floor(startZ / minorGridSize) * minorGridSize;

        int vertIndex = 0;

        // 3. Generation (Vertical)
        for (float x = firstX; x <= endX; x += minorGridSize)
        {
            int lineIndex = Mathf.RoundToInt(x / minorGridSize);
            bool isMajor = (lineIndex % minorLinesPerMajor == 0);
            Color c = isMajor ? majorLineColor : currentMinorColor;

            if (!isMajor && c.a <= 0.01f) continue;
            AddLine(new Vector3(x, gridAltitude, startZ), new Vector3(x, gridAltitude, endZ), thickness, c, ref vertIndex);
        }

        // 4. Generation (Horizontal)
        for (float z = firstZ; z <= endZ; z += minorGridSize)
        {
            int lineIndex = Mathf.RoundToInt(z / minorGridSize);
            bool isMajor = (lineIndex % minorLinesPerMajor == 0);
            Color c = isMajor ? majorLineColor : currentMinorColor;

            if (!isMajor && c.a <= 0.01f) continue;
            AddLine(new Vector3(startX, gridAltitude, z), new Vector3(endX, gridAltitude, z), thickness, c, ref vertIndex);
        }

        // 5. Apply and RECALCULATE BOUNDS (The "Disappearing" Fix)
        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetIndices(indices.ToArray(), MeshTopology.Quads, 0);

        mesh.RecalculateBounds(); // This tells Unity "Hey, the mesh is actually over here now!"
    }

    void AddLine(Vector3 start, Vector3 end, float thickness, Color color, ref int vertIndex)
    {
        float half = thickness / 2f;

        if (Mathf.Abs(start.x - end.x) < 0.1f) // Vertical line
        {
            vertices.Add(new Vector3(start.x - half, start.y, start.z));
            vertices.Add(new Vector3(start.x + half, start.y, start.z));
            vertices.Add(new Vector3(end.x + half, end.y, end.z));
            vertices.Add(new Vector3(end.x - half, end.y, end.z));
        }
        else // Horizontal line
        {
            vertices.Add(new Vector3(start.x, start.y, start.z - half));
            vertices.Add(new Vector3(start.x, start.y, start.z + half));
            vertices.Add(new Vector3(end.x, end.y, end.z + half));
            vertices.Add(new Vector3(end.x, end.y, end.z - half));
        }

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        indices.Add(vertIndex);
        indices.Add(vertIndex + 1);
        indices.Add(vertIndex + 2);
        indices.Add(vertIndex + 3);

        vertIndex += 4;
    }
}