using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FollowTarget : MonoBehaviour
{
    public Transform target;
    public float heightAbovePlayer = 5000f;
    public float orthoSize = 10000f;
    public RawImage minimapUI;
    private Camera cam;

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
        Vector3 pos = target.position;
        pos.y = target.position.y + heightAbovePlayer;
        transform.position = pos;

        if (Mouse.current == null)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && IsPointerOverMinimapUI())
        {
            HandleMinimapClick();
        }
    }

    bool IsPointerOverMinimapUI()
    {
        if (minimapUI == null) return false;

        Canvas canvas = minimapUI.canvas;
        Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        return RectTransformUtility.RectangleContainsScreenPoint(minimapUI.rectTransform, Mouse.current.position.ReadValue(), uiCamera);
    }

    void HandleMinimapClick()
    {
        Canvas canvas = minimapUI.canvas;
        Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Vector2 localClickPos;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapUI.rectTransform, mousePosition, uiCamera, out localClickPos))
        {
            Debug.Log("Could not convert minimap click to local position");
            return;
        }

        Rect rect = minimapUI.rectTransform.rect;
        float normalizedX = (localClickPos.x - rect.x) / rect.width;
        float normalizedY = (localClickPos.y - rect.y) / rect.height;

        Ray ray = cam.ViewportPointToRay(new Vector3(normalizedX, normalizedY, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100000f))
        {
            Transform clickedObject = hit.collider.transform;

            if (clickedObject.parent != null)
            {
                clickedObject = clickedObject.parent;
            }

            Debug.Log("Clicked planet: " + clickedObject.name);
        }
        else
        {
            //Debug.Log("No planet hit under minimap click");
        }
    }
}
