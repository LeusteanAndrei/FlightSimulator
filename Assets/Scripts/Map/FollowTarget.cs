using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FollowTarget : MonoBehaviour
{
    [Header("Follow")]
    public Transform target;
    public float heightAbovePlayer = 5000f;

    [Header("Zoom")]
    public float orthoSize = 10000f;
    public float zoomSpeed = 2000f;
    public float minZoom = 2000f;
    public float maxZoom = 30000f;

    [Header("Pan")]
    public float panSpeed = 2f;

    [Header("UI")]
    public RawImage minimapUI;

    private Camera cam;

    // Keeps track of manual camera offset while panning
    private Vector3 panOffset;

    // Used for drag panning
    private Vector2 lastMousePosition;

    // When true, camera follows target normally
    // When panning, offset is applied on top
    private bool isPanning;

    // Store the currently outlined object
    private Outline currentOutline;

    void Start()
    {
        cam = GetComponent<Camera>();

        cam.orthographic = true;
        cam.orthographicSize = orthoSize;
        cam.farClipPlane = 50000f;

        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    void Update()
    {
        if (Mouse.current == null)
        {
            return;
        }

        HandleZoom();
        HandlePan();
        HandleHoverOutline();

        // Keep original follow logic
        Vector3 pos = target.position + panOffset;
        pos.y = target.position.y + heightAbovePlayer;

        transform.position = pos;

        // Keep original raycast click logic
        if (Mouse.current.leftButton.wasPressedThisFrame && IsPointerOverMinimapUI())
        {
            HandleMinimapClick();
        }
    }

    void HandleZoom()
    {
        if (!IsPointerOverMinimapUI())
        {
            return;
        }

        float scroll = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            cam.orthographicSize -= scroll * zoomSpeed * Time.unscaledDeltaTime;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }

    void HandlePan()
    {
        // Start panning
        if (Mouse.current.rightButton.wasPressedThisFrame && IsPointerOverMinimapUI())
        {
            isPanning = true;
            lastMousePosition = Mouse.current.position.ReadValue();
        }

        // Stop panning
        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            isPanning = false;
        }

        if (!isPanning)
        {
            return;
        }

        Vector2 currentMousePosition = Mouse.current.position.ReadValue();

        // Convert previous and current mouse positions to world positions
        Vector3 prevWorld =
            cam.ScreenToWorldPoint(new Vector3(
                lastMousePosition.x,
                lastMousePosition.y,
                cam.transform.position.y));

        Vector3 currentWorld =
            cam.ScreenToWorldPoint(new Vector3(
                currentMousePosition.x,
                currentMousePosition.y,
                cam.transform.position.y));

        // Difference between positions
        Vector3 delta = prevWorld - currentWorld;

        // Since camera looks straight down,
        // use X/Z plane only
        panOffset += new Vector3(delta.x, 0f, delta.z) * panSpeed;

        lastMousePosition = currentMousePosition;
    }

    void HandleHoverOutline()
    {
        // Disable outline if not hovering minimap
        if (!IsPointerOverMinimapUI())
        {
            ClearCurrentOutline();
            return;
        }

        Canvas canvas = minimapUI.canvas;

        Camera uiCamera =
            canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Vector2 localMousePos;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapUI.rectTransform,
            mousePosition,
            uiCamera,
            out localMousePos))
        {
            ClearCurrentOutline();
            return;
        }

        Rect rect = minimapUI.rectTransform.rect;

        float normalizedX = (localMousePos.x - rect.x) / rect.width;
        float normalizedY = (localMousePos.y - rect.y) / rect.height;

        Ray ray = cam.ViewportPointToRay(new Vector3(normalizedX, normalizedY, 0f));

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100000f))
        {
            Transform hoveredObject = hit.collider.transform;

            if (hoveredObject.parent != null)
            {
                hoveredObject = hoveredObject.parent;
            }

            // Try to get any component named "Outline"
            Outline outline =
                hoveredObject.GetComponent<Outline>();

            // If same outline already active, do nothing
            if (outline == currentOutline)
            {
                return;
            }

            // Disable previous outline
            ClearCurrentOutline();

            // Enable new outline if found
            if (outline != null)
            {
                outline.enabled = true;
                currentOutline = outline;
            }
        }
        else
        {
            ClearCurrentOutline();
        }
    }

    void ClearCurrentOutline()
    {
        if (currentOutline != null)
        {
            currentOutline.enabled = false;
            currentOutline = null;
        }
    }

    bool IsPointerOverMinimapUI()
    {
        if (minimapUI == null)
        {
            return false;
        }

        Canvas canvas = minimapUI.canvas;

        Camera uiCamera =
            canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        return RectTransformUtility.RectangleContainsScreenPoint(
            minimapUI.rectTransform,
            Mouse.current.position.ReadValue(),
            uiCamera
        );
    }

    void HandleMinimapClick()
    {
        Canvas canvas = minimapUI.canvas;

        Camera uiCamera =
            canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Vector2 localClickPos;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapUI.rectTransform,
            mousePosition,
            uiCamera,
            out localClickPos))
        {
            Debug.Log("Could not convert minimap click to local position");
            return;
        }

        Rect rect = minimapUI.rectTransform.rect;

        float normalizedX = (localClickPos.x - rect.x) / rect.width;
        float normalizedY = (localClickPos.y - rect.y) / rect.height;

        Ray ray = cam.ViewportPointToRay(new Vector3(normalizedX, normalizedY, 0f));

        RaycastHit hit;

        // KEEPING ORIGINAL RAYCAST LOGIC
        if (Physics.Raycast(ray, out hit, 100000f))
        {
            Transform clickedObject = hit.collider.transform;

            if (clickedObject.parent != null)
            {
                clickedObject = clickedObject.parent;
            }

            Debug.Log("Clicked planet: " + clickedObject.name);
        }
    }
}