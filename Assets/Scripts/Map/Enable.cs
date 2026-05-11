using UnityEngine;
using UnityEngine.InputSystem;
public class Enable : MonoBehaviour
{
    [SerializeField] private InputAction toggleAction;
    private float previousTimeScale = 1f;
    void Start()
    {
        this.gameObject.SetActive(false);
        SetCursorState(false);
    }
    private void OnEnable()
    {
        toggleAction.Enable();
        toggleAction.performed += Toggle;
    }

    private void OnDisable()
    {
        toggleAction.performed -= Toggle;
        toggleAction.Disable();
    }

    public void Toggle(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        bool shouldEnable = !gameObject.activeSelf;
        if (shouldEnable)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = previousTimeScale;
        }

        gameObject.SetActive(shouldEnable);
        SetCursorState(shouldEnable);
    }

    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
