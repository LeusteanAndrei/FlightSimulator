using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RuntimeGridMesh : MonoBehaviour
{
    [Header("References")]
    public Camera targetCamera;

    [Header("Grid Size")]
    [Tooltip("Size of smallest grid square.")]
    public float minorGridSize = 500f;

    [Tooltip("How many minor cells fit inside one major cell.")]
    public int minorLinesPerMajor = 5;

    [Header("Appearance")]
    public float gridAltitude = 5f;

    [Tooltip("Constant thickness in screen pixels.")]
    public float lineThicknessPixels = 2f;

    public Color majorLineColor = new Color(1f, 1f, 1f, 0.4f);
    public Color minorLineColor = new Color(1f, 1f, 1f, 0.15f);

    [Header("Minor Grid Fade")]
    public float fadeStartZoom = 5000f;
    public float fadeEndZoom = 15000f;

    [Header("Performance")]
    [Tooltip("Rebuild every frame. Disable only if profiling says necessary.")]
    public bool rebuildEveryFrame = true;

    [Tooltip("Distance camera must move before rebuild.")]
    public float movementThreshold = 5f;

    [Tooltip("Zoom delta before rebuild.")]
    public float zoomThreshold = 5f;

    private Mesh mesh;

    private readonly List<Vector3> vertices = new();
    private readonly List<Color> colors = new();
    private readonly List<int> indices = new();

    private Vector3 lastCamPos;
    private float lastZoom;

    void Awake()
    {
        MeshFilter filter = GetComponent<MeshFilter>();

        mesh = new Mesh();
        mesh.name = "Runtime Grid Mesh";

        // IMPORTANT:
        // Keeps mesh from exploding at high zoom distances.
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        filter.sharedMesh = mesh;

        MeshRenderer rend = GetComponent<MeshRenderer>();

        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rend.receiveShadows = false;
        rend.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;

        if (rend.sharedMaterial == null)
        {
            Material mat = new Material(Shader.Find("Sprites/Default"));
            rend.sharedMaterial = mat;
        }
    }

    void LateUpdate()
    {
        if (targetCamera == null)
        {
            return;
        }

        Vector3 camPos = targetCamera.transform.position;
        float zoom = targetCamera.orthographicSize;

        bool shouldRebuild =
            rebuildEveryFrame ||
            Vector3.Distance(camPos, lastCamPos) > movementThreshold ||
            Mathf.Abs(zoom - lastZoom) > zoomThreshold;

        if (!shouldRebuild)
        {
            return;
        }

        lastCamPos = camPos;
        lastZoom = zoom;

        RebuildGrid(camPos, zoom);
    }

    void RebuildGrid(Vector3 camPos, float zoom)
    {
        mesh.Clear(false);

        vertices.Clear();
        colors.Clear();
        indices.Clear();

        // Keep mesh transform centered near camera.
        // This prevents floating precision issues.
        transform.position = new Vector3(camPos.x, 0f, camPos.z);
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        float worldHeight = zoom * 2f;
        float worldWidth = worldHeight * targetCamera.aspect;

        float screenHeight =
            targetCamera.pixelHeight > 0
            ? targetCamera.pixelHeight
            : 1080f;

        // Constant pixel thickness
        float thickness =
            (lineThicknessPixels / screenHeight) * worldHeight;

        float buffer = minorGridSize * 2f;

        float startX = camPos.x - (worldWidth * 0.5f) - buffer;
        float endX = camPos.x + (worldWidth * 0.5f) + buffer;

        float startZ = camPos.z - (worldHeight * 0.5f) - buffer;
        float endZ = camPos.z + (worldHeight * 0.5f) + buffer;

        float firstX =
            Mathf.Floor(startX / minorGridSize) * minorGridSize;

        float firstZ =
            Mathf.Floor(startZ / minorGridSize) * minorGridSize;

        // Fade minor lines when zooming out
        float minorFade =
            1f - Mathf.InverseLerp(fadeStartZoom, fadeEndZoom, zoom);

        Color currentMinorColor = minorLineColor;
        currentMinorColor.a *= minorFade;

        int vertIndex = 0;

        // VERTICAL LINES
        for (float x = firstX; x <= endX; x += minorGridSize)
        {
            int lineIndex = Mathf.RoundToInt(x / minorGridSize);

            bool isMajor =
                lineIndex % minorLinesPerMajor == 0;

            Color c = isMajor
                ? majorLineColor
                : currentMinorColor;

            if (!isMajor && c.a <= 0.001f)
            {
                continue;
            }

            Vector3 start =
                new Vector3(
                    x - camPos.x,
                    gridAltitude,
                    startZ - camPos.z);

            Vector3 end =
                new Vector3(
                    x - camPos.x,
                    gridAltitude,
                    endZ - camPos.z);

            AddLine(start, end, thickness, c, ref vertIndex);
        }

        // HORIZONTAL LINES
        for (float z = firstZ; z <= endZ; z += minorGridSize)
        {
            int lineIndex = Mathf.RoundToInt(z / minorGridSize);

            bool isMajor =
                lineIndex % minorLinesPerMajor == 0;

            Color c = isMajor
                ? majorLineColor
                : currentMinorColor;

            if (!isMajor && c.a <= 0.001f)
            {
                continue;
            }

            Vector3 start =
                new Vector3(
                    startX - camPos.x,
                    gridAltitude,
                    z - camPos.z);

            Vector3 end =
                new Vector3(
                    endX - camPos.x,
                    gridAltitude,
                    z - camPos.z);

            AddLine(start, end, thickness, c, ref vertIndex);
        }

        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetIndices(indices, MeshTopology.Quads, 0);

        // Prevent frustum clipping
        Bounds bounds = new Bounds(
            Vector3.zero,
            new Vector3(
                worldWidth + buffer * 2f,
                10000f,
                worldHeight + buffer * 2f));

        mesh.bounds = bounds;
    }

    void AddLine(
        Vector3 start,
        Vector3 end,
        float thickness,
        Color color,
        ref int vertIndex)
    {
        float half = thickness * 0.5f;

        bool vertical =
            Mathf.Abs(start.x - end.x) < 0.01f;

        if (vertical)
        {
            vertices.Add(new Vector3(start.x - half, start.y, start.z));
            vertices.Add(new Vector3(start.x + half, start.y, start.z));
            vertices.Add(new Vector3(end.x + half, end.y, end.z));
            vertices.Add(new Vector3(end.x - half, end.y, end.z));
        }
        else
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

        indices.Add(vertIndex + 0);
        indices.Add(vertIndex + 1);
        indices.Add(vertIndex + 2);
        indices.Add(vertIndex + 3);

        vertIndex += 4;
    }
}